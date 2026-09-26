using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;

namespace TsiYuki.Core.Menus.Editor
{
    /// <summary>
    /// Lets one TsiYuki tool install its menu inside a menu another one
    /// generates, without either having to run first.
    ///
    /// During a build each tool registers the menu it generated and, if the user
    /// pointed it at another TsiYuki menu, asks for it to be moved there.
    /// Nothing is moved at that moment, because the destination may not exist
    /// yet. <see cref="MenusPlugin"/> settles every request once all the tools
    /// have had their turn.
    ///
    /// The user may point at the TsiYuki component or at the object carrying it
    /// (dragging an object from the Hierarchy gives the object), so each menu is
    /// also listed under the object its component is on.
    ///
    /// Everything is keyed by instance id, captured while the components are
    /// still alive: a tool removes its own component at the end of its pass, and
    /// a destroyed component compares equal to null, so holding the references
    /// would mean losing the table before it is ever read.
    /// </summary>
    internal static class MenuRegistry
    {
        class Request
        {
            public int Source;
            public int Destination;
            public int? DestinationObject;
            public string DestinationName;
            public string Label;
            public Object Context;

            // Filled in by Settle: the menu the destination names, or why there is none.
            public int? Target;
            public string Problem;
        }

        static readonly Dictionary<int, GameObject> Roots = new Dictionary<int, GameObject>();
        static readonly Dictionary<int, string> Names = new Dictionary<int, string>();
        static readonly Dictionary<int, List<int>> MenusOnObject = new Dictionary<int, List<int>>();
        static readonly List<Request> Requests = new List<Request>();

        /// <summary>"This component's menu is that object."</summary>
        public static void Register(Object source, GameObject menuRoot)
        {
            if (source == null || menuRoot == null) return;
            var id = source.GetInstanceID();
            Roots[id] = menuRoot;
            Names[id] = source.name;

            var component = source as Component;
            if (component == null) return;
            List<int> menus;
            if (!MenusOnObject.TryGetValue(component.gameObject.GetInstanceID(), out menus))
                MenusOnObject[component.gameObject.GetInstanceID()] = menus = new List<int>();
            if (!menus.Contains(id)) menus.Add(id);
        }

        /// <summary>
        /// "Move my menu inside the menu that component, or the one on that
        /// object, generates." The destination may already be destroyed, when
        /// its tool ran first; only its id is read then.
        /// </summary>
        public static void RequestMove(Object source, Object destination, string label)
        {
            if (source == null || ReferenceEquals(destination, null)) return;
            var alive = destination != null;
            var component = alive ? destination as Component : null;
            var go = component != null ? component.gameObject : alive ? destination as GameObject : null;
            Requests.Add(new Request
            {
                Source = source.GetInstanceID(),
                Destination = destination.GetInstanceID(),
                DestinationObject = go != null ? go.GetInstanceID() : (int?)null,
                DestinationName = alive ? destination.name : null,
                Label = label,
                Context = source,
            });
        }

        /// <summary>
        /// Settles every request, then forgets the build. A destination that
        /// names no menu, or more than one, is reported and the menu left where
        /// it was, as is anything that would make a menu contain itself.
        /// </summary>
        public static void Resolve()
        {
            // Cycles follow menus, not whatever the user picked, so every
            // destination is turned into a menu first.
            foreach (var request in Requests) Settle(request);

            foreach (var request in Requests)
            {
                GameObject sourceRoot;
                if (!Roots.TryGetValue(request.Source, out sourceRoot) || sourceRoot == null) continue;

                if (request.Problem != null) Warn(request.Problem, request);
                else if (Cycles(request.Source, request.Target.Value)) Warn("warn.menu_cycle", request);
                else Nest(sourceRoot, Roots[request.Target.Value]);
            }
            Reset();
        }

        /// <summary>Forgets every menu and request, so a build starts from nothing.</summary>
        public static void Reset()
        {
            Roots.Clear();
            Names.Clear();
            MenusOnObject.Clear();
            Requests.Clear();
        }

        /// <summary>The destination's own menu if it made one, else the one menu made on its object.</summary>
        static void Settle(Request request)
        {
            if (Has(request.Destination)) { request.Target = request.Destination; return; }

            var menus = new List<int>();
            List<int> onObject;
            if (request.DestinationObject.HasValue && MenusOnObject.TryGetValue(request.DestinationObject.Value, out onObject))
                foreach (var id in onObject)
                    if (Has(id)) menus.Add(id);

            if (menus.Count == 1) request.Target = menus[0];
            else request.Problem = menus.Count == 0 ? "warn.menu_parent_missing" : "warn.menu_parent_ambiguous";
        }

        static bool Has(int source)
        {
            GameObject root;
            return Roots.TryGetValue(source, out root) && root != null;
        }

        static void Warn(string key, Request request)
        {
            string registered;
            var name = request.DestinationName ?? (Names.TryGetValue(request.Destination, out registered) ? registered : "?");
            MenusText.Errors.Report(ErrorSeverity.NonFatal, key, request.Context, request.Label, name);
        }

        /// <summary>
        /// The whole menu becomes a child of the destination's. Its root lists
        /// its children as its submenu, so being a child is all it takes — and
        /// the menu's own installer has to go, or it would also appear at the
        /// avatar's root.
        /// </summary>
        static void Nest(GameObject sourceRoot, GameObject destinationRoot)
        {
            var installer = sourceRoot.GetComponent<ModularAvatarMenuInstaller>();
            if (installer != null) Object.DestroyImmediate(installer);
            sourceRoot.transform.SetParent(destinationRoot.transform, false);
        }

        /// <summary>
        /// True when following "installs into" from <paramref name="target"/>
        /// leads back to <paramref name="source"/> — a menu that would end up
        /// inside itself.
        /// </summary>
        static bool Cycles(int source, int target)
        {
            var seen = new HashSet<int>();
            int? at = target;
            while (at.HasValue && seen.Add(at.Value))
            {
                if (at.Value == source) return true;
                int? next = null;
                foreach (var request in Requests)
                    if (request.Source == at.Value) { next = request.Target; break; }
                at = next;
            }
            return false;
        }
    }
}
