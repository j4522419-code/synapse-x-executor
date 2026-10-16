# Synapse X — themed edition

An unofficial Synapse X inspired interface for the supplied Windows application.
This project is not affiliated with the original Synapse X developers.

**[Download the Windows build](https://github.com/j4522419-code/synapse-x-potassium-theme/releases/latest)**

![Updated start page and attached button](docs/home-preview.jpg)

The preview uses the actual frontend in a browser harness, with sample filenames
and a simulated confirmed attachment to show the button styling.

## Version 1.3.0

- Attach pulses green with a subtle outward ring after the client connection is
  confirmed. Disconnecting clears it. Reduced motion uses static green feedback.
- Attach failures show the reason returned by the runtime in a small Windows
  window that stays above other apps. Retry dismisses the previous error.
- Simplified home page: **Using Grand’s Internal**, Start actions, Tip of the Day
  underneath, and a large Synapse X logo on the right. Recent Scripts and Getting
  Started are removed.
- Wider Scripts / Auto Execute pane with wrapped filenames and section labels.
  Its header arrow slides it closed; the folder button beside Settings reopens
  it. Ctrl+B also toggles the pane. Hidden contents cannot take keyboard focus.

## Install

1. Download `Synapse-X-v1.3.0-windows-x64.zip` from the release’s **Assets**.
2. Extract the **complete ZIP** into a new folder, including the entire `app` folder.
3. Restart Roblox to unload an older runtime.
4. Open the **top-level Synapse X.exe**, keeping the `app` folder beside it.

The root executable is a small launcher and Windows error-window host. It starts
the main interface inside `app`. Windows x64 with .NET Framework 4.x is required.
Opening the inner executable directly uses an in-app error dialog when the host
is unavailable; that fallback does not stay above other Windows apps.

This version bundles its inspected runtime in `app/bin`. Automatic vendor runtime
downloads are skipped to preserve the existing internal-menu removal. Update the
complete application from this repository’s releases; future Roblox changes may
require a compatible release. Existing sign-in requirements still apply.

## Editor and appearance

The classic charcoal interface retains the gray editor, compact tabs, colored
Lua/Luau syntax, diagnostics and bottom action row:
**Execute · Clear · Open File · Execute File · Save File · Attach**.

Execute turns green and fades back over 0.7 seconds as activation feedback. Clear
uses the editor’s undo boundaries. Open File imports a local script into a new tab;
Execute File sends the selected file through the existing execution request.
New tabs begin with `print("Grand was here");`; existing/imported contents remain intact.

- Ctrl+T / Ctrl+N: new tab. Ctrl+W: close active tab.
- Ctrl+Tab / Ctrl+Shift+Tab: next / previous tab, wrapping at either end.
- Ctrl+S: save. Ctrl+Enter: execute.
- Ctrl+B: show/hide scripts. Ctrl+backtick: toggle console.

Synapse X is the only theme. The assistant and Agent Access entry points remain
removed. The native window caption, product metadata and application icon retain
Synapse X branding. This edition keeps its own frontend preferences and tabs.

## Source and verification

The release ZIP is the runnable package. GitHub’s source-code ZIP contains the
documentation, theme assets and [launcher source](launcher/AttachErrorHost.cs),
not the full recovered native application source. Check the download against
`SHA256SUMS.txt` on the same release.

Tests cover Attach error extraction and propagation, native structured errors,
the existing channel-repair path, confirmation timeouts, disconnect feedback,
editor controls, keyboard shortcuts, diagnostics and progress presentation.
The first-party error host was run in its test mode: its actual Windows TopMost
style, Unicode reason text, update/dismiss behavior, origin checks and session
token checks passed. A browser integration test delivered the error to that
native window. The browser preview also verified wrapping, collapse/reopen and
the connected button’s animation styles.

Package checks verify embedded frontend assets, PE resources, checksums and
relocations. The supplied core code sections retain the previous release’s
branding-only edits; native authentication and execution wrappers are preserved.
The bundled runtime’s internal-menu patch is unchanged from v1.2.3.
**The supplied native core and a live Roblox attachment have not been run.**

The theme keeps existing backend service/documentation URLs for compatibility.
See [NOTICE.txt](NOTICE.txt), [theme assets](theme/), and [launcher build notes](launcher/README.md).
