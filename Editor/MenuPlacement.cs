using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace TsiYuki.Core.Menus.Editor
{
    /// <summary>
    /// Puts a generated menu where the user asked for it. Three kinds of
    /// destination, because three kinds of thing can hold a menu:
    ///
    /// a menu asset, which Modular Avatar's installer takes directly; an object
    /// already carrying a menu item, which takes an install target underneath
    /// it; and another TsiYuki menu — its component, or the object carrying it —
    /// which may not have been generated yet and so waits for
    /// <see cref="MenuRegistry"/>. An object carrying a menu item counts as the
    /// menu item even if a TsiYuki component is on it too.
    ///
    /// Call it from a pass declared <c>BeforePlugin&lt;MenusPlugin&gt;()</c>.
    /// </summary>
    public static class MenuPlacement
    {
        /// <param name="source">The component whose menu this is.</param>
        /// <param name="destination">Where the user wants it, usually from <see cref="MenuParentField"/>;
        /// null for the avatar's root menu.</param>
        /// <param name="menuRoot">The menu's root, as <see cref="MenuItems.Root"/> made it.</param>
        /// <param name="label">How warnings name the menu.</param>
        public static void Place(Object source, Object destination, GameObject menuRoot, string label)
        {
            MenuRegistry.Register(source, menuRoot);
            if (IsEmpty(destination)) return;

            // Gone already: a TsiYuki component whose tool ran first and removed
            // it at the end of its pass. Its menu was registered under its id,
            // which is all a destroyed object still has.
            if (destination == null)
            {
                MenuRegistry.RequestMove(source, destination, label);
                return;
            }

            var asset = destination as VRCExpressionsMenu;
            if (asset != null)
            {
                var installer = menuRoot.GetComponent<ModularAvatarMenuInstaller>();
                if (installer != null) installer.installTargetMenu = asset;
                return;
            }

            var go = destination as GameObject;
            if (go == null && destination is Component) go = ((Component)destination).gameObject;

            // An object that already carries a menu item can take the menu right away.
            if (go != null && go.GetComponent<ModularAvatarMenuItem>() != null)
            {
                InstallUnder(source, go, menuRoot, label);
                return;
            }

            // Otherwise it should be another TsiYuki menu, which may not exist yet.
            MenuRegistry.RequestMove(source, destination, label);
        }

        /// <summary>
        /// An empty field holds null, or in the editor an object with no instance
        /// behind it. A destroyed object also compares equal to null but keeps its id.
        /// </summary>
        static bool IsEmpty(Object o) => ReferenceEquals(o, null) || o.GetInstanceID() == 0;

        static void InstallUnder(Object source, GameObject destination, GameObject menuRoot, string label)
        {
            var installer = menuRoot.GetComponent<ModularAvatarMenuInstaller>();
            if (installer == null) return;
            if (InstallTarget.Add(destination.transform, menuRoot.name, installer) == null)
                MenusText.Errors.Report(ErrorSeverity.NonFatal, "warn.no_install_target", source, label);
        }
    }
}
