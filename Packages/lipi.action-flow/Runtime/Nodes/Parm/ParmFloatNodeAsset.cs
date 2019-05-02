using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    [System.Serializable]
    public class ParmFloatNode : INode, IParameterType<float>
    {
        [NodeOutputParm]
        public float Value;

       // [NodeOutputParameter("Value")]
        public float GetValue(ref Context context, int nodeIndex)
        {
            return Value;
        }
    }


    [NodeInfo("Parm/float")]
    public class ParmFloatNodeAsset : NodeAsset<ParmFloatNode>
    {

    }
}

