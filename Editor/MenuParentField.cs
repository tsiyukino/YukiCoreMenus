using UnityEditor;
using UnityEngine;

namespace TsiYuki.Core.Menus.Editor
{
    /// <summary>
    /// The "Install into" field, for a tool's editor. Anything can be dropped
    /// in; <see cref="MenuPlacement"/> decides at build time what it means, so
    /// the field and its explanation live here with the rules they describe.
    /// </summary>
    public static class MenuParentField
    {
        /// <summary>Draws the field with <c>EditorGUILayout</c> and returns what it holds now.</summary>
        public static Object Draw(Object current) =>
            EditorGUILayout.ObjectField(new GUIContent(MenusText.L["ui.menu_parent"], MenusText.L["ui.menu_parent.tip"]),
                                        current, typeof(Object), true);
    }
}
