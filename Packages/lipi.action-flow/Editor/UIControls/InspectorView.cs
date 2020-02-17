using UnityEngine;
using UnityEngine.UIElements;

namespace ActionFlow
{
    public class InspectorView:VisualElement
    {
        public InspectorView()
        {
            style.backgroundColor = new StyleColor(new Color(0.22f,0.22f,0.22f,1f));
            _head = new VisualElement();
            _head.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            _head.style.backgroundColor = new StyleColor( new Color(0.12f,0.12f,0.12f,1f));
            _head.style.height = 22;
            _head.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            _head.style.paddingLeft = 5;
            Add(_head);
            var label = new Label("Inspector");
            label.style.color = new StyleColor(new Color(0.75f,0.75f,0.75f,1f));
            _head.Add(label);
            
            _inspector = new InspectorElement();
            Add(_inspector);
            
            
        }


        private readonly VisualElement _head;
        private InspectorElement _inspector;

        public void SetTarget(object obj)
        {
            _inspector.SetTarget(obj);
        }

    }
    
}