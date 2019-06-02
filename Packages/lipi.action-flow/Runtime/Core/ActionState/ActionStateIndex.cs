using UnityEngine;
using System.Collections;
using System;

namespace ActionFlow
{
    public struct ActionStateIndex:IEquatable<ActionStateIndex>
    {
        public int ChunkIndex;
        public int NodeIndex;

        public ActionStateIndex(int chunkIndex, int nodeIndex)
        {
            ChunkIndex = chunkIndex;
            NodeIndex = nodeIndex;
        }

        public ActionStateIndex NewStateIndex(int nodeIndex)
        {
            var index = this;
            index.NodeIndex = nodeIndex;
            return index;
        }

        public override int GetHashCode()
        {
            return new Vector2Int(ChunkIndex, NodeIndex).GetHashCode();   
            //return base.GetHashCode();
        }

        public bool Equals(ActionStateIndex other)
        {
            return other.ChunkIndex == ChunkIndex && other.NodeIndex == NodeIndex;
        }
    }

}
