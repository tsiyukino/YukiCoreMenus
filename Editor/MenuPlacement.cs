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
    /// it; and another TsiYuki menu, which may not have been generated yet and
    /// so waits for <see cref="MenuRegistry"/>.
    ///
    /// Call it from a pass that runs before <c>moe.tsiyuki.core.menus</c>.
    /// </summary>
    public static class MenuPlacement
    {
        /// <param name="source">The component whose menu this is.</param>
        /// <param name="destination">Where the user wants it; null for the avatar's root menu.</param>
        /// <param name="menuRoot">The menu's root, as <see cref="MenuItems.Root"/> made it.</param>
        /// <param name="label">How warnings name the menu.</param>
        public static void Place(Object source, Object destination, GameObject menuRoot, string label)
        {
            MenuRegistry.Register(source, menuRoot);
            if (destination == null) return;

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

            // Otherwise it should be another TsiYuki component, whose menu may not exist yet.
            MenuRegistry.RequestMove(source, destination, label);
        }

        static void InstallUnder(Object source, GameObject destination, GameObject menuRoot, string label)
        {
            var installer = menuRoot.GetComponent<ModularAvatarMenuInstaller>();
            if (installer == null) return;
            if (InstallTarget.Add(destination.transform, menuRoot.name, installer) == null)
                MenusText.Errors.Report(ErrorSeverity.NonFatal, "warn.no_install_target", source, label);
        }
    }
}
