using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace TsiYuki.Core.Menus.Editor.Tests
{
    public class MenuItemsTests
    {
        GameObject _host;
        readonly List<Object> _made = new List<Object>();

        [SetUp]
        public void SetUp() => _host = new GameObject("MenuItemsTests");

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            foreach (var o in _made) if (o != null) Object.DestroyImmediate(o);
            _made.Clear();
        }

        [Test]
        public void RootCarriesTheInstallerAndListsItsChildren()
        {
            var icon = new Texture2D(1, 1);
            _made.Add(icon);
            var root = MenuItems.Root(_host.transform, "Wardrobe", icon);

            Assert.That(root.transform.parent, Is.SameAs(_host.transform));
            Assert.That(root.name, Is.EqualTo("Wardrobe"));
            Assert.IsNotNull(root.GetComponent<ModularAvatarMenuInstaller>());
            var item = root.GetComponent<ModularAvatarMenuItem>();
            Assert.That(item.Control.type, Is.EqualTo(VRCExpressionsMenu.Control.ControlType.SubMenu));
            Assert.That(item.MenuSource, Is.EqualTo(SubmenuSource.Children));
            Assert.That(item.Control.icon, Is.SameAs(icon));
            Assert.That(item.label, Is.EqualTo("Wardrobe"));
            Assert.IsFalse(item.automaticValue);
        }

        [Test]
        public void SubMenuHasNoInstaller()
        {
            var sub = MenuItems.SubMenu(_host.transform, "Colors", null);
            Assert.That(sub.transform.parent, Is.SameAs(_host.transform));
            Assert.IsNull(sub.GetComponent<ModularAvatarMenuInstaller>());
            Assert.That(sub.GetComponent<ModularAvatarMenuItem>().MenuSource, Is.EqualTo(SubmenuSource.Children));
        }

        [TestCase(VRCExpressionsMenu.Control.ControlType.Toggle, 3f)]
        [TestCase(VRCExpressionsMenu.Control.ControlType.Button, 0f)]
        public void ControlSetsItsParameterToItsValue(VRCExpressionsMenu.Control.ControlType type, float value)
        {
            var go = MenuItems.Control(_host.transform, "Red", null, type, "Wardrobe/abc", value);
            var item = go.GetComponent<ModularAvatarMenuItem>();

            Assert.That(go.name, Is.EqualTo("Red"));
            Assert.That(item.Control.type, Is.EqualTo(type));
            Assert.That(item.Control.parameter.name, Is.EqualTo("Wardrobe/abc"));
            Assert.That(item.Control.value, Is.EqualTo(value));
            Assert.IsNull(item.Control.icon);
            Assert.IsFalse(item.automaticValue);
        }

        [Test]
        public void InstallTargetPointsAtTheInstaller()
        {
            var installer = MenuItems.Root(_host.transform, "Outfit menu", null).GetComponent<ModularAvatarMenuInstaller>();
            var target = InstallTarget.Add(_host.transform, "Outfit menu", installer);

            Assert.IsNotNull(target);
            Assert.That(target.transform.parent, Is.SameAs(_host.transform));
            var component = target.GetComponents<Component>().Single(c => c.GetType().Name == "ModularAvatarMenuInstallTarget");
            var field = component.GetType().GetField("installer",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            Assert.That(field.GetValue(component), Is.SameAs(installer));
        }
    }
}
