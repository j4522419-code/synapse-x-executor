using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

sealed class RuntimeCatalog { public int schema {get;set;} public RuntimeRule[] runtimes {get;set;} }
sealed class RuntimeRule {
    public string source_sha256 {get;set;} public string patched_sha256 {get;set;}
    public string name {get;set;} public string version {get;set;} public string user_agent {get;set;}
    public RuntimeEdit[] patches {get;set;}
}
sealed class RuntimeEdit { public int offset {get;set;} public string before {get;set;} public string after {get;set;} }

sealed class RuntimeBranding {
    const string Brand="SYNAPSE X MADE BY GRAND";
    const string CatalogUrl="https://raw.githubusercontent.com/j4522419-code/synapse-x-potassium-theme/main/runtime/catalog.json";
    readonly string file;
    readonly Func<string> remote;
    readonly object gate=new object();
    public RuntimeBranding(string installation,Func<string> remoteFetch) {
        file=Path.Combine(Path.GetFullPath(installation),"app","core","bin","Synapse X.dll");remote=remoteFetch;
    }
    public static string DownloadCatalog() {
        ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
        var request=(HttpWebRequest)WebRequest.Create(CatalogUrl+"?check="+DateTime.UtcNow.Ticks.ToString("x"));
        request.Method="GET";request.UserAgent="Synapse-X-runtime-updater/1.3.4";
        request.Timeout=8000;request.ReadWriteTimeout=8000;request.AllowAutoRedirect=false;
        using(var response=(HttpWebResponse)request.GetResponse())
        using(var input=response.GetResponseStream())
        using(var output=new MemoryStream()) {
            var buffer=new byte[8192];int read;
            while((read=input.Read(buffer,0,buffer.Length))>0) { if(output.Length+read>524288)throw new InvalidDataException("Runtime catalog is too large.");output.Write(buffer,0,read); }
            return Encoding.UTF8.GetString(output.ToArray());
        }
    }
    static string Digest(byte[] bytes) { using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant(); }
    static byte[] Hex(string value) {
        if(value==null||value.Length%2!=0||value.Length>16384)throw new InvalidDataException("Invalid runtime patch bytes.");
        var result=new byte[value.Length/2];for(int i=0;i<result.Length;i++)result[i]=Convert.ToByte(value.Substring(i*2,2),16);return result;
    }
    static RuntimeCatalog Parse(string text) {
        if(text==null||text.Length>524288)throw new InvalidDataException("Invalid runtime catalog.");
        var catalog=new JavaScriptSerializer().Deserialize<RuntimeCatalog>(text);
        if(catalog==null||catalog.schema!=1||catalog.runtimes==null||catalog.runtimes.Length>128)throw new InvalidDataException("Unsupported runtime catalog.");
        foreach(var rule in catalog.runtimes) {
            if(rule==null||rule.name!=Brand||String.IsNullOrWhiteSpace(rule.version)||rule.version.Length>80||rule.user_agent!=Brand+"/"+rule.version||
                Hex(rule.source_sha256).Length!=32||Hex(rule.patched_sha256).Length!=32||rule.patches==null||rule.patches.Length>128)throw new InvalidDataException("Invalid runtime identity rule.");
            foreach(var edit in rule.patches)if(edit==null||edit.offset<0||Hex(edit.before).Length!=Hex(edit.after).Length)throw new InvalidDataException("Invalid runtime edit.");
        }
        return catalog;
    }
    public object Prepare() {
        lock(gate) {
            var rules=new List<RuntimeRule>();
            // The repository supplies version-matched patch data, never a program
            // or a caller-provided path. The embedded catalog works offline.
            try { if(remote!=null)rules.AddRange(Parse(remote()).runtimes); } catch { }
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("SynapseX.runtime-catalog.json")) {
                if(stream==null)throw new InvalidDataException("The launcher is missing its bundled runtime catalog.");
                using(var reader=new StreamReader(stream))rules.AddRange(Parse(reader.ReadToEnd()).runtimes);
            }
            if(!File.Exists(file))throw new FileNotFoundException("The compatibility downloader did not provide its runtime. Retry the download.");
            var info=new FileInfo(file);if(info.Length<1024||info.Length>67108864)throw new InvalidDataException("The downloaded runtime has an unexpected size.");
            byte[] original=File.ReadAllBytes(file);string digest=Digest(original);
            foreach(var rule in rules)if(digest==rule.patched_sha256)return new{ok=true,name=rule.name,version=rule.version,user_agent=rule.user_agent,patched=false};
            RuntimeRule selected=null;foreach(var rule in rules)if(digest==rule.source_sha256) {selected=rule;break;}
            if(selected==null)throw new InvalidDataException("The vendor runtime was updated, but no matching SYNAPSE X MADE BY GRAND branding patch is available yet. Retry after a matching patch is published to the repository. Runtime: "+digest.Substring(0,12));
            byte[] patched=(byte[])original.Clone();var touched=new HashSet<int>();
            foreach(var edit in selected.patches) {
                byte[] before=Hex(edit.before),after=Hex(edit.after);
                if(edit.offset>patched.Length-before.Length)throw new InvalidDataException("Runtime patch is outside the file.");
                for(int i=0;i<before.Length;i++) {
                    int offset=edit.offset+i;if(!touched.Add(offset)||original[offset]!=before[i])throw new InvalidDataException("Runtime patch does not match the downloaded file.");patched[offset]=after[i];
                }
            }
            if(Digest(patched)!=selected.patched_sha256)throw new InvalidDataException("Runtime patch verification failed.");
            string temporary=file+".branding-"+Guid.NewGuid().ToString("N")+".tmp",backup=file+".before-branding";
            try {
                File.WriteAllBytes(temporary,patched);
                if(Digest(File.ReadAllBytes(temporary))!=selected.patched_sha256||Digest(File.ReadAllBytes(file))!=digest)throw new IOException("The runtime changed while preparing it. Retry after the download finishes.");
                File.Copy(file,backup,true);File.Replace(temporary,file,null);
                if(Digest(File.ReadAllBytes(file))!=selected.patched_sha256)throw new IOException("The installed runtime did not pass verification. Its previous copy is saved beside it.");
            } finally { if(File.Exists(temporary))File.Delete(temporary); }
            return new{ok=true,name=selected.name,version=selected.version,user_agent=selected.user_agent,patched=true};
        }
    }
}
