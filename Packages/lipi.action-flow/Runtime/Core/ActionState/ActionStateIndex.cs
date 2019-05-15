using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    public struct ActionStateIndex
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
    }

}
