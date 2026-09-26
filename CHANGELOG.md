# Changelog

## [0.1.0] - 2026-09-26

### Added
- `MenuItems`: the Modular Avatar menu items a generated menu is built from — a root carrying the installer, a
  submenu listing its children, and a control setting a parameter to a value.
- `InstallTarget`: Modular Avatar's Menu Install Target, added by reflection since the component is internal.
- `MenuPlacement.Place`: installs a generated menu under a menu asset, an object carrying a menu item, or
  another TsiYuki menu, whichever tool runs first. The requests between TsiYuki menus are settled by this
  package's NDMF pass (`moe.tsiyuki.core.menus`) before Modular Avatar; tools order themselves before it.
  Replaces `YukiMenuRegistry` from TsiYuki Core 0.3.0 and the `MenuPlacement` each tool kept. The warnings
  (a destination that makes no menu, a menu that would end up inside itself, a Modular Avatar without the
  install target) are reported here, in this package's words.

### Fixed
- The table of generated menus is cleared after every build. `YukiMenuRegistry` never cleared it, so it kept
  every build's menu objects alive for the rest of the editor session.
