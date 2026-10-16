using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
[assembly: AssemblyTitle("Synapse X")]
[assembly: AssemblyProduct("Synapse X")]
[assembly: AssemblyDescription("Synapse X launcher and attachment error window")]
[assembly: AssemblyVersion("1.3.3.0")]
[assembly: AssemblyFileVersion("1.3.3.0")]

sealed class ErrorWindow : Form {
    readonly TextBox reason;
    public string Reason { get { return reason.Text; } }
    public ErrorWindow(string message) {
        Text="Synapse X - Attach failed"; ClientSize=new Size(420,220);
        FormBorderStyle=FormBorderStyle.FixedDialog; MaximizeBox=false; MinimizeBox=false;
        StartPosition=FormStartPosition.CenterScreen; TopMost=true; ShowInTaskbar=true;
        BackColor=Color.FromArgb(28,29,32); ForeColor=Color.FromArgb(239,242,246);
        Font=new Font("Segoe UI",10); AutoScaleMode=AutoScaleMode.Dpi;
        Icon=System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        var heading=new Label { Text="Attach failed", Font=new Font("Segoe UI",14,FontStyle.Bold),
            AutoSize=true, Location=new Point(20,18), ForeColor=Color.FromArgb(240,155,163) };
        reason=new TextBox { Text=message, Multiline=true, ReadOnly=true, WordWrap=true,
            ScrollBars=ScrollBars.Vertical, BorderStyle=BorderStyle.None, BackColor=BackColor,
            ForeColor=ForeColor, Location=new Point(22,58), Size=new Size(376,100), TabIndex=0 };
        var close=new Button { Text="Close", Location=new Point(310,174), Size=new Size(88,29),
            FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(47,50,56), ForeColor=ForeColor, TabIndex=1 };
        close.FlatAppearance.BorderColor=Color.FromArgb(77,83,94); close.Click+=(s,e)=>Close();
        AcceptButton=close; CancelButton=close; Controls.Add(heading); Controls.Add(reason); Controls.Add(close);
    }
    public void UpdateReason(string message) { reason.Text=message; if(!Visible)Show(); TopMost=true; BringToFront(); Activate(); }
}

sealed class Host : ApplicationContext {
    const int Port=19347;
    readonly Control dispatch=new Control();
    readonly TcpListener listener=new TcpListener(IPAddress.Loopback,Port);
    readonly string token;
    readonly bool testing;
    readonly RuntimeBranding runtime;
    volatile bool stopping;
    ErrorWindow popup;
    Process core;
    [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")] static extern IntPtr WindowLong(IntPtr hwnd,int index);
    public Host(bool test) {
        testing=test;runtime=new RuntimeBranding(AppDomain.CurrentDomain.BaseDirectory,RuntimeBranding.DownloadCatalog);
        var bytes=new byte[32]; using(var rng=RandomNumberGenerator.Create())rng.GetBytes(bytes);
        token=Convert.ToBase64String(bytes); var handle=dispatch.Handle;
        listener.Start(); new Thread(Accept) { IsBackground=true }.Start();
    }
    public void StartCore() {
        string dir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"app");
        string file=Path.Combine(dir,"Synapse X.exe");
        if(!File.Exists(file))throw new FileNotFoundException("Extract the complete ZIP, including its app folder.",file);
        core=Process.Start(new ProcessStartInfo(file) { WorkingDirectory=dir, UseShellExecute=false });
        core.EnableRaisingEvents=true; core.Exited+=(s,e)=>Queue(Stop);
        if(core.HasExited)Queue(Stop);
    }
    void Queue(Action action) { if(!stopping)try { dispatch.BeginInvoke(action); } catch(InvalidOperationException) {} }
    void ShowFailure(string message) {
        if(popup==null||popup.IsDisposed) { popup=new ErrorWindow(message); popup.Show(); popup.Activate(); }
        else popup.UpdateReason(message);
    }
    void Dismiss() { if(popup!=null&&!popup.IsDisposed)popup.Close(); popup=null; }
    void Stop() { if(stopping)return; stopping=true; listener.Stop(); Dismiss(); dispatch.Dispose(); ExitThread(); }
    void Accept() {
        while(!stopping)try { var client=listener.AcceptTcpClient(); ThreadPool.QueueUserWorkItem(o=>Handle((TcpClient)o),client); }
        catch(SocketException) { if(!stopping)Queue(Stop); }
        catch(ObjectDisposedException) { return; }
    }
    bool AllowedOrigin(string origin) {
        if(origin=="http://tauri.localhost"||origin=="https://tauri.localhost"||origin=="tauri://localhost")return true;
        Uri uri; return testing&&Uri.TryCreate(origin,UriKind.Absolute,out uri)&&uri.Host=="127.0.0.1";
    }
    void Reply(NetworkStream stream,int code,string origin,object body) {
        byte[] bytes=Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(body));
        string cors=AllowedOrigin(origin)?"Access-Control-Allow-Origin: "+origin+"\r\nVary: Origin\r\n":"";
        string headers="HTTP/1.1 "+code+" Result\r\nContent-Type: application/json; charset=utf-8\r\n"+
            "Cache-Control: no-store\r\nConnection: close\r\n"+cors+
            "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\nAccess-Control-Allow-Headers: Content-Type, X-Synapse-Token\r\n"+
            "Access-Control-Allow-Private-Network: true\r\nContent-Length: "+bytes.Length+"\r\n\r\n";
        byte[] head=Encoding.ASCII.GetBytes(headers); stream.Write(head,0,head.Length); stream.Write(bytes,0,bytes.Length);
    }
    void Handle(TcpClient client) {
        using(client)try {
            client.ReceiveTimeout=2500; client.SendTimeout=2500; var stream=client.GetStream();
            var header=new List<byte>(); int matched=0;
            while(header.Count<16384&&matched<4) {
                int value=stream.ReadByte(); if(value<0)return; header.Add((byte)value);
                byte wanted=new byte[]{13,10,13,10}[matched]; matched=value==wanted?matched+1:value==13?1:0;
            }
            if(matched!=4)return;
            string[] lines=Encoding.ASCII.GetString(header.ToArray()).Split(new[]{"\r\n"},StringSplitOptions.None);
            string[] first=lines[0].Split(' '); if(first.Length!=3)return;
            var headers=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            for(int i=1;i<lines.Length;i++) { int split=lines[i].IndexOf(':'); if(split>0)headers[lines[i].Substring(0,split)]=lines[i].Substring(split+1).Trim(); }
            int length=0;
            if((first[0]=="POST"&&!headers.ContainsKey("Content-Length"))||(headers.ContainsKey("Content-Length")&&(!Int32.TryParse(headers["Content-Length"],out length)||length<0||length>65536))) { Reply(stream,413,"",new{error="Invalid request size"});return; }
            var payload=new byte[length];int read=0;
            while(read<length) { int n=stream.Read(payload,read,length-read);if(n==0)return;read+=n; }
            string origin=headers.ContainsKey("Origin")?headers["Origin"]:"";
            if(!AllowedOrigin(origin)) { Reply(stream,403,origin,new{error="Origin denied"}); return; }
            if(first[0]=="OPTIONS") { Reply(stream,200,origin,new{ok=true}); return; }
            if(first[0]=="GET"&&first[1]=="/session") { Reply(stream,200,origin,new{host="synapse-x-error-host",version=1,token=token}); return; }
            if(!headers.ContainsKey("X-Synapse-Token")||!String.Equals(headers["X-Synapse-Token"],token,StringComparison.Ordinal)) { Reply(stream,403,origin,new{error="Invalid session"}); return; }
            if(testing&&first[0]=="GET"&&first[1]=="/inspect") {
                object result=dispatch.Invoke(new Func<object>(()=>new{visible=popup!=null&&!popup.IsDisposed,
                    topMost=popup!=null&&!popup.IsDisposed&&popup.TopMost,
                    nativeTopMost=popup!=null&&!popup.IsDisposed&&(WindowLong(popup.Handle,-20).ToInt64()&8)!=0,
                    reason=popup!=null&&!popup.IsDisposed?popup.Reason:""}));
                Reply(stream,200,origin,result); return;
            }
            if(first[0]!="POST"||(first[1]!="/notify"&&first[1]!="/runtime/prepare")) { Reply(stream,404,origin,new{error="Not found"}); return; }
            var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(Encoding.UTF8.GetString(payload));
            if(first[1]=="/runtime/prepare") {
                if(testing) { Reply(stream,403,origin,new{error="Runtime installation is disabled in popup test mode"});return; }
                try { Reply(stream,200,origin,runtime.Prepare()); }
                catch(Exception failure) { Reply(stream,409,origin,new{error=failure.Message}); }
                return;
            }
            string kind=data.ContainsKey("type")?data["type"] as string:"";
            if(kind=="error"&&data.ContainsKey("message")&&data["message"] is string) {
                string message=((string)data["message"]).Trim(); if(message.Length==0)message="No detailed reason was returned by the runtime.";
                string text=message; Queue(()=>ShowFailure(text));
            } else if(kind=="dismiss")Queue(Dismiss);
            else if(kind=="exit"&&testing)Queue(Stop);
            else { Reply(stream,400,origin,new{error="Invalid notification"}); return; }
            Reply(stream,200,origin,new{ok=true});
        } catch(Exception) { /* Malformed local requests do not stop the host. */ }
    }
    protected override void Dispose(bool disposing) { if(!stopping) { stopping=true; listener.Stop(); } if(disposing)dispatch.Dispose(); base.Dispose(disposing); }
}

static class Entry {
    [STAThread] static void Main(string[] args) {
        bool testing=args.Length==1&&args[0]=="--host-test",first;
        using(var single=new Mutex(true,"Local\\SynapseX.AttachErrorHost.v1",out first)) {
            if(!first) { MessageBox.Show("Synapse X is already running.","Synapse X",MessageBoxButtons.OK,MessageBoxIcon.Information); return; }
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            try { using(var host=new Host(testing)) { if(!testing)host.StartCore(); Application.Run(host); } }
            catch(Exception error) { MessageBox.Show(error.Message,"Synapse X",MessageBoxButtons.OK,MessageBoxIcon.Error); }
        }
    }
}
