using UnityEngine;
using System.Collections;
using System;

namespace ActionFlow
{

    [Serializable]
    [NodeInfo("Entry")]
    public class EntryNode : StatusNodeBase<NullStatus>
    {
        [NodeOutput]
        public override void OnTick(ref Context context)
        {
            context.NodeOutput();
            context.Inactive(this);
        }
    }


}
