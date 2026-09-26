# TsiYuki Core Menus

Modular Avatar menu building and placement shared by TsiYuki tools that generate menus: menu items, and installing a generated menu under a menu asset, an object carrying a menu item, or another TsiYuki menu, whichever tool runs first.

Part of the TsiYuki Core family: VCC installs it along with the tools that need it, so there is no reason to add it by hand.

## For tool authors

- Build the menu with `MenuItems.Root`, `MenuItems.SubMenu` and `MenuItems.Control`.
- Draw the user's **Install into** setting with `MenuParentField.Draw`; its label and explanation come from
  this package, so every tool shows the same field.
- At build time, call `MenuPlacement.Place(component, installInto, menuRoot, label)` from a pass declared
  `.BeforePlugin<MenusPlugin>()`.

What **Install into** can hold:

| Destination | Result |
| --- | --- |
| empty | the avatar's root menu |
| a menu asset | Modular Avatar's installer puts the menu in it |
| an object with a Modular Avatar menu item | an install target under that object |
| another TsiYuki menu's component | inside that menu |
| the object carrying another TsiYuki menu, or any other component on it | inside that menu; an object carrying more than one TsiYuki menu is reported as ambiguous |

An object with a Modular Avatar menu item counts as the menu item even when a TsiYuki component is on it too. A
destination that makes no menu, or would put a menu inside itself, is reported in NDMF's error window and the
menu stays at the root.

## License

MIT

---

## 中文

TsiYuki 中生成菜单的工具共用的 Modular Avatar 菜单构建与放置：生成菜单项，并把生成的菜单装进菜单资源、带菜单项的对象，或另一个 TsiYuki 菜单里（不论哪个工具先执行）。

属于 TsiYuki Core 系列。安装需要它的工具时 VCC 会自动带上，不需要单独安装。
