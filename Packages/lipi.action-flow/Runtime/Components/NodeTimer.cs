using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace ActionFlow
{
    public struct NodeTimer : IBufferElementData
    {
        public float Time;
        public int NodeIndex;
    }

}
