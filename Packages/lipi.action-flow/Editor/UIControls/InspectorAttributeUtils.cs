using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEditor.UIElements;

namespace ActionFlow
{
    public class InspectorAttributeUtils
    {
        public static List<InspectorAttributeBase> GetAttribute(SerializedObject so, SerializedProperty property)
        {
            var propertyPath = property.propertyPath;
            var type = so.targetObject.GetType();
            var paths = propertyPath.Split('.');
            FieldInfo fieldInfo = null;
            for (int i = 0; i < paths.Length; i++)
            {
                fieldInfo = type.GetField(paths[i]);
                type = fieldInfo.FieldType;
            }
            var objs = fieldInfo.GetCustomAttributes(false);
            List<InspectorAttributeBase> attrBaseList = new List<InspectorAttributeBase>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is InspectorAttributeBase attrBase)
                {
                    attrBaseList.Add(attrBase);
                }
            }
            attrBaseList.Sort((InspectorAttributeBase a, InspectorAttributeBase b) => (a.SortIndex > b.SortIndex) ? 1 : -1);

            return attrBaseList;
        }
    }

}
