using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VolanteStudio {
    public static class Program {
        [STAThread] public static void Main(string[] args) {
            bool ownsMutex;
            using(var mutex=new System.Threading.Mutex(true,"Local\\VolanteStudio.Desktop",out ownsMutex)) {
                if(!ownsMutex) { MessageBox.Show("O Volante Studio já está aberto.","Volante Studio");return; }
                try {
                    var app=new Application { ShutdownMode=ShutdownMode.OnMainWindowClose };
                    app.DispatcherUnhandledException+=(s,e)=>{ WriteError(e.Exception); MessageBox.Show("Não foi possível concluir a ação. "+EmcBridge.Unwrap(e.Exception).Message,"Volante Studio");e.Handled=true; };
                    Window window;
                    using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("MainWindow.xaml")) window=(Window)XamlReader.Load(stream);
                    var controller=new StudioController(window,args.Contains("--demo"));
                    app.Run(window);
                    GC.KeepAlive(controller);
                } catch(Exception ex) { WriteError(ex);MessageBox.Show(EmcBridge.Unwrap(ex).Message,"Volante Studio — erro ao abrir");Environment.ExitCode=1; }
            }
        }
        public static void WriteError(Exception ex) {
            try { var dir=System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data");Directory.CreateDirectory(dir);File.AppendAllText(System.IO.Path.Combine(dir,"errors.log"),DateTime.Now.ToString("s")+" "+ex+Environment.NewLine); } catch { }
        }
    }
    public sealed class StudioController {
        readonly Window window;
        readonly EmcBridge bridge=new EmcBridge();
        readonly ProfileStore profiles;
        readonly Dictionary<string,int> values=SettingsCatalog.Defaults();
        readonly Dictionary<string,List<Slider>> sliders=new Dictionary<string,List<Slider>>();
        readonly Dictionary<string,List<TextBlock>> labels=new Dictionary<string,List<TextBlock>>();
        readonly HashSet<string> dirty=new HashSet<string>();
        readonly List<Button> usbButtons=new List<Button>();
        readonly List<FrameworkElement> editControls=new List<FrameworkElement>();
        readonly List<PedalView> pedals=new List<PedalView>();
        readonly WheelView wheel=new WheelView { Width=400,Height=400 };
        readonly DispatcherTimer timer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(33) };
        readonly Stopwatch clock=Stopwatch.StartNew();
        bool demo, busy, loading, lastReady;
        int tick;
        double wheelAngle;
        string profileName="Personalizado";
        public StudioController(Window window,bool demo) {
            this.window=window;this.demo=demo;
            window.Width=Math.Min(1320,Math.Max(window.MinWidth,SystemParameters.WorkArea.Width-40));
            window.Height=Math.Min(840,Math.Max(window.MinHeight,SystemParameters.WorkArea.Height-40));
            profiles=new ProfileStore(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data","Profiles"));
            Find<Grid>("WheelHost").Children.Add(wheel);
            AddSlider("AngleControls","Angle");
            AddSlider("ForceControls","Maxforce");AddSlider("ForceControls","Minforce");AddSlider("ForceControls","GetDamper");
            AddSlider("StopControls","GetSoftlock");
            AddSlider("EffectControls","GetDamper");AddSlider("EffectControls","GetFriction");AddSlider("EffectControls","GetInertia");
            AddPedal("Acelerador","Throttle",-32767,32767,"Gasmin","Gasmax","#248CFF");
            AddPedal("Freio","Brake",0,1024,"BrakeMin","BrakeMax","#248CFF");
            AddPedal("Embreagem","Clutch",0,1024,"ClutchMin","ClutchMax","#248CFF");
            AddPedal("Freio de mão","Handbrake",0,1024,"HandbrakeMin","HandbrakeMax","#248CFF");
            AddOptionalInputs();
            Click("NavHome",()=>Navigate("Home"));Click("NavEffects",()=>Navigate("Effects"));Click("NavProfiles",()=>{RefreshProfiles();Navigate("Profiles");});Click("NavDevice",()=>Navigate("Device"));
            NativeWindowTheme.Apply(window);
            ResponsiveLayout.Apply(window);
            Click("DemoToggle",()=>{ this.demo=!this.demo;lastReady=false;UpdateDemo();SetStatus(this.demo?"Demonstração visual":"Modo USB",this.demo?"Movimentos simulados. Nenhum ajuste é enviado ao volante.":"Aguardando leitura do dispositivo."); });
            Click("WindowsTest",OpenWindowsTest);Click("WindowsTestDevice",OpenWindowsTest);
            Find<Button>("CenterButton").Click+=async(s,e)=>await SendCommand("DoSetCenter",null,"Centralização enviada");
            Find<Button>("ApplyButton").Click+=async(s,e)=>await Apply();
            Find<Button>("ApplyHardware").Click+=async(s,e)=>await ApplyHardware();
            Click("FooterProfile",()=>{Find<TextBox>("ProfileName").Text=profileName=="Personalizado"?"Meu volante":profileName;RefreshProfiles();Navigate("Profiles");});
            Click("SaveProfile",SaveProfile);Click("LoadProfile",LoadProfile);
            usbButtons.Add(Find<Button>("CenterButton"));usbButtons.Add(Find<Button>("ApplyHardware"));
            editControls.Add(Find<TextBox>("CprText"));editControls.Add(Find<ListBox>("BridgeMode"));editControls.Add(Find<Button>("LoadProfile"));editControls.Add(Find<Button>("DemoToggle"));
            window.Loaded+=(s,e)=>{
                // Initialize only after WPF has a real window and dispatcher, as required by the original HID library.
                bridge.Open(AppDomain.CurrentDomain.BaseDirectory);
                if(bridge.Error!=null)SetStatus("Comunicação USB indisponível",bridge.Error);
                timer.Start();
            };
            window.Closing+=(s,e)=>{if(busy){e.Cancel=true;SetStatus("Envio em andamento","Aguarde a conclusão antes de fechar o aplicativo.");}};
            window.Closed+=(s,e)=>{timer.Stop();bridge.Dispose();};
            timer.Tick+=(s,e)=>UpdateLive();
            Navigate("Home");UpdateDemo();RefreshProfiles();UpdateEnabled();
            if(demo)SetStatus("Demonstração visual","Movimentos simulados. Nenhum ajuste é enviado ao volante.");
        }
        T Find<T>(string name) where T:class { return (T)window.FindName(name); }
        static Brush Brush(string color) { return (Brush)new BrushConverter().ConvertFromString(color); }
        void Click(string name,Action action) { Find<Button>(name).Click+=(s,e)=>action(); }
        void Navigate(string page) {
            foreach(var p in new[]{"Home","Effects","Profiles","Device"}) {
                Find<Grid>(p+"Page").Visibility=p==page?Visibility.Visible:Visibility.Collapsed;
                var b=Find<Button>("Nav"+p);b.Background=Brush(p==page?"#163456":"#00000000");b.Foreground=Brush(p==page?"#65B0FF":"#728196");
            }
            Find<TextBlock>("PageTitle").Text=page=="Home"?"Visão geral":page=="Effects"?"Efeitos de força":page=="Profiles"?"Perfis":"Configuração do dispositivo";
        }
        void AddSlider(string panel,string key) {
            var spec=SettingsCatalog.Find(key);var stack=new StackPanel { Margin=new Thickness(0,0,0,panel=="EffectControls"?30:15) };
            var heading=new Grid();heading.ColumnDefinitions.Add(new ColumnDefinition());heading.ColumnDefinitions.Add(new ColumnDefinition {Width=GridLength.Auto});
            var label=new TextBlock { Text=spec.Label,FontSize=12,Foreground=Brush("#B9C7D8"),ToolTip=spec.Help };
            var value=new TextBlock { Text=values[key]+spec.Unit,FontSize=12,FontWeight=FontWeights.SemiBold,Foreground=Brush("#69ADFF") };Grid.SetColumn(value,1);heading.Children.Add(label);heading.Children.Add(value);stack.Children.Add(heading);
            var slider=new Slider { Minimum=spec.Min,Maximum=spec.Max,Value=values[key],Margin=new Thickness(0,8,0,0),ToolTip=spec.Help,SmallChange=1,LargeChange=key=="Angle"?90:10 };
            System.Windows.Automation.AutomationProperties.SetName(slider,spec.Label);
            if(!sliders.ContainsKey(key)){sliders[key]=new List<Slider>();labels[key]=new List<TextBlock>();}
            sliders[key].Add(slider);labels[key].Add(value);editControls.Add(slider);
            slider.ValueChanged+=(s,e)=>{if(loading)return;int v=(int)Math.Round(slider.Value);SetValue(key,v);dirty.Add(key);profileName="Personalizado";SetStatus("Ajustes preparados",dirty.Count+" ajuste(s) pendente(s). Aplique para enviar ao volante.");UpdateEnabled();};
            stack.Children.Add(slider);
            if(panel=="EffectControls")stack.Children.Add(new TextBlock {Text=spec.Help,Foreground=Brush("#70839B"),FontSize=11,Margin=new Thickness(0,4,0,0)});
            if(key=="Angle") {
                var presets=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,4,0,0) };
                foreach(int angle in new[]{180,360,540,900,1080}) {int a=angle;var b=new Button { Content=a+"°",Style=(Style)window.Resources["TinyButton"],Margin=new Thickness(2,0,2,0),Padding=new Thickness(8,3,8,3) };b.Click+=(s,e)=>{SetValue("Angle",a);dirty.Add("Angle");SetStatus("Ângulo preparado",a+"° — clique em Aplicar ao volante.");UpdateEnabled();};presets.Children.Add(b);editControls.Add(b);}
                stack.Children.Add(presets);
            }
            Find<StackPanel>(panel).Children.Add(stack);
        }
        void SetValue(string key,int value) {
            var spec=SettingsCatalog.Find(key);value=Math.Max(spec.Min,Math.Min(spec.Max,value));
            loading=true;values[key]=value;
            foreach(var slider in sliders[key])slider.Value=value;
            foreach(var label in labels[key])label.Text=value+spec.Unit;
            loading=false;
        }
        sealed class PedalView { public string Key;public int Min,Max;public Border Fill;public TextBlock Value;public Grid Track; }
        void AddPedal(string title,string key,int min,int max,string minCommand,string maxCommand,string color) {
            var stack=new StackPanel { Margin=new Thickness(0,0,0,20) };
            var grid=new Grid();var label=new TextBlock {Text=title,FontSize=12,Foreground=Brush("#C9D3E0")};var value=new TextBlock {Text="0%",Foreground=Brush("#76AEF2"),FontSize=11,HorizontalAlignment=HorizontalAlignment.Right};grid.Children.Add(label);grid.Children.Add(value);stack.Children.Add(grid);
            var track=new Grid { Height=5,Margin=new Thickness(0,11,0,10),Background=Brush("#242E3C"),ClipToBounds=true };
            var fill=new Border {Width=0,Background=Brush(color),CornerRadius=new CornerRadius(2),HorizontalAlignment=HorizontalAlignment.Left};track.Children.Add(fill);stack.Children.Add(track);
            var buttons=new Grid();buttons.ColumnDefinitions.Add(new ColumnDefinition());buttons.ColumnDefinitions.Add(new ColumnDefinition());
            foreach(var pair in new[]{new[]{"Definir mín.",minCommand},new[]{"Definir máx.",maxCommand}}) {
                string cmd=pair[1];var b=new Button {Content=pair[0],Style=(Style)window.Resources["TinyButton"],HorizontalAlignment=pair[0].Contains("mín")?HorizontalAlignment.Left:HorizontalAlignment.Right};
                Grid.SetColumn(b,pair[0].Contains("mín")?0:1);b.Click+=async(s,e)=>await SendCommand("DoPedalCommand",cmd,title+": calibração enviada");buttons.Children.Add(b);usbButtons.Add(b);
                System.Windows.Automation.AutomationProperties.SetName(b,title+" "+pair[0]);
            }
            stack.Children.Add(buttons);Find<StackPanel>("PedalControls").Children.Add(stack);
            pedals.Add(new PedalView {Key=key,Min=min,Max=max,Fill=fill,Track=track,Value=value});
        }
        void UpdateDemo() {
            var demoButton=Find<Button>("DemoToggle");
            demoButton.Content=demo?"Demonstração ativa • Sair":"Explorar demonstração";
            demoButton.Background=Brush(demo?"#654619":"#142F4D");
            demoButton.BorderBrush=Brush(demo?"#F3BA55":"#448DDA");
            demoButton.Foreground=Brush(demo?"#FFE2A8":"#D5EAFF");
            demoButton.FontWeight=FontWeights.SemiBold;
            demoButton.ToolTip=demo?"Modo simulado ativo. Clique para voltar ao dispositivo USB.":"Ativar movimentos simulados sem enviar ajustes ao volante.";
            Find<TextBlock>("LiveLabel").Text=demo?"MOVIMENTO SIMULADO":"POSIÇÃO DO VOLANTE";
            Find<TextBlock>("PedalsBadge").Text=demo?"SIMULADO":"AO VIVO";
            Find<TextBlock>("ContextHint").Text=demo?"Demonstração visual • dados simulados • USB sem alterações":"Configuração via USB";
            Find<TextBlock>("ContextHint").Foreground=Brush(demo?"#C8A564":"#6F7F93");UpdateEnabled();
        }
        void AddOptionalInputs() {
            var panel=Find<StackPanel>("OptionalInputs");
            panel.Children.Add(new TextBlock {Text="Entradas opcionais",FontSize=14,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,14)});
            var selectors=new List<ListBox>();
            foreach(string name in new[]{"Embreagem","Freio de mão"}) {
                var row=new DockPanel {Margin=new Thickness(0,0,0,9)};
                var label=new TextBlock {Text=name,FontSize=12,Width=110,VerticalAlignment=VerticalAlignment.Center,Foreground=Brush("#9AAABE")};row.Children.Add(label);
                var list=new ListBox {BorderThickness=new Thickness(0),Background=Brush("#00000000"),SelectedIndex=0};
                var factory=new FrameworkElementFactory(typeof(StackPanel));factory.SetValue(StackPanel.OrientationProperty,Orientation.Horizontal);list.ItemsPanel=new ItemsPanelTemplate(factory);
                foreach(string option in new[]{"Manter","Ativar","Desativar"})list.Items.Add(new ListBoxItem {Content=option,FontSize=10,Padding=new Thickness(10,6,10,6),Margin=new Thickness(2,0,2,0)});
                list.SelectedIndex=0;row.Children.Add(list);panel.Children.Add(row);selectors.Add(list);editControls.Add(list);
                System.Windows.Automation.AutomationProperties.SetName(list,name+" — ativação");
            }
            var apply=new Button {Content="Aplicar entradas opcionais",Padding=new Thickness(12,8,12,8),Margin=new Thickness(0,5,0,0)};
            panel.Children.Add(apply);usbButtons.Add(apply);
            apply.Click+=async(s,e)=>{
                if(busy||demo||!bridge.CanConfigure)return;
                if(selectors.All(l=>l.SelectedIndex==0)){SetStatus("Nenhuma alteração nas entradas","Escolha Ativar ou Desativar para uma das entradas.");return;}
                busy=true;UpdateEnabled();
                try {
                    for(int i=0;i<2;i++) {
                        int selected=selectors[i].SelectedIndex;if(selected==0)continue;
                        string property=i==0?"DisableClutch":"DisableHandbrake";bool disable=selected==2;
                        byte op=(byte)(i==0?(disable?0x0D:0x0E):(disable?0x0F:0x10));
                        await Transmit(()=>bridge.Set(property,disable),new TransferExpectation(0x90,op));selectors[i].SelectedIndex=0;
                    }
                    SetStatus("Entradas configuradas","Os comandos foram transmitidos por USB.");
                }catch(Exception ex){SetStatus("Configuração não concluída",EmcBridge.Unwrap(ex).Message);Program.WriteError(ex);}
                finally{busy=false;UpdateEnabled();}
            };
        }
        void UpdateLive() {
            try {
                bool connected=bridge.Connected,ready=bridge.CanConfigure;
                double time=clock.Elapsed.TotalSeconds;
                double target=demo?Math.Sin(time*0.55)*110:connected?bridge.SteeringPosition*values["Angle"]/2.0:0;
                if(Math.Abs(target-wheelAngle)>0.02) {wheelAngle+=(target-wheelAngle)*0.4;wheel.Angle=wheelAngle;wheel.InvalidateVisual();}
                Find<TextBlock>("AngleValue").Text=Math.Round(demo?wheelAngle:target).ToString("0",CultureInfo.InvariantCulture);
                for(int i=0;i<pedals.Count;i++) {
                    var p=pedals[i];double percent=demo?Math.Max(0,Math.Sin(time*0.65+i*1.3))*(i==3?35:100):connected?InputMath.Percent(bridge.ReadInt(p.Key),p.Min,p.Max):0;
                    p.Fill.Width=p.Track.ActualWidth*percent/100;p.Value.Text=Math.Round(percent)+"%";
                }
                if(++tick%10!=0)return;
                Find<TextBlock>("ConnectionLabel").Text=demo?"DEMONSTRAÇÃO":ready?"USB CONECTADO":connected?"VERIFICANDO FIRMWARE":"USB DESCONECTADO";
                Find<Ellipse>("ConnectionDot").Fill=Brush(demo?"#E8B65E":ready?"#4BC99B":"#718096");
                Find<TextBlock>("DeviceInfo").Text=connected?"Conectado via USB • Firmware "+bridge.Firmware+(ready?"":"\nAguardando a validação da biblioteca EMC."):bridge.Error??"Nenhum volante detectado. Conecte o Leonardo pelo USB. A conexão é reconhecida automaticamente.";
                if(ready&&!lastReady&&!demo) {
                    if(dirty.Count==0)SyncFromDevice();
                    SetStatus("Volante conectado",dirty.Count==0?"Leitura USB ativa. Pronto para ajustar.":"Existem ajustes preparados. Aplique quando desejar.");
                }
                if(!ready&&lastReady&&!demo)SetStatus("Volante desconectado","Os ajustes preparados foram mantidos. Reconecte para continuar.");
                lastReady=ready&&!demo;UpdateEnabled();
            }catch(Exception ex){timer.Stop();SetStatus("Falha na comunicação",EmcBridge.Unwrap(ex).Message);Program.WriteError(ex);foreach(var b in usbButtons)b.IsEnabled=false;Find<Button>("ApplyButton").IsEnabled=false;}
        }
        void SyncFromDevice() {
            // Vendor getters initially contain local zeros, not settings read from EEPROM.
            // Do not overwrite the prepared UI values or hardware fields with those defaults.
            if(bridge.ReadInt("Angle")<30)return;
            foreach(var spec in SettingsCatalog.All) {int raw=bridge.ReadInt(spec.Key);int v=raw/spec.Scale;if(v>=spec.Min&&v<=spec.Max)SetValue(spec.Key,v);}
            Find<TextBox>("CprText").Text=bridge.ReadString("GetSetCpr");
            string mode=bridge.ReadString("HbridgeMode");foreach(ListBoxItem item in Find<ListBox>("BridgeMode").Items)if((string)item.Content==mode)item.IsSelected=true;
        }
        void UpdateEnabled() {
            bool ready=!demo&&!busy&&bridge.CanConfigure;
            foreach(var b in usbButtons)b.IsEnabled=ready;
            foreach(var c in editControls)c.IsEnabled=!busy;
            Find<Button>("ApplyButton").IsEnabled=ready&&dirty.Count>0;
            Find<Button>("ApplyButton").Content=busy?"Enviando…":"Aplicar ao volante  →";
        }
        void SetStatus(string title,string detail) { Find<TextBlock>("StatusTitle").Text=title;Find<TextBlock>("StatusDetail").Text=detail;Find<TextBlock>("StatusDetail").ToolTip=detail; }
        async Task Transmit(Action operation,TransferExpectation expected) {
            if(demo||!bridge.CanConfigure)throw new InvalidOperationException("Conecte um volante compatível e saia da demonstração.");
            var completion=new TaskCompletionSource<bool>();Action<byte[]> sent=bytes=>{if(expected.Matches(bytes))completion.TrySetResult(true);};bridge.PacketSent+=sent;
            try {
                operation();
                var done=await Task.WhenAny(completion.Task,Task.Delay(2500));
                if(done!=completion.Task)throw new TimeoutException("O envio USB não foi confirmado. Verifique a conexão e tente novamente.");
                // This is a successful USB transfer, not an EEPROM/read-back acknowledgement.
                await Task.Delay(100);
                if(!bridge.CanConfigure)throw new IOException("O volante foi desconectado durante o envio.");
            } finally { bridge.PacketSent-=sent; }
        }
        async Task Apply() {
            if(busy||demo||dirty.Count==0||!bridge.CanConfigure)return;
            if(values["Minforce"]>values["Maxforce"]) { SetStatus("Revise os ajustes","A força mínima não pode ser maior que a intensidade.");return; }
            busy=true;UpdateEnabled();int sent=0;
            var keys=SettingsCatalog.All.Where(s=>dirty.Contains(s.Key)).ToList();
            try {
                foreach(var spec in keys) {
                    SetStatus("Aplicando ajustes",spec.Label+"…");
                    int raw=spec.ToDevice(values[spec.Key]);
                    await Transmit(()=>bridge.Set(spec.Key,raw),TransferExpectation.Setting(spec.Key,raw));
                    dirty.Remove(spec.Key);sent++;
                }
                SetStatus("Comandos enviados ao volante",sent+" ajuste(s) enviado(s) por USB. Confira a resposta no volante.");
            } catch(Exception ex) { SetStatus("Envio interrompido",sent+" ajuste(s) enviado(s). "+EmcBridge.Unwrap(ex).Message);Program.WriteError(ex); }
            finally {busy=false;UpdateEnabled();}
        }
        async Task SendCommand(string command,object parameter,string message) {
            if(busy||demo||!bridge.CanConfigure)return;busy=true;UpdateEnabled();
            try {await Transmit(()=>bridge.Command(command,parameter),TransferExpectation.Command((string)parameter));SetStatus(message,"Comando transmitido por USB.");}
            catch(Exception ex){SetStatus("Não foi possível enviar",EmcBridge.Unwrap(ex).Message);Program.WriteError(ex);}
            finally{busy=false;UpdateEnabled();}
        }
        async Task ApplyHardware() {
            if(busy||demo||!bridge.CanConfigure)return;
            ushort cpr;var item=Find<ListBox>("BridgeMode").SelectedItem as ListBoxItem;
            if(!ushort.TryParse(Find<TextBox>("CprText").Text,out cpr)||cpr<1||item==null){SetStatus("Revise o hardware","Informe um CPR entre 1 e 65535 e selecione a ponte H.");return;}
            busy=true;UpdateEnabled();
            try {await Transmit(()=>bridge.Set("GetSetCpr",cpr.ToString(CultureInfo.InvariantCulture)),TransferExpectation.Setting("GetSetCpr",cpr));await Transmit(()=>bridge.Set("HbridgeMode",Convert.ToString(item.Content)),TransferExpectation.Bridge(Convert.ToString(item.Content)));SetStatus("Configuração enviada","CPR e modo da ponte H transmitidos por USB.");}
            catch(Exception ex){SetStatus("Configuração não concluída",EmcBridge.Unwrap(ex).Message);Program.WriteError(ex);}
            finally{busy=false;UpdateEnabled();}
        }
        void RefreshProfiles() {
            try {Find<ListBox>("ProfileList").ItemsSource=profiles.Load();}
            catch(Exception ex){SetStatus("Não foi possível ler os perfis",ex.Message);Program.WriteError(ex);}
        }
        void SaveProfile() {
            try {
                string name=Find<TextBox>("ProfileName").Text.Trim();
                if(values["Minforce"]>values["Maxforce"])throw new ArgumentException("A força mínima não pode ser maior que a intensidade.");
                profiles.Save(new Profile {Name=name,Values=new Dictionary<string,int>(values)});profileName=name;RefreshProfiles();SetStatus("Perfil salvo",name+" — salvo neste computador.");
            }catch(Exception ex){SetStatus("Não foi possível salvar",ex.Message);Program.WriteError(ex);}
        }
        void LoadProfile() {
            var p=Find<ListBox>("ProfileList").SelectedItem as Profile;
            if(p==null){SetStatus("Selecione um perfil","Escolha um perfil da lista para carregar.");return;}
            foreach(var spec in SettingsCatalog.All){SetValue(spec.Key,p.Values[spec.Key]);dirty.Add(spec.Key);}profileName=p.Name;Find<TextBox>("ProfileName").Text=p.Name;SetStatus("Perfil carregado",p.Name+" — clique em Aplicar ao volante para enviar.");UpdateEnabled();Navigate("Home");
        }
        void OpenWindowsTest() {
            try { Process.Start(new ProcessStartInfo("control.exe","joy.cpl") { UseShellExecute=true }); }
            catch(Exception ex){SetStatus("Não foi possível abrir o painel",ex.Message);}
        }
    }
}
