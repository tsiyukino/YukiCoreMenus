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
    /// pointed it at another TsiYuki component, asks for it to be moved there.
    /// Nothing is moved at that moment, because the destination may not exist
    /// yet. <see cref="MenusPlugin"/> settles every request once all the tools
    /// have had their turn.
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
            public string DestinationName;
            public string Label;
            public Object Context;
        }

        static readonly Dictionary<int, GameObject> Roots = new Dictionary<int, GameObject>();
        static readonly List<Request> Requests = new List<Request>();

        /// <summary>"This component's menu is that object."</summary>
        public static void Register(Object source, GameObject menuRoot)
        {
            if (source == null || menuRoot == null) return;
            Roots[source.GetInstanceID()] = menuRoot;
        }

        /// <summary>"Move my menu inside the menu that component generates."</summary>
        public static void RequestMove(Object source, Object destination, string label)
        {
            if (source == null || destination == null) return;
            Requests.Add(new Request
            {
                Source = source.GetInstanceID(),
                Destination = destination.GetInstanceID(),
                DestinationName = destination.name,
                Label = label,
                Context = source,
            });
        }

        /// <summary>
        /// Settles every request, then forgets the build. A destination that
        /// generated no menu is reported and left where it was, as is anything
        /// that would make a menu contain itself.
        /// </summary>
        public static void Resolve()
        {
            foreach (var request in Requests)
            {
                GameObject sourceRoot, destinationRoot;
                if (!Roots.TryGetValue(request.Source, out sourceRoot) || sourceRoot == null) continue;

                if (!Roots.TryGetValue(request.Destination, out destinationRoot) || destinationRoot == null)
                {
                    MenusText.Errors.Report(ErrorSeverity.NonFatal, "warn.menu_parent_missing", request.Context, request.Label, request.DestinationName);
                    continue;
                }
                if (Cycles(request.Source, request.Destination))
                {
                    MenusText.Errors.Report(ErrorSeverity.NonFatal, "warn.menu_cycle", request.Context, request.Label, request.DestinationName);
                    continue;
                }
                Nest(sourceRoot, destinationRoot);
            }
            Requests.Clear();
            Roots.Clear();
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
        /// True when following "installs into" from <paramref name="destination"/>
        /// leads back to <paramref name="source"/> — a menu that would end up
        /// inside itself.
        /// </summary>
        static bool Cycles(int source, int destination)
        {
            var seen = new HashSet<int>();
            var at = destination;
            while (seen.Add(at))
            {
                if (at == source) return true;
                bool found = false;
                foreach (var request in Requests)
                    if (request.Source == at) { at = request.Destination; found = true; break; }
                if (!found) return false;
            }
            return false;
        }
    }
}
