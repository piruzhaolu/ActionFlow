using UnityEngine;
using UnityEngine.UIElements;

namespace ActionFlow
{
    public class ActionNodeStatusBar:VisualElement
    {
        public ActionNodeStatusBar()
        {
            style.position = new StyleEnum<Position>(Position.Absolute);
            style.left = 0;
            style.right = 0;
            style.bottom = 0;
            style.height = 2;
            style.backgroundColor = s_NoEntry;
            
            _background = new VisualElement();
            _background.style.width = new StyleLength(new Length(0, LengthUnit.Percent));
            _background.style.flexGrow = 1;
            _background.style.backgroundColor = s_Run1;
            Add(_background);
        }

        private readonly VisualElement _background;
        private float _lastTime = -1;
        
        private static readonly StyleColor s_NoEntry = new StyleColor(new Color(0,0,0,0));
        private static readonly StyleColor s_Entry = new StyleColor(new Color(0.5f,0.5f,0.5f,1));
        private static readonly StyleColor s_Running = new StyleColor(new Color(0.0f, 0.7f, 0.0f, 1));
        private static readonly StyleColor s_Run1 = new StyleColor(new Color(0.8f, 0.9f, 0.4f, 0.7f));
        private static readonly StyleColor s_Run2 = new StyleColor(new Color(0.4f, 0.9f, 0.8f, 0.7f));

        public void End()
        {
            style.backgroundColor = s_NoEntry;
            _background.style.width = new StyleLength(new Length(0, LengthUnit.Percent));
        }

        public void SetStatus(bool running, float value)
        {
            if (value >= 0)
            {
                if (Time.realtimeSinceStartup - value > 2f)
                {
                    _background.style.width = 0;
                }
                else
                {
                    if (value - _lastTime > 1f)
                    {
                        _lastTime = Time.realtimeSinceStartup;
                        if (Time.frameCount % 2 == 0)
                        {
                            _background.style.backgroundColor = s_Run1;
                        }
                        else
                        {
                            _background.style.backgroundColor = s_Run2;
                        }
                    }
                    var spaceTime = Time.realtimeSinceStartup - _lastTime;
                    _background.style.width = new StyleLength(new Length(spaceTime*100, LengthUnit.Percent));
                }
                
                
                if (running)
                {
                    style.backgroundColor = s_Running;
                }
                else
                {
                    style.backgroundColor = s_Entry;
                }
            }
            
            
            
            
            
        }


    }
    
    
}