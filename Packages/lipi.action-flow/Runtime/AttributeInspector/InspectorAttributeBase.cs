using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace ActionFlow
{
    public class InspectorAttributeBase : Attribute
    {
        public virtual int SortIndex { get; }

#if UNITY_EDITOR
        public virtual (bool, VisualElement) CreateUI(SerializedObject so, SerializedProperty property, VisualElement preElement)
        {

            return (false, null);
        }
      
#endif
    }

}
