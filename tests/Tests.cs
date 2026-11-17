using System;
using System.IO;
using System.Linq;
namespace VolanteStudio {
 public static class Tests {
  static int count;
  static void Check(bool condition,string name) {if(!condition)throw new Exception(name);Console.WriteLine("PASS "+name);count++;}
  [STAThread] public static int Main() {
   string temp=Path.Combine(Path.GetTempPath(),"VolanteStudioTests-"+Guid.NewGuid().ToString("N"));
   try {
    Check(SettingsCatalog.Find("GetDamper").ToDevice(50)==100,"Damper matches original 0..200 scale");
    Check(SettingsCatalog.Find("GetFriction").ToDevice(100)==200,"Friction maximum matches original");
    Check(SettingsCatalog.Find("GetSoftlock").ToDevice(100)==100,"Soft lock keeps original scale");
    var angleReceipt=TransferExpectation.Setting("Angle",900);
    Check(angleReceipt.Matches(new byte[]{0xEE,0x02,0x03,0x84}),"Angle receipt matches original packet builder");
    Check(!angleReceipt.Matches(new byte[]{0xFF,0xFF,0xFF,0xFF}),"Idle traffic cannot falsely confirm an adjustment");
    Check(!angleReceipt.Matches(new byte[]{0xEE,0x02,0x02,0x1C}),"Old angle cannot confirm a new value");
    Check(!TransferExpectation.Command("ClutchMin").Matches(new byte[]{0x90,0x0D}),"Input toggles cannot confirm pedal calibration");
    bool rejected=false;try{SettingsCatalog.Find("Angle").ToDevice(1081);}catch(ArgumentOutOfRangeException){rejected=true;}
    Check(rejected,"Out-of-range angle rejected before USB");
    Check(InputMath.Percent(-32767,-32767,32767)==0&&InputMath.Percent(32767,-32767,32767)==100,"Signed throttle normalization");
    Check(InputMath.Percent(512,0,1024)==50&&InputMath.Percent(9999,0,1024)==100,"Pedals normalize and clamp");
    var store=new ProfileStore(temp);var p=new Profile{Name="Teste / corrida .. Á"};p.Values["Angle"]=540;store.Save(p);
    Check(store.Load().Single().Values["Angle"]==540,"Profile round trip with Unicode and path separators");
    Check(Directory.GetFiles(temp).Length==1,"Profile name cannot escape data folder");
    p.Values["Angle"]=900;store.Save(p);Check(store.Load().Single().Values["Angle"]==900,"Atomic profile replacement");
    var file=Directory.GetFiles(temp,"*.xml").Single();File.WriteAllText(file,"<profile name='Invalid'><setting key='Angle' value='99999'/></profile>");
    rejected=false;try{store.Load();}catch(ArgumentOutOfRangeException){rejected=true;}Check(rejected,"Invalid stored profile rejected");
    using(var bridge=new EmcBridge()) {rejected=false;try{bridge.Set("Angle",900);}catch(InvalidOperationException){rejected=true;}Check(rejected,"Disconnected device cannot receive settings");}
    var app=new System.Windows.Application();
    using(var stream=typeof(Tests).Assembly.GetManifestResourceStream("MainWindow.xaml")) {
     var window=(System.Windows.Window)System.Windows.Markup.XamlReader.Load(stream);
     var controller=new StudioController(window,true);
     Check(!((System.Windows.Controls.Button)window.FindName("ApplyButton")).IsEnabled,"Demo cannot apply USB settings");
     Check(((System.Windows.Controls.Grid)window.FindName("WheelHost")).Children.Count==1,"Native interface and steering wheel construct successfully");
     Check(window.WindowStyle==System.Windows.WindowStyle.SingleBorderWindow,"System caption provides native window controls");
     var viewport=window.Content as System.Windows.Controls.Border;
     Check(viewport!=null,"Window uses fluid content without a fixed canvas");
     foreach(var size in new[]{new System.Windows.Size(1920,1080),new System.Windows.Size(2560,1080),new System.Windows.Size(960,620)}) {
      viewport.Measure(size);viewport.Arrange(new System.Windows.Rect(size));viewport.UpdateLayout();
      var surface=(System.Windows.FrameworkElement)viewport.Child;
      Check(surface.ActualWidth>=size.Width-20 && surface.ActualHeight>=size.Height-20,"Content fills viewport at "+size);
     }
     var home=(System.Windows.Controls.Grid)window.FindName("HomePage");
     Check(home.ColumnDefinitions[2].Width.Value==0,"Narrow window reflows home panels into two rows");
     var pageViewport=(System.Windows.Controls.ScrollViewer)window.FindName("PageViewport");
     Check(pageViewport.ExtentWidth<=pageViewport.ViewportWidth+1,"Narrow layout has no horizontal overflow");
     viewport.Measure(new System.Windows.Size(1920,1080));viewport.Arrange(new System.Windows.Rect(0,0,1920,1080));viewport.UpdateLayout();
     Check(home.ColumnDefinitions[2].Width.IsStar,"Wide window restores three fluid columns");
     var demoButton=(System.Windows.Controls.Button)window.FindName("DemoToggle");
     Check(System.Windows.Controls.Grid.GetColumn(demoButton)==1 && demoButton.Content.ToString().Contains("ativa"),"Demo action is prominent in footer and identifies active mode");
     demoButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
     Check(demoButton.Content.ToString().StartsWith("Explorar"),"Demo action returns clearly to USB mode");
     var wheel=(WheelView)((System.Windows.Controls.Grid)window.FindName("WheelHost")).Children[0];
     wheel.Measure(new System.Windows.Size(400,400));wheel.Arrange(new System.Windows.Rect(0,0,400,400));
     var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(800,800,192,192,System.Windows.Media.PixelFormats.Pbgra32);
     bitmap.Render(wheel);
     Check(bitmap.PixelWidth==800,"GT-R illustration renders at high DPI");
     window.Close();GC.KeepAlive(controller);
    }
    app.Shutdown();
    Console.WriteLine(count+" checks passed.");return 0;
   }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
   finally{if(Directory.Exists(temp))Directory.Delete(temp,true);}
  }
 }
}
