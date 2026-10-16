using System;
using System.IO;
using System.Security.Cryptography;

sealed class RuntimeLayout {
    readonly string visible,compatibility;
    public RuntimeLayout(string installation) {
        string root=Path.GetFullPath(installation);
        visible=Path.Combine(root,"bin");compatibility=Path.Combine(root,"app","core","bin");
    }
    static string Digest(byte[] data) {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(data));}
    static void CopyVerified(string source,string destination,bool onlyMissing) {
        if(onlyMissing&&File.Exists(destination))return;
        byte[] bytes=File.ReadAllBytes(source);
        if(bytes.Length==0||bytes.Length>67108864)throw new InvalidDataException("Unexpected runtime file size.");
        if(File.Exists(destination)&&Digest(File.ReadAllBytes(destination))==Digest(bytes))return;
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        string temporary=destination+".layout-"+Guid.NewGuid().ToString("N")+".tmp";
        try {
            File.WriteAllBytes(temporary,bytes);
            if(Digest(File.ReadAllBytes(temporary))!=Digest(bytes))throw new IOException("Runtime copy verification failed.");
            if(onlyMissing&&File.Exists(destination))return;
            if(File.Exists(destination))File.Replace(temporary,destination,null);else File.Move(temporary,destination);
        } finally {if(File.Exists(temporary))File.Delete(temporary);}
    }
    public void Seed() {
        // Preserve the original protected loader's basename, neighboring DLL
        // and bin directory. Only fixed installation paths are used.
        CopyVerified(Path.Combine(visible,"Synapse X.dll"),Path.Combine(compatibility,"Potassium.dll"),true);
        CopyVerified(Path.Combine(visible,"Synapse X.exe"),Path.Combine(compatibility,"Potassium.exe"),true);
    }
    public void PublishVisibleCopies() {
        CopyVerified(Path.Combine(compatibility,"Potassium.dll"),Path.Combine(visible,"Synapse X.dll"),false);
        CopyVerified(Path.Combine(compatibility,"Potassium.exe"),Path.Combine(visible,"Synapse X.exe"),false);
    }
}
