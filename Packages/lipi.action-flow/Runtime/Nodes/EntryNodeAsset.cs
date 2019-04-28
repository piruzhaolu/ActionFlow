using UnityEngine;
using System.Collections;
using System;

namespace ActionFlow
{

    [Serializable]
    public class EntryNode : StatusNodeBase<NullStatus>
    {
        [NodeOutput]
        public override void OnTick(ref Context context)
        {
            context.NodeOutput();
            context.Inactive();
        }
    }



    [NodeInfo("Entry")]
    public class EntryNodeAsset : NodeAsset<EntryNode>
    {

    }


}
