using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(TsiYuki.Core.Menus.Editor.MenusPlugin))]

namespace TsiYuki.Core.Menus.Editor
{
    /// <summary>
    /// Settles the menu moves every TsiYuki tool asked for.
    ///
    /// Each tool's pass declares <c>BeforePlugin&lt;MenusPlugin&gt;()</c>, so by
    /// the time this runs they have all generated their menus and a request can
    /// point at any of them regardless of which ran first. It still lands
    /// before Modular Avatar, which is what installs the result.
    ///
    /// The table is also emptied before any tool runs: a build that failed
    /// halfway never reached the pass that would have emptied it.
    /// </summary>
    public class MenusPlugin : Plugin<MenusPlugin>
    {
        public override string QualifiedName => "moe.tsiyuki.core.menus";
        public override string DisplayName => "TsiYuki Core Menus";

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving)
                .Run("Forget earlier builds", _ => MenuRegistry.Reset());

            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("Place TsiYuki menus", _ => MenuRegistry.Resolve());
        }
    }
}
