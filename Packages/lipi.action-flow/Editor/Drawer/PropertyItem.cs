using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ActionFlow
{
    public class PropertyItem:VisualElement
    {
        public const float IndentWidth = 16f;
        public PropertyItem()
        {
            _indentElement = new VisualElement();
            Add(_indentElement);
        }

        private readonly VisualElement _indentElement;
        private VisualElement _element;
        private object _target;

        private void AddElement(VisualElement element)
        {
            _element = element;
            Add(element);
        }
        
        private void RegisterChangeEvent<T>(BaseField<T> element)
        {
            element.RegisterCallback<ChangeEvent<T>>(delegate(ChangeEvent<T> evt) {  
                if (evt.currentTarget is BindableElement e)
                {
                    DynamicObjectUtility.SetObjectValue(_target,evt.newValue, e.bindingPath);
                }
            });
        }


        public static PropertyItem CreatItem<T,TValue>(T element, object target, string path, int indent) where T:BaseField<TValue>,new()
        {
            var item = new PropertyItem();
            item._indentElement.style.width = indent * IndentWidth;
            item._target = target;
            item.RegisterChangeEvent<TValue>(element);
            element.bindingPath = path;
            element.value = (TValue) DynamicObjectUtility.GetObjectValue(target, path);
            item.AddElement(element);
            return item;
        }


        public static PropertyItem CreatTitleItem(string label,object target, string path, int indent, bool expand, Action<bool,bool> expandCallback)
        {
            var item = new PropertyItem();
            item._indentElement.style.width = indent * IndentWidth;
            item._target = target;
            item.AddToClassList("title");
            
            
            var ve = new VisualElement();
            var toggle = new Foldout {value = expand, text = label};
            toggle.RegisterValueChangedCallback(delegate(ChangeEvent<bool> evt) { expandCallback(evt.previousValue, evt.newValue); });
            item.Add(toggle);
            return item;
        }
       

    }
}