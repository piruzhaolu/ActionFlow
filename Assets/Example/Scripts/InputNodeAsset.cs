using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionFlow;


[System.Serializable]
public class InputNode :INode, INodeInput
{
    public string RoleName;
    public int Age;

    public void OnInput(ref Context context)
    {
    }

    [NodeOutput] //(typeof(int))
    public void TT()
    {

    }


}

[NodeInfo("Example/Input")]
public class InputNodeAsset : NodeAsset<InputNode>
{
}