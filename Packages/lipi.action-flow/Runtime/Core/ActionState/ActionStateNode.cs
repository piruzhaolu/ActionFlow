using UnityEngine;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ActionFlow
{

    public enum NodeCycle
    {
        Inactive    = 0,
        Sleeping    = 0b001,
        Active      = 0b010,
        Waking      = 0b100
    }

    public static class NodeCycleExt
    {
        public static bool Has(this NodeCycle self, NodeCycle cycle)
        {
            return (self & cycle) == cycle;
        }
        public static NodeCycle Add(this NodeCycle self, NodeCycle cycle)
        {
            return self | cycle;
        }

        public static NodeCycle Remove(this NodeCycle self, NodeCycle cycle)
        {
            return self & ~cycle;
        }

    }


    public struct ActionStateNode
    {
        public NodeCycle Cycle;
        public int offset;


        public static readonly int NodeSize = UnsafeUtility.SizeOf<ActionStateNode>();

    }



}
