using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

[StructLayout(LayoutKind.Sequential)] struct TaskbarKey {
    public Guid format; public uint id;
    public TaskbarKey(uint value) { format=new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3");id=value; }
}
[StructLayout(LayoutKind.Explicit,Size=24)] struct TaskbarValue {
    [FieldOffset(0)] public ushort kind;
    [FieldOffset(8)] public IntPtr text;
}
[ComImport,Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface TaskbarStore {
    void GetCount(out uint count);void GetAt(uint index,out TaskbarKey key);
    void GetValue(ref TaskbarKey key,out TaskbarValue value);void SetValue(ref TaskbarKey key,ref TaskbarValue value);void Commit();
}
[ComImport,Guid("00021401-0000-0000-C000-000000000046")] class TaskbarLink {}
[ComImport,Guid("000214F9-0000-0000-C000-000000000046"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface TaskbarShellLink {
    void GetPath([Out,MarshalAs(UnmanagedType.LPWStr)] StringBuilder path,int length,IntPtr data,uint flags);
    void GetIDList(out IntPtr list);void SetIDList(IntPtr list);
    void GetDescription([Out,MarshalAs(UnmanagedType.LPWStr)] StringBuilder text,int length);void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string text);
    void GetWorkingDirectory([Out,MarshalAs(UnmanagedType.LPWStr)] StringBuilder path,int length);void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string path);
    void GetArguments([Out,MarshalAs(UnmanagedType.LPWStr)] StringBuilder text,int length);void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string text);
    void GetHotkey(out short key);void SetHotkey(short key);void GetShowCmd(out int show);void SetShowCmd(int show);
    void GetIconLocation([Out,MarshalAs(UnmanagedType.LPWStr)] StringBuilder path,int length,out int index);void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string path,int index);
    void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string path,uint reserved);void Resolve(IntPtr window,uint flags);
    void SetPath([MarshalAs(UnmanagedType.LPWStr)] string path);
}

static class TaskbarIntegration {
    public const string AppId="Grand.SynapseX.ThemedEdition";
    static System.Windows.Forms.Timer watcher;
    [DllImport("shell32.dll",CharSet=CharSet.Unicode)] static extern int SetCurrentProcessExplicitAppUserModelID(string id);
    [DllImport("shell32.dll")] static extern int SHGetPropertyStoreForWindow(IntPtr window,ref Guid id,[MarshalAs(UnmanagedType.Interface)] out TaskbarStore store);
    [DllImport("ole32.dll")] static extern int PropVariantClear(ref TaskbarValue value);
    [DllImport("shell32.dll")] static extern void SHChangeNotify(uint change,uint flags,IntPtr first,IntPtr second);
    public static void SetProcessIdentity() { SetCurrentProcessExplicitAppUserModelID(AppId); }
    public static string CompatibilityRoot(string directory) {
        directory=Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar);
        if(!String.Equals(Path.GetFileName(directory),"app",StringComparison.OrdinalIgnoreCase))return null;
        string parent=Path.GetDirectoryName(directory),launcher=Path.Combine(parent,"Synapse X.exe");
        return File.Exists(launcher)&&File.Exists(Path.Combine(directory,"core","Synapse X.exe"))?launcher:null;
    }
    public static bool RedirectCompatibilityLauncher() {
        string root=CompatibilityRoot(AppDomain.CurrentDomain.BaseDirectory);if(root==null)return false;
        Process.Start(new ProcessStartInfo(root) { WorkingDirectory=Path.GetDirectoryName(root),UseShellExecute=true });return true;
    }
    static void Put(TaskbarStore store,uint id,string text) {
        var key=new TaskbarKey(id);var value=new TaskbarValue();
        try { if(text!=null) {value.kind=31;value.text=Marshal.StringToCoTaskMemUni(text);}store.SetValue(ref key,ref value); }
        finally { PropVariantClear(ref value); }
    }
    public static string ReadWindowProperty(IntPtr window,uint id) {
        Guid iid=typeof(TaskbarStore).GUID;TaskbarStore store;Marshal.ThrowExceptionForHR(SHGetPropertyStoreForWindow(window,ref iid,out store));
        var key=new TaskbarKey(id);var value=new TaskbarValue();
        try {store.GetValue(ref key,out value);return value.kind==31?Marshal.PtrToStringUni(value.text):null;}
        finally {PropVariantClear(ref value);Marshal.ReleaseComObject(store);}
    }
    public static void SetWindowProperties(IntPtr window,string launcher) {
        Guid iid=typeof(TaskbarStore).GUID;TaskbarStore store;Marshal.ThrowExceptionForHR(SHGetPropertyStoreForWindow(window,ref iid,out store));
        try {
            // Relaunch information must precede the AppUserModelID update.
            Put(store,2,"\""+launcher+"\"");Put(store,3,launcher+",0");Put(store,4,"@"+launcher+",-1");Put(store,5,AppId);
        } finally {Marshal.ReleaseComObject(store);}
    }
    public static void Watch(Process core,string launcher) {
        int attempts=0;watcher=new System.Windows.Forms.Timer {Interval=250};
        watcher.Tick+=(sender,args)=>{
            bool finished=false;
            try {core.Refresh();if(core.HasExited)finished=true;else if(core.MainWindowHandle!=IntPtr.Zero) {SetWindowProperties(core.MainWindowHandle,launcher);finished=true;}}
            catch(InvalidOperationException) {finished=true;}catch(COMException) { }
            if(finished||++attempts>=120) {watcher.Stop();watcher.Dispose();watcher=null;}
        };watcher.Start();
    }
    static string Digest(string file) {using(var sha=SHA256.Create())using(var input=File.OpenRead(file))return BitConverter.ToString(sha.ComputeHash(input)).Replace("-","").ToLowerInvariant();}
    public static int RepairPins(string directory,string launcher,HashSet<string> legacy,Action<Exception> report=null) {
        if(!Directory.Exists(directory))return 0;int count=0;string installation=Path.GetDirectoryName(Path.GetFullPath(launcher));
        foreach(string file in Directory.GetFiles(directory,"*.lnk")) {
            TaskbarShellLink link=null;
            try {
                link=(TaskbarShellLink)new TaskbarLink();var persist=(IPersistFile)link;persist.Load(file,2);
                var target=new StringBuilder(32768);link.GetPath(target,target.Capacity,IntPtr.Zero,0);string old=target.ToString();
                if(!String.Equals(Path.GetFileName(old),"Synapse X.exe",StringComparison.OrdinalIgnoreCase)||!File.Exists(old))continue;
                string full=Path.GetFullPath(old);
                bool local=String.Equals(full,Path.Combine(installation,"app","Synapse X.exe"),StringComparison.OrdinalIgnoreCase)||
                    String.Equals(full,Path.Combine(installation,"app","core","Synapse X.exe"),StringComparison.OrdinalIgnoreCase);
                if(!local&&!legacy.Contains(Digest(full)))continue;
                string backup=file+".before-synapse-x";if(!File.Exists(backup))File.Copy(file,backup);
                link.SetPath(launcher);link.SetArguments("");link.SetWorkingDirectory(installation);link.SetIconLocation(launcher,0);link.SetDescription("Synapse X");
                var store=(TaskbarStore)link;Put(store,2,"\""+launcher+"\"");Put(store,3,launcher+",0");Put(store,4,"@"+launcher+",-1");Put(store,5,AppId);store.Commit();
                persist.Save(file,true);count++;
                IntPtr changed=Marshal.StringToCoTaskMemUni(file);try {SHChangeNotify(0x2000,5,changed,IntPtr.Zero);}finally {Marshal.FreeCoTaskMem(changed);}
            } catch(IOException error) {if(report!=null)report(error);}catch(UnauthorizedAccessException error) {if(report!=null)report(error);}catch(COMException error) {if(report!=null)report(error);}
            finally {if(link!=null)Marshal.ReleaseComObject(link);}
        }return count;
    }
    public static void RepairUserPins(string launcher) {
        try {
            string directory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Microsoft","Internet Explorer","Quick Launch","User Pinned","TaskBar");
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("SynapseX.legacy-taskbar-hashes.json")) {
                if(stream==null)return;using(var reader=new StreamReader(stream)) {
                    var hashes=new HashSet<string>(new JavaScriptSerializer().Deserialize<string[]>(reader.ReadToEnd()),StringComparer.OrdinalIgnoreCase);
                    RepairPins(directory,launcher,hashes);
                }
            }
        }catch(IOException) { }catch(UnauthorizedAccessException) { }
    }
}
