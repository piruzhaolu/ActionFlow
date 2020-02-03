using System;
using System.Collections;
using System.Reflection;
using UnityEditor.Experimental;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ActionFlow
{
    
    public class InspectorElement:ContainerItem
    {

        private object _target;

        public InspectorElement(object target)
        {
            _target = target;
            AddToClassList("Inspector");
            styleSheets.Add(Resources.Load<StyleSheet>("InspectorElement"));
            InitContainer(target,"", 0);
        }

//        public void Draw(object target, string parentPath, int indent = 0, ContainerItem container = null)
//        {
//            var type = target.GetType();
//            var fields = type.GetFields();
//            foreach (var field in fields)
//            {
//                var success = DrawBaseField(field, parentPath,indent,container);
//                if (!success) success = DrawArray(field, parentPath);
//                if (!success) DrawInstance(field, parentPath,indent,container);
//            }
//        }
//
//        public bool DrawArray(FieldInfo fieldInfo, string parentPath)
//        {
//            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
//            {
//                return true;
//            }
//            return false;
//        }
//
//        private bool DrawInstance(FieldInfo fieldInfo, string parentPath,int indent, ContainerItem container = null)
//        {
//            if (fieldInfo.FieldType.IsClass || fieldInfo.FieldType.IsValueType)
//            {
//                if (!string.IsNullOrEmpty(parentPath)) parentPath = $"{parentPath}/";
//                var path = $"{parentPath}{fieldInfo.Name}";
//                var obj = DynamicObjectUtility.GetObjectValue(_target, path);
//                if (obj == null)
//                {
//                    var newObj = Activator.CreateInstance(fieldInfo.FieldType);
//                    DynamicObjectUtility.SetObjectValue(_target, newObj, path);
//                    obj = newObj;
//                }
//                var titleItem = ContainerItem.Creat(fieldInfo.Name, obj, parentPath, indent, false);
//                if (container == null)
//                {
//                    Add(titleItem);
//                    Draw(obj,path,indent+1,  titleItem );
//                }
//                else
//                {
//                    container.AddChild(titleItem);
//                    
//                }
//                return true;
//            }
//
//            return false;
//        }
//
//        private bool DrawBaseField(FieldInfo fieldInfo, string parentPath, int indent , ContainerItem container = null)
//        {
//            VisualElement element = null;
//            var path = (string.IsNullOrEmpty(parentPath)) ? fieldInfo.Name : $"{parentPath}/{fieldInfo.Name}";
//            if (fieldInfo.FieldType == typeof(int))
//            {
//                element = PropertyItem.CreatItem<IntegerField, int>(new IntegerField(fieldInfo.Name),_target, path,indent);
//            }else if (fieldInfo.FieldType == typeof(float))
//            {
//                element = PropertyItem.CreatItem<FloatField, float>(new FloatField(fieldInfo.Name),_target, path, indent);
//            } else if (fieldInfo.FieldType == typeof(string))
//            {
//                element = PropertyItem.CreatItem<TextField, string>(new TextField(fieldInfo.Name),_target, path, indent);
//            }else if(fieldInfo.FieldType.IsEnum)
//            {
//                var field = new EnumField(fieldInfo.Name, (Enum) Activator.CreateInstance(fieldInfo.FieldType));
//                element = PropertyItem.CreatItem<EnumField, Enum>(field,_target, path, indent);
//            }else if(fieldInfo.FieldType == typeof(Object) || fieldInfo.FieldType.IsSubclassOf(typeof(Object))) {
//                var field = new ObjectField(fieldInfo.Name) {objectType = fieldInfo.FieldType};
//                element = PropertyItem.CreatItem<ObjectField, Object>(field,_target, path, indent);
//            } else if (fieldInfo.FieldType == typeof(Vector3) )
//            {
//                var field = new Vector3Field(fieldInfo.Name);
//                element = PropertyItem.CreatItem<Vector3Field, Vector3>(field,_target, path, indent);
//            }
//
//            if (element != null)
//            {
//                if (container == null)  Add(element);
//                else container.AddChild(element);
//                return true;
//            }
//            return false;
//        }

        
        
    }
}