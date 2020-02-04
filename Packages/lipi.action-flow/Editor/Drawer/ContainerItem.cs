using System;
using System.Collections;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ActionFlow
{
    public class ContainerItem:VisualElement
    {


        internal void InitContainer(object target, string path, int indent)
        {
            _target = target;
            _parentPath = path;
            _indent = indent;
            _container = new VisualElement();
            Add(_container);
            DrawChild();
        }
        
        
        
        private object _target;
        private string _parentPath;
        private int _indent;
        private VisualElement _container;
        private PropertyItem _propertyItem;

//        public void AddChild(VisualElement element)
//        {
//            _container?.Add(element);
//        }

        
        
        private void DrawChild()
        {
            var type = _target.GetType();
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var success = DrawBaseField(field, _parentPath,_indent);
                if (!success) success = DrawArray(field, _parentPath);
                if (!success) DrawInstance(field, _parentPath,_indent);
            }
        }
        private bool DrawArray(FieldInfo fieldInfo, string parentPath)
        {
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
            {
                return true;
            }
            return false;
        }
        
        private bool DrawInstance(FieldInfo fieldInfo, string parentPath,int indent)
        {
            if (fieldInfo.FieldType.IsClass || fieldInfo.FieldType.IsValueType)
            {
                if (!string.IsNullOrEmpty(parentPath)) parentPath = $"{parentPath}/";
                var path = $"{parentPath}{fieldInfo.Name}";
                var obj = DynamicObjectUtility.GetObjectValue(_target, path);
                if (obj == null)
                {
                    var newObj = Activator.CreateInstance(fieldInfo.FieldType);
                    DynamicObjectUtility.SetObjectValue(_target, newObj, path);
                    obj = newObj;
                }
                var titleItem = ContainerItem.Creat(fieldInfo.Name, obj, parentPath, indent, false);
                _container.Add(titleItem);
                    
                return true;
            }

            return false;
        }
        
        private bool DrawBaseField(FieldInfo fieldInfo, string parentPath, int indent )
        {
            VisualElement element = null;
            var path = (string.IsNullOrEmpty(parentPath)) ? fieldInfo.Name : $"{parentPath}/{fieldInfo.Name}";
            if (fieldInfo.FieldType == typeof(int))
            {
                element = PropertyItem.CreatItem<IntegerField, int>(new IntegerField(fieldInfo.Name),_target, path,indent);
            }else if (fieldInfo.FieldType == typeof(float))
            {
                element = PropertyItem.CreatItem<FloatField, float>(new FloatField(fieldInfo.Name),_target, path, indent);
            } else if (fieldInfo.FieldType == typeof(string))
            {
                element = PropertyItem.CreatItem<TextField, string>(new TextField(fieldInfo.Name),_target, path, indent);
            }else if(fieldInfo.FieldType.IsEnum)
            {
                var field = new EnumField(fieldInfo.Name, (Enum) Activator.CreateInstance(fieldInfo.FieldType));
                element = PropertyItem.CreatItem<EnumField, Enum>(field,_target, path, indent);
            }else if(fieldInfo.FieldType == typeof(Object) || fieldInfo.FieldType.IsSubclassOf(typeof(Object))) {
                var field = new ObjectField(fieldInfo.Name) {objectType = fieldInfo.FieldType};
                element = PropertyItem.CreatItem<ObjectField, Object>(field,_target, path, indent);
            } else if (fieldInfo.FieldType == typeof(Vector3) )
            {
                var field = new Vector3Field(fieldInfo.Name);
                element = PropertyItem.CreatItem<Vector3Field, Vector3>(field,_target, path, indent);
            }

            if (element != null)
            {
                if (_container == null) Add(element);
                else _container.Add(element);
                return true;
            }
            return false;
        }


        private void ExpandCallback(bool previousValue, bool newValue)
        {
            if (previousValue != newValue)
            {
                if (newValue)
                {
                    DrawChild();
                }
                else
                {
                    var count = _container.childCount;
                    for (var i = count-1; i>=0; i--)
                    {
                        _container.RemoveAt(i);
                    }
                }
            }
        }
        
        public static ContainerItem Creat(string label,object target, string path, int indent, bool expand)
        {
            var item = new ContainerItem();
            var propertyItem = PropertyItem.CreatTitleItem(label, target, path, indent, expand, item.ExpandCallback);
            item.Add(propertyItem);
            item._propertyItem = propertyItem;
            var ve = new VisualElement();
            item._container = ve;
            item.Add(ve);

            item._target = target;
            item._parentPath = path;
            item._indent = indent+1;
            return item;
        }
        
    }
}