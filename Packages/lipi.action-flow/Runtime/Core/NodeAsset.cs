using UnityEngine;
using System.Collections;

namespace ActionFlow
{

    public class NodeAsset<T> : ScriptableObject, INodeAsset where T:INode
    {
        public T Value;


        public INode GetValue()
        {
            return Value;
        }
        
    }
}
