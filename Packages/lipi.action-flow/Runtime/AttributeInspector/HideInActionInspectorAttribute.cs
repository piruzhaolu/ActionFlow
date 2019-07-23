using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ActionFlow
{
    public class HideInActionInspectorAttribute : InspectorAttributeBase
    {
        public override int SortIndex => 10;

#if UNITY_EDITOR
        public override (bool, VisualElement) CreateUI(SerializedObject so, SerializedProperty property, VisualElement preElement)
        {
            return (false,null);
        }
#endif
    }
}
