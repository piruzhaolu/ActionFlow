using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionFlow;


[NodeInfo("输入测试")]
public class InputNode : ScriptableObject, INode, INodeInput
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
