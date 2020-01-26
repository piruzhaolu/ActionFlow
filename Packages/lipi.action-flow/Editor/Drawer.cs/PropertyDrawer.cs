using System;
using System.Collections;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ActionFlow
{
    
    public class PropertyDrawer:VisualElement
    {

        private object _target;
//        private string _path;
//        
//        public void Bind(object target)
//        {
//            Bind(target, string.Empty);
//        }
//
//        public void Bind(object target, string path)
//        {
//            _target = target;
//            _path = path;
//            Draw(target,path);
//
//        }

        public PropertyDrawer(object target)
        {
            _target = target;
            Draw(target,"");
        }

        private void Draw(object target, string parentPath, int indent = 0, string lable = "", PropertyToggleMode mode = PropertyToggleMode.None)
        {
            var type = target.GetType();
            if (mode == PropertyToggleMode.On)
            {
                DrawToggle(true, lable);
                var fields = type.GetFields();
                foreach (var field in fields)
                {
                    var success = DrawBaseField(field,parentPath);
                    if (!success) success = DrawArray(field, parentPath);
                    if (!success) DrawInstance(field, parentPath);
                }
            }
            else if (mode == PropertyToggleMode.Off)
            {
                DrawToggle(false, lable);
            }
            else
            {
                var fields = type.GetFields();
                foreach (var field in fields)
                {
                    var success = DrawBaseField(field, parentPath);
                    if (!success) success = DrawArray(field, parentPath);
                    if (!success) DrawInstance(field, parentPath);
                }
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

        private bool DrawInstance(FieldInfo fieldInfo, string parentPath)
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
                Draw(obj,path,0, fieldInfo.Name, PropertyToggleMode.On );
                return true;
            }

            return false;
        }

        private bool DrawBaseField(FieldInfo fieldInfo, string parentPath )
        {
            BindableElement element = null;
            if (fieldInfo.FieldType == typeof(int))
            {
                element = RegisterChangeEvent(new IntegerField(fieldInfo.Name));
            }else if (fieldInfo.FieldType == typeof(float))
            {
                element = RegisterChangeEvent(new FloatField(fieldInfo.Name));
            } else if (fieldInfo.FieldType == typeof(string))
            {
                element = RegisterChangeEvent(new TextField(fieldInfo.Name));
            }else if(fieldInfo.FieldType.IsEnum)
            {
                var field = new EnumField(fieldInfo.Name, (Enum) Activator.CreateInstance(fieldInfo.FieldType));
                element = RegisterChangeEvent(field);
            }else if(fieldInfo.FieldType == typeof(Object) || fieldInfo.FieldType.IsSubclassOf(typeof(Object))) {
                var field = new ObjectField(fieldInfo.Name) {objectType = fieldInfo.FieldType};
                element = RegisterChangeEvent(field);
            }

            if (element != null)
            {
                if (string.IsNullOrEmpty(parentPath))
                {
                    element.bindingPath = fieldInfo.Name;
                }
                else
                {
                    element.bindingPath = $"{parentPath}/{fieldInfo.Name}" ;
                }
                Add(element);
                return true;
            }
            return false;
        }
        
        private BindableElement RegisterChangeEvent<T>(BaseField<T> element)
        {
            element.RegisterCallback<ChangeEvent<T>>(delegate(ChangeEvent<T> evt) {  
                if (evt.currentTarget is BindableElement e)
                {
                    DynamicObjectUtility.SetObjectValue(_target,evt.newValue, e.bindingPath);
                }
            });
            return element;
        }

        private void DrawToggle(bool toggleValue, string labelValue)
        {
            var ve = new VisualElement();
            var toggle = new Toggle();
            toggle.value = toggleValue;
            var label = new Label(labelValue);
            ve.Add(toggle);
            ve.Add(label);
            Add(ve);
        }
        
        
    }
}