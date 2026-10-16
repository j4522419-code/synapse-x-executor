# Windows launcher and attachment error host

This first-party component starts `app/Synapse X.exe` with `app` as its working
directory, hosts a small TopMost WinForms error window, and exits when the core
process exits. It does not implement or modify attachment or script execution.

Build on Windows with the .NET Framework 4.x compiler from the repository root:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 '/out:Synapse X.exe' /win32icon:theme\synapse-x.ico /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll launcher\AttachErrorHost.cs
```

The error bridge listens only on `127.0.0.1:19347`. Production origins are
`http://tauri.localhost`, `https://tauri.localhost` and `tauri://localhost`.
Notifications require a random per-launch token; request sizes and timeouts are
limited. The bridge supports only displaying/dismissing a plain-text error.

`--host-test` runs the error host without launching the core. Test mode also
allows `127.0.0.1` browser origins, exposes authenticated `/inspect` to verify
the popup’s native TopMost flag, and permits a test-only exit notification.
Normal launches do not enable those test endpoints.
