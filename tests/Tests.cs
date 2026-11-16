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
     window.Close();GC.KeepAlive(controller);
    }
    app.Shutdown();
    Console.WriteLine(count+" checks passed.");return 0;
   }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
   finally{if(Directory.Exists(temp))Directory.Delete(temp,true);}
  }
 }
}
