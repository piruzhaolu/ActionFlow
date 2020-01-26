using UnityEngine.UIElements;

namespace ActionFlow
{
    public class PropertyItem:VisualElement
    {
        public const float IndentWidth = 5f;
        public PropertyItem()
        {
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            _indentElement = new VisualElement();
            //_indentElement.style.width = indent * IndentWidth; 
            Add(_indentElement);
        }

        private readonly VisualElement _indentElement;


        public static PropertyItem CreatItem<T>(string label, int indent)
        {
            var item = new PropertyItem();
            
            return item;
        }

    }
}