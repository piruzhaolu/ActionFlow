using UnityEngine;
using System.Collections;
using ActionFlow;


/// <summary>
/// 调试用打印一个文本
/// </summary>
[System.Serializable]
public class BTLog : INode, IBehaviorNode
{
    public string LogText = string.Empty;

    public BehaviorStatus BehaviorInput(ref Context context)
    {
        Debug.Log(LogText);
        return BehaviorStatus.Success;
    }

}


[NodeInfo("BT/Log")]
public class BTLogAsset : NodeAsset<BTLog>
{

}
