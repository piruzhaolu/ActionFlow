using UnityEngine;
using System.Collections;
using ActionFlow;


namespace ActionFlow
{
    /// <summary>
    /// 调试用打印一个文本
    /// </summary>
    [System.Serializable]
    [NodeInfo("BT/Log")]
    public class BTLog : INode, IBehaviorNode
    {
        [HideLabelInGraphView]
        public string LogText = string.Empty;

        public BehaviorStatus BehaviorInput(ref Context context)
        {
            Debug.Log(LogText);
            return BehaviorStatus.Success;
        }

    }
}




//public class BTLogAsset : NodeAsset<BTLog>
//{

//}

