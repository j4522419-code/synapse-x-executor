# Changelog

## 1.3.6 — encrypted download and one runtime folder

- Use one app/core/bin folder; remove the root bin and mirror/seed operations.
- Retain the original vendor filenames and the exact v1.3.5 runtime bytes.
- Preserve runtime/settings files during startup; report an incomplete extraction clearly.
- Set the runtime folder's ordinary Hidden attribute in the package and refresh it at startup/preparation.
- Distribute a password-protected 7z archive with encrypted filenames; the password is shared privately by the distributor.
- Verify password rejection, extraction, attributes, one-folder layout, unchanged binaries and updater behavior with disposable fixtures.
- Require a fresh extraction; do not delete an existing installation's files automatically.
- Remain a compatibility prerelease: the reported injection error is not confirmed fixed.

## 1.3.5 — compatibility candidate

- Reverse v1.3.4's native runtime filename changes while retaining visible Synapse X filenames.
- Seed original-name compatibility copies in app/core/bin before native startup.
- Keep visible branded files in the root bin folder and mirror prepared updates.
- Preserve existing compatibility copies rather than downgrading them on every launch.
- Bundle the exact submitted vendor 2.5.4 loader and v1.3.4 patched DLL, without new runtime code changes.
- Retain runtime identity/version 1.3.4-grand so this test isolates filename/layout compatibility.
- Check layout creation, copies, mirrors, settings, bootstrap, updater and package integrity with disposable fixtures.
- Live injection remains untested; error 2147549194 is not yet confirmed fixed.

## 1.3.4

- Correct purchase permission sizing, selection borders, focus outlines and narrow-window wrapping.
- Add Game settings with the existing RakNet switch and preserve its saved value.
- Set native taskbar relaunch properties and group the window under the launcher identity.
- Redirect app/Synapse X.exe to the root launcher automatically.
- Migrate pins for this installation and exact known previous builds, with backups and unrelated-pin preservation.
- Keep the native interface and dependencies in app/core with the original executable basename and working-directory relationship.
- Rename the bin runtime files to Synapse X.dll and Synapse X.exe; update native download/launch references and preparation paths.
- Include current and legacy catalog rules. Updated identity rules report SYNAPSE X MADE BY GRAND / 1.3.4-grand.
- Verify the layout in the browser and taskbar/bootstrap behavior with first-party fixtures. No anti-cheat bypass or live attachment test.

## Runtime catalog update — 2026-10-17

- Add a hash-matched rule for the vendor 2.5.4 runtime (`6f25af29fee6`).
- Resolve its missing-patch startup error in existing v1.3.3 installations without replacing their launcher.
- Inspect the new function locations and protocol constants; preserve the requested identity and menu removal.
- Verify both identity paths in isolated emulation and the existing launcher updater with disposable copies.
- Keep the previous runtime rule and all other runtime bytes unchanged.

## 1.3.3

- Use SYNAPSE X MADE BY GRAND / 1.3.3-grand for both native Lua identification aliases.
- Change the separate default HTTP User-Agent from the vendor name/version to SYNAPSE X MADE BY GRAND/1.3.3-grand.
- Restore the original startup compatibility downloader, followed by launcher verification before native initialization.
- Fetch version-matched branding/menu patches from the repository catalog, with an embedded offline fallback.
- Verify full source/output hashes and patch bytes, retain a backup and replace the runtime atomically.
- Stop preparation with a clear error for unknown runtimes until a matching catalog entry is published.
- Keep the custom interface, native authentication and execution wrappers, and existing Settings preferences.
- Verify identity in isolated emulation and updater behavior with disposable files; no live client or fresh sUNC report was run.

## 1.3.2

- Rewrite all remaining Settings descriptions and remove unlisted controls/pages.
- Fix the theme to Synapse X and remove its selector.
- Default the seven requested editor switches to on, including once on upgrade, while preserving later choices.
- Preserve enabled hidden defaults and console behavior; keep the saved scrollback limit without its control.
- Replace the purchase prompt hand with an Ask permission question-message icon.
- Replace Account with a dedicated Sign out page wired to the existing native logout flow.
- Embed requireAdministrator in the root launcher so Windows requests elevation on launch.
- Update identification metadata to Synapse X / 1.3.2-grand.

## 1.3.1

- Fix the home layout overriding Vue’s hidden state and covering newly created script editors.
- Restore the main Start tab; New Script, the plus button and Ctrl+T create separate file tabs.
- Report Synapse X / 1.3.1-grand through identifyexecutor and getexecutorname.
- Limit the runtime change to identification metadata and preserve the original Lua string helper, stack frame, aliases and all other runtime bytes.
- Verify New Script and editor input in the browser, and the native identity function in isolated CPU emulation.

## 1.3.0

- Add a subtle outward green pulse while the client connection is confirmed; clear it on disconnect.
- Show the runtime Attach failure reason in a small native always-on-top window.
- Add a first-party launcher/error host, with a guarded loopback bridge and in-app fallback.
- Simplify the start page, put Tip of the Day below Start and add a large Synapse X logo.
- Widen the files pane, wrap filenames/section labels, and animate collapse/reopen.
- Keep collapsed contents out of keyboard navigation and honor reduced motion.
- Package the native core and its dependencies in `app/`; launch the root Synapse X.exe.

## 1.2.4

- Add Ctrl+Tab / Ctrl+Shift+Tab to select the next / previous workspace tab in displayed order.
- Wrap at either end, handle zero or one open tab, and retain the existing Settings shortcut guard.

## 1.2.3

- Remove the native runtime popup renderer and prevent its key from toggling cursor/menu state.
- Bundle the inspected runtime and skip vendor runtime downloads that would overwrite the UI patch.
- Fix the trailing `m` in the actual native window-caption constructor to read Synapse X.
- Seed new script tabs with `print("Grand was here");` through both Ctrl+T and the new-tab button.
- Preserve existing/opened script contents, authentication and script execution.

## 1.2.2

- Remove the Agent Access Settings category, page, search rows and integration UI.
- Disable persisted MCP enablement when loading this edition.
- Set the internal menu key binding to none (`menu_keybind: 0`) in the configuration snapshot and bundled runtime settings.

## 1.2.1

- Replace Execute jitter and trails with green activation feedback that fades back over 0.7 seconds.
- Restore the button on repeated clicks or when the window is hidden.
- Keep the label still and readable; reduced motion uses a brief static color cue.

## 1.2.0

- Replace the previous loading presentation with three filling bars.
- Restyle sign-in and show filling bars during credential checks.
- Add capture-phase Ctrl+T / Ctrl+W for creating and closing script tabs.
- Replace fractured-letter Execute effects with 50 jitter steps in 500 ms and a brief trail.
- Patch the native product-name constants missed by ordinary string replacement.
- Rebrand native display messages, package naming and Windows resources.
- Preserve authentication, real progress events, execution and attachment behavior.

## 1.1.0

- Rebrand runtime release-note text to Synapse X while preserving link targets.
- Patch native display-name strings and keep the window caption set to Synapse X.
- Restrict the theme list to Synapse X and remove custom-theme controls.
- Remove assistant entry points, its panel and shortcut, and restored assistant tabs.
- Add colored Lua/Luau tokens and enable existing diagnostics/autocomplete on upgrade.
- Animate Execute with a rightward swish, fractured glyphs, and accelerated fall.
- Show pulsing green Attach feedback after confirmed connection; red on failure.
- Clear green feedback on disconnection and respect reduced-motion preferences.

## 1.0.0

- Initial classic gray layout, right-hand scripts pane and six-button action row.
- Synapse X window/executable/product branding and a dark-background logo.
