using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace VolanteStudio {
    // Keep the installed vendor protocol intact. No bootloader, serial port or flashing code.
    public sealed class EmcBridge : IDisposable {
        object model, hid;
        Type modelType;
        string compat;
        public event Action<byte[]> PacketSent;
        Delegate sentHandler;
        EventInfo sentEvent;
        Delegate receivedHandler;
        EventInfo receivedEvent;
        int steering;
        public double SteeringPosition { get { return Math.Max(-1,Math.Min(1,System.Threading.Volatile.Read(ref steering)/32767.0)); } }
        public bool IsReady { get { return model!=null; } }
        public bool Connected { get { return hid!=null && Convert.ToString(hid.GetType().GetProperty("ConnectionStatus").GetValue(hid,null))=="Connected"; } }
        public bool CanConfigure { get { return Connected && string.Equals(ReadString("mainWindow"),"true",StringComparison.OrdinalIgnoreCase); } }
        public string Firmware { get { return ReadString("fwver").Replace("Firmware ver :", "").Trim(); } }
        public string Error { get; private set; }
        public void Open(string basePath) {
            compat=Path.Combine(basePath,"compat");
            if(!File.Exists(Path.Combine(compat,"EMC.dll"))) compat=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"EMCLite");
            AppDomain.CurrentDomain.AssemblyResolve+=Resolve;
            try {
                if(!File.Exists(Path.Combine(compat,"EMC.dll"))) throw new FileNotFoundException("Os componentes de comunicação não foram encontrados. Mantenha a pasta compat junto do aplicativo ou a instalação original do EMCLite.");
                Assembly.LoadFrom(Path.Combine(compat,"EMCHid.dll"));
                var asm=Assembly.LoadFrom(Path.Combine(compat,"EMC.dll"));
                modelType=asm.GetType("EMC.MainViewModel",true);
                model=Activator.CreateInstance(modelType);
                hid=modelType.GetField("HidUtil").GetValue(model);
                sentEvent=hid.GetType().GetEvent("RaisePacketSentEvent");
                var invoke=sentEvent.EventHandlerType.GetMethod("Invoke");
                var method=GetType().GetMethod("OnPacketSent",BindingFlags.Instance|BindingFlags.NonPublic).MakeGenericMethod(invoke.GetParameters()[1].ParameterType);
                sentHandler=Delegate.CreateDelegate(sentEvent.EventHandlerType,this,method);
                sentEvent.AddEventHandler(hid,sentHandler);
                receivedEvent=hid.GetType().GetEvent("RaisePacketReceivedEvent");
                var receivedType=receivedEvent.EventHandlerType.GetMethod("Invoke").GetParameters()[1].ParameterType;
                receivedHandler=Delegate.CreateDelegate(receivedEvent.EventHandlerType,this,GetType().GetMethod("OnPacketReceived",BindingFlags.Instance|BindingFlags.NonPublic).MakeGenericMethod(receivedType));
                receivedEvent.AddEventHandler(hid,receivedHandler);
            } catch(Exception ex) { Error=Unwrap(ex).Message; Dispose(); }
        }
        void OnPacketReceived<T>(object sender,T buffer) {
            if(!(bool)typeof(T).GetProperty("TransferSuccessful").GetValue(buffer,null))return;
            int value;
            if(TryReadSteering((byte[])typeof(T).GetProperty("buffer").GetValue(buffer,null),out value))System.Threading.Volatile.Write(ref steering,value);
        }
        public static bool TryReadSteering(byte[] report,out int value) {
            value=0;
            if(report==null || report.Length<7 || report[0]!=3)return false;
            value=(short)(report[5] | (report[6]<<8));return true;
        }
        void OnPacketSent<T>(object sender, T buffer) {
            var success=typeof(T).GetProperty("TransferSuccessful");
            if(success!=null && (bool)success.GetValue(buffer,null)) {
                var data=(byte[])typeof(T).GetProperty("buffer").GetValue(buffer,null);
                var callback=PacketSent; if(callback!=null) callback((byte[])data.Clone());
            }
        }
        Assembly Resolve(object sender,ResolveEventArgs args) {
            var name=new AssemblyName(args.Name).Name;
            var file=Path.Combine(compat,name+".dll");
            return File.Exists(file)?Assembly.LoadFrom(file):null;
        }
        public int ReadInt(string name) { if(model==null)return 0; return Convert.ToInt32(modelType.GetProperty(name).GetValue(model,null)); }
        public string ReadString(string name) { if(model==null)return ""; return Convert.ToString(modelType.GetProperty(name).GetValue(model,null)); }
        public void Set(string name,object value) {
            RequireConnection();
            var property=modelType.GetProperty(name);
            if(property==null || !property.CanWrite) throw new InvalidOperationException("Ajuste não disponível: "+name);
            property.SetValue(model,Convert.ChangeType(value,property.PropertyType),null);
        }
        public void Command(string name, object parameter) {
            RequireConnection();
            var command=(ICommand)modelType.GetProperty(name).GetValue(model,null);
            if(!command.CanExecute(parameter))throw new InvalidOperationException("O volante ainda não está pronto para esse comando.");
            command.Execute(parameter);
        }
        void RequireConnection() { if(!CanConfigure)throw new InvalidOperationException("Aguarde um volante com firmware compatível conectado pelo USB."); }
        public static Exception Unwrap(Exception ex) { while(ex.InnerException!=null)ex=ex.InnerException;return ex; }
        public void Dispose() {
            if(hid!=null) {
                if(sentHandler!=null && sentEvent!=null)sentEvent.RemoveEventHandler(hid,sentHandler);
                if(receivedHandler!=null && receivedEvent!=null)receivedEvent.RemoveEventHandler(hid,receivedHandler);
                try { hid.GetType().GetMethod("CloseDevice").Invoke(hid,null); } catch { }
            }
            hid=null; model=null;
            AppDomain.CurrentDomain.AssemblyResolve-=Resolve;
        }
    }
}
