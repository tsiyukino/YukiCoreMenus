using TsiYuki.Core.Editor;

namespace TsiYuki.Core.Menus.Editor
{
    // The placement warnings (Localization/<code>.txt), reported in NDMF's error window.
    internal static class MenusText
    {
        public const string Package = "moe.tsiyuki.core.menus";
        public static readonly YukiLocalizer L = new YukiLocalizer(Package);
        public static readonly YukiNdmfReport Errors = new YukiNdmfReport(L);
    }
}
