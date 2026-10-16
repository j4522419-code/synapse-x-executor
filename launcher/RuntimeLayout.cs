using System;
using System.IO;

sealed class RuntimeLayout {
    readonly string runtime;
    public RuntimeLayout(string installation) {
        runtime=Path.Combine(Path.GetFullPath(installation),"app","core","bin");
    }
    public void Prepare() {
        // One runtime folder, retaining the protected vendor's filenames.
        // No root bin copy is created and no runtime is overwritten here.
        foreach(string name in new[]{"Potassium.dll","Potassium.exe"})
            if(!File.Exists(Path.Combine(runtime,name)))
                throw new FileNotFoundException("The runtime is incomplete. Extract the entire download into a fresh folder before opening Synapse X Executor.");
        RefreshVisibility();
    }
    public void RefreshVisibility() {
        // This changes ordinary Explorer visibility, not access permissions.
        // Never change ACLs, System attributes, or global Explorer settings.
        try {
            FileAttributes attributes=File.GetAttributes(runtime);
            if((attributes&FileAttributes.ReparsePoint)!=0)return;
            if((attributes&FileAttributes.Hidden)==0)
                File.SetAttributes(runtime,attributes|FileAttributes.Hidden);
        }catch(IOException) {
            // Filesystems that cannot retain Hidden must still run normally.
        }catch(UnauthorizedAccessException) {}
    }
}
