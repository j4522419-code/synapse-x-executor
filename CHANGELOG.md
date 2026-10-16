# Changelog

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
