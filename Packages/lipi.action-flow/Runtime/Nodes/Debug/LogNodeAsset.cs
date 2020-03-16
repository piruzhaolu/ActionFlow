using System;
using UnityEngine;

namespace ActionFlow
{
    [Serializable]
    [NodeInfo("Log")]
    public class LogNodeAsset : INodeInput,INode
    {
        public string Text;
        
        
        
       [NodeInput]
       public void OnInput(ref Context context)
       {
           Debug.Log(Text);
       }
    }
}