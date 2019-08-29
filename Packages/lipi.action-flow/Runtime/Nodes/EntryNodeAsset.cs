using UnityEngine;
using System.Collections;
using System;

namespace ActionFlow
{

    [Serializable]
    [NodeInfo("Entry")]
    public class EntryNode : StatusNodeBase<NullStatus>
    {
        public int _i; //无意义,暂时处理无数据类型序列化出错

        [NodeOutput]
        public override void OnTick(ref Context context)
        {
            context.NodeOutput();
            context.Inactive(this);
        }
    }


}
