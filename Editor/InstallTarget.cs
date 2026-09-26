using System;
using System.Reflection;
using nadena.dev.modular_avatar.core;
using UnityEngine;

namespace TsiYuki.Core.Menus.Editor
{
    /// <summary>
    /// Modular Avatar's Menu Install Target: what its own "Select Menu" button
    /// creates to pull an installer's menu in at a chosen place. The component is
    /// internal to Modular Avatar, so it is added by reflection.
    /// </summary>
    public static class InstallTarget
    {
        static readonly Type TargetType =
            typeof(ModularAvatarMenuInstaller).Assembly.GetType("nadena.dev.modular_avatar.core.ModularAvatarMenuInstallTarget");

        /// <summary>
        /// A new object under <paramref name="parent"/> that installs
        /// <paramref name="installer"/>'s menu there, or null — with nothing
        /// created — when this Modular Avatar version has no install target.
        /// </summary>
        public static GameObject Add(Transform parent, string name, ModularAvatarMenuInstaller installer)
        {
            if (TargetType == null) return null;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var component = go.AddComponent(TargetType);
            TargetType.GetField("installer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.SetValue(component, installer);
            return go;
        }
    }
}
