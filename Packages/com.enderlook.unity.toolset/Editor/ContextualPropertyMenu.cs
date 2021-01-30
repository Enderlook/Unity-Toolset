using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset
{
    public static class ContextualPropertyMenu
    {
        public static event EditorApplication.SerializedPropertyCallbackFunction contextualPropertyMenu;

        static ContextualPropertyMenu() => EditorApplication.contextualPropertyMenu += (GenericMenu menu, SerializedProperty property) =>
                                         {
                                             foreach (Delegate item in contextualPropertyMenu.GetInvocationList())
                                             {
                                                 try
                                                 {
                                                     ((EditorApplication.SerializedPropertyCallbackFunction)item)(menu, property);
                                                 }
                                                 catch (Exception e)
                                                 {
                                                     Debug.LogException(e);
                                                 }
                                             }
                                         };
    }
}
