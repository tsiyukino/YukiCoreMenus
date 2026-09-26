using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace TsiYuki.Core.Menus.Editor
{
    /// <summary>
    /// Builds a menu as a tree of Modular Avatar menu items, so Modular Avatar
    /// handles installation and paging. Every item is labelled explicitly and
    /// never takes an automatic value: the tool that builds the menu owns its
    /// parameter values.
    /// </summary>
    public static class MenuItems
    {
        /// <summary>
        /// A new object under <paramref name="host"/> carrying the installer and
        /// the submenu that lists its children.
        /// </summary>
        public static GameObject Root(Transform host, string label, Texture2D icon)
        {
            var root = new GameObject(label);
            root.transform.SetParent(host, false);
            root.AddComponent<ModularAvatarMenuInstaller>();
            MakeSubMenu(root, label, icon);
            return root;
        }

        /// <summary>A submenu that lists the children of the returned object.</summary>
        public static GameObject SubMenu(Transform parent, string label, Texture2D icon)
        {
            var go = Child(parent, label);
            MakeSubMenu(go, label, icon);
            return go;
        }

        /// <summary>A control that sets <paramref name="parameter"/> to <paramref name="value"/>.</summary>
        public static GameObject Control(Transform parent, string label, Texture2D icon,
                                         VRCExpressionsMenu.Control.ControlType type, string parameter, float value)
        {
            var go = Child(parent, label);
            var item = go.AddComponent<ModularAvatarMenuItem>();
            item.Control = new VRCExpressionsMenu.Control
            {
                name = label,
                icon = icon,
                type = type,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = parameter },
                value = value,
            };
            item.label = label;
            item.automaticValue = false;
            return go;
        }

        static GameObject Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        static void MakeSubMenu(GameObject go, string label, Texture2D icon)
        {
            var item = go.AddComponent<ModularAvatarMenuItem>();
            item.Control = new VRCExpressionsMenu.Control
            {
                name = label,
                icon = icon,
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = "" },
            };
            item.MenuSource = SubmenuSource.Children;
            item.label = label;
            item.automaticValue = false;
        }
    }
}
