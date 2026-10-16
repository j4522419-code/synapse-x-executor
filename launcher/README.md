# Windows launcher, runtime preparation and attachment error host

This first-party component starts `app/core/Synapse X.exe` with `app/core` as its working
directory, prepares known compatibility updates before native initialization,
hosts a TopMost WinForms error window, and exits when the core process exits.
It does not implement attachment or script execution.

Build on Windows with the .NET Framework 4.x compiler from the repository root:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 '/out:Synapse X.exe' /win32icon:theme\synapse-x.ico /win32manifest:launcher\launcher.manifest /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /resource:runtime\catalog.json,SynapseX.runtime-catalog.json /resource:launcher\legacy-taskbar-hashes.json,SynapseX.legacy-taskbar-hashes.json launcher\AttachErrorHost.cs launcher\RuntimeBranding.cs launcher\RuntimeLayout.cs launcher\TaskbarIntegration.cs
```

The root launcher embeds `requireAdministrator` with `uiAccess=false`. Opening
it requests elevation through Windows UAC. The child interface inherits that
process context. Test mode retains the manifest requirement; the isolated host
checks use a separately compiled asInvoker test executable.

## Runtime preparation

After the original compatibility downloader finishes, the frontend calls the
authenticated `POST /runtime/prepare` endpoint. The host reads the fixed
`app/core/bin/Potassium.dll` path; callers cannot supply paths, patch bytes or URLs.
The only remote catalog URL is
`https://raw.githubusercontent.com/j4522419-code/synapse-x-potassium-theme/main/runtime/catalog.json`.
A cache-refresh query is appended when checking the fixed catalog URL.
It uses HTTPS certificate validation, TLS 1.2, bounded response sizes/timeouts
and no redirects. A bundled catalog is available when remote retrieval fails.

Rules must match the entire source SHA-256. Every patch checks its expected
bytes and bounds; the complete output must match the catalog's target SHA-256.
The host keeps `Potassium.dll.before-branding` and replaces the prepared DLL
atomically. Already prepared hashes are accepted. Unknown source hashes wait
with a clear message until a matching catalog entry is published. Future vendor
code needs inspection and its own matching rule; old offsets are never applied
to an unknown binary. The native authentication/execution wrappers are retained.

After compilation, add the Windows string-table name used for taskbar relaunch:

```powershell
python -c "from pathlib import Path; from launcher.launcher_resources import add_taskbar_name; add_taskbar_name(Path('Synapse X.exe').resolve())"
```

Copy the launcher to `app/Synapse X.exe` as the compatibility redirect. The
native interface, dependencies and bin folder live in `app/core`, with the core
directory as the native process working directory. The redirect only resolves
the fixed root entry in this directory layout; it takes no path from the UI.

The launcher sets the native window's AppUserModelID, relaunch command, icon and
display-name resource through its Windows property store. Relaunch properties
are set before the AppUserModelID. On upgrade it repairs this installation's
taskbar links and links to exact previous executable hashes in the embedded
allowlist. It backs up links and leaves unrelated targets unchanged. Migration
runs when the new root launcher opens; it does not start old executables.

Before native startup, `RuntimeLayout` seeds original-name files in
`app/core/bin` from the visible root `bin/Synapse X.dll` and `bin/Synapse X.exe`.
Existing internal files are preserved. After verified preparation, the visible
copies are synchronized. Copies use fixed installation paths, verified temporary
files and atomic replacement. Original native download/launch filename
references are retained. Internal names are necessary compatibility copies;
they are not evidence of an older runtime version.

The candidate bundles the exact submitted 2.5.4 pair. Its DLL patch and runtime
identity version are unchanged from v1.3.4, isolating the layout restoration.
This does not establish that the protected loader accepts the DLL patch; live
attachment on the affected PC remains necessary to confirm the reported error.

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
