# Changelog

## [0.1.0] - 2026-09-26

### Added
- `MenuItems`: the Modular Avatar menu items a generated menu is built from — a root carrying the installer, a
  submenu listing its children, and a control setting a parameter to a value.
- `InstallTarget`: Modular Avatar's Menu Install Target, added by reflection since the component is internal.
- `MenuPlacement.Place`: installs a generated menu under a menu asset, an object carrying a menu item, or
  another TsiYuki menu, whichever tool runs first. Another TsiYuki menu can be named by its component — even
  when its tool ran first and has already removed it — or by the object carrying it, which is what dragging from
  the Hierarchy gives; an object carrying two is reported as ambiguous. The requests between TsiYuki menus are settled by this package's NDMF pass before Modular Avatar;
  tools order themselves with `.BeforePlugin<MenusPlugin>()`. Replaces `YukiMenuRegistry` from TsiYuki Core
  0.3.0 and the `MenuPlacement` each tool kept, which only understood the component, and only while it still
  existed. The warnings (a
  destination that makes no menu or several, a menu that would end up inside itself, a Modular Avatar without
  the install target) are reported here, in this package's words.
- `MenuParentField`: the **Install into** field for a tool's editor, with its label and explanation, so every
  tool shows the same field and describes the same rules.

### Fixed
- The table of generated menus is cleared after every build, and again before the next one in case a build
  failed halfway. `YukiMenuRegistry` never cleared it, so it kept every build's menu objects alive for the rest
  of the editor session.
