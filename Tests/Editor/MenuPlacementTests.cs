using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace TsiYuki.Core.Menus.Editor.Tests
{
    // Components only stand in for "the TsiYuki component whose menu this is"; any component will do.
    // The registry is static, so each test starts by flushing whatever an earlier one left behind.
    public class MenuPlacementTests
    {
        readonly List<Object> _made = new List<Object>();

        [SetUp]
        public void SetUp() => Resolve();

        [TearDown]
        public void TearDown()
        {
            Resolve();
            foreach (var o in _made) if (o != null) Object.DestroyImmediate(o);
            _made.Clear();
        }

        GameObject Make(string name)
        {
            var go = new GameObject(name);
            _made.Add(go);
            return go;
        }

        // A TsiYuki-like component with its generated menu root, as a tool would have them.
        (Component source, GameObject root) Tool(string name)
        {
            var source = Make(name).AddComponent<BoxCollider>();
            var root = MenuItems.Root(Make(name + " host").transform, name, null);
            return (source, root);
        }

        static List<ErrorContext> Resolve() => ErrorReport.CaptureErrors(MenuRegistry.Resolve);

        static string[] Keys(List<ErrorContext> errors) =>
            errors.Select(e => ((SimpleError)e.TheError).TitleKey).ToArray();

        [Test]
        public void NoDestinationLeavesTheMenuAtTheRoot()
        {
            var (source, root) = Tool("A");
            var parent = root.transform.parent;
            MenuPlacement.Place(source, null, root, "A");

            Assert.That(Resolve(), Is.Empty);
            Assert.That(root.transform.parent, Is.SameAs(parent));
            Assert.IsNull(root.GetComponent<ModularAvatarMenuInstaller>().installTargetMenu);
        }

        [Test]
        public void MenuAssetBecomesTheInstallersTarget()
        {
            var (source, root) = Tool("A");
            var asset = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            _made.Add(asset);
            MenuPlacement.Place(source, asset, root, "A");
            Assert.That(root.GetComponent<ModularAvatarMenuInstaller>().installTargetMenu, Is.SameAs(asset));
        }

        [Test]
        public void ObjectWithAMenuItemGetsAnInstallTarget()
        {
            var (source, root) = Tool("A");
            var destination = MenuItems.SubMenu(Make("Existing").transform, "Existing", null);
            MenuPlacement.Place(source, destination, root, "A");

            Assert.That(destination.transform.childCount, Is.EqualTo(1));
            var target = destination.transform.GetChild(0);
            Assert.That(target.name, Is.EqualTo("A"));
            Assert.That(target.GetComponents<Component>().Any(c => c.GetType().Name == "ModularAvatarMenuInstallTarget"));
        }

        [Test]
        public void AnotherToolsMenuTakesItWhicheverRunsFirst()
        {
            var (a, rootA) = Tool("A");
            var (b, rootB) = Tool("B");
            MenuPlacement.Place(a, b, rootA, "A"); // asks before B has registered anything
            MenuPlacement.Place(b, null, rootB, "B");

            Assert.That(Resolve(), Is.Empty);
            Assert.That(rootA.transform.parent, Is.SameAs(rootB.transform));
            Assert.IsNull(rootA.GetComponent<ModularAvatarMenuInstaller>());
        }

        [Test]
        public void MenusThatWouldContainEachOtherStayPut()
        {
            var (a, rootA) = Tool("A");
            var (b, rootB) = Tool("B");
            var parentA = rootA.transform.parent;
            var parentB = rootB.transform.parent;
            MenuPlacement.Place(a, b, rootA, "A");
            MenuPlacement.Place(b, a, rootB, "B");

            Assert.That(Keys(Resolve()), Is.EqualTo(new[] { "warn.menu_cycle", "warn.menu_cycle" }));
            Assert.That(rootA.transform.parent, Is.SameAs(parentA));
            Assert.That(rootB.transform.parent, Is.SameAs(parentB));
        }

        [Test]
        public void DestinationWithoutAMenuIsReported()
        {
            var (a, rootA) = Tool("A");
            var silent = Make("Silent").AddComponent<BoxCollider>();
            var parent = rootA.transform.parent;
            MenuPlacement.Place(a, silent, rootA, "A");

            Assert.That(Keys(Resolve()), Is.EqualTo(new[] { "warn.menu_parent_missing" }));
            Assert.That(rootA.transform.parent, Is.SameAs(parent));
        }

        [Test]
        public void NothingCarriesOverToTheNextBuild()
        {
            var (b, rootB) = Tool("B");
            MenuPlacement.Place(b, null, rootB, "B");
            Resolve();

            var (a, rootA) = Tool("A");
            MenuPlacement.Place(a, b, rootA, "A");
            Assert.That(Keys(Resolve()), Is.EqualTo(new[] { "warn.menu_parent_missing" }));
            Assert.That(rootA.transform.parent, Is.Not.SameAs(rootB.transform));
        }
    }
}
