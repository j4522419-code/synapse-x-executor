# Windows launcher, runtime preparation and attachment error host

This first-party component starts `app/Synapse X.exe` with `app` as its working
directory, prepares known compatibility updates before native initialization,
hosts a TopMost WinForms error window, and exits when the core process exits.
It does not implement attachment or script execution.

Build on Windows with the .NET Framework 4.x compiler from the repository root:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 '/out:Synapse X.exe' /win32icon:theme\synapse-x.ico /win32manifest:launcher\launcher.manifest /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /resource:runtime\catalog.json,SynapseX.runtime-catalog.json launcher\AttachErrorHost.cs launcher\RuntimeBranding.cs
```

The root launcher embeds `requireAdministrator` with `uiAccess=false`. Opening
it requests elevation through Windows UAC. The child interface inherits that
process context. Test mode retains the manifest requirement; the isolated host
checks use a separately compiled asInvoker test executable.

## Runtime preparation

After the original compatibility downloader finishes, the frontend calls the
authenticated `POST /runtime/prepare` endpoint. The host reads the fixed
`app/bin/Potassium.dll` path; callers cannot supply paths, patch bytes or URLs.
The only remote catalog URL is
`https://raw.githubusercontent.com/j4522419-code/synapse-x-potassium-theme/main/runtime/catalog.json`.
It uses HTTPS certificate validation, TLS 1.2, bounded response sizes/timeouts
and no redirects. A bundled catalog is available when remote retrieval fails.

Rules must match the entire source SHA-256. Every patch checks its expected
bytes and bounds; the complete output must match the catalog's target SHA-256.
The host keeps `Potassium.dll.before-branding` and replaces the prepared DLL
atomically. Already prepared hashes are accepted. Unknown source hashes wait
with a clear message until a matching catalog entry is published. Future vendor
code needs inspection and its own matching rule; old offsets are never applied
to an unknown binary. The native authentication/execution wrappers are retained.

The launcher is required for startup preparation. Building this launcher alone
does not recover or build the supplied native application and dependencies.

## Local bridge and error window

The bridge listens only on `127.0.0.1:19347`. Production origins are
`http://tauri.localhost`, `https://tauri.localhost` and `tauri://localhost`.
Notifications and preparation require a random per-launch token. Request sizes
and timeouts are limited. Error notifications display or dismiss plain text.

`--host-test` runs the error host without launching the core. Test mode also
allows `127.0.0.1` browser origins, exposes authenticated `/inspect` to verify
the native TopMost flag, and permits a test-only exit notification. Runtime
installation is denied in this mode. Production does not enable the inspection
or test-exit endpoints.
