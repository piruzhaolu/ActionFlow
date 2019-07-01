using UnityEngine;
using System.Collections;
using System;
using Unity.Collections;

namespace ActionFlow
{
    public struct NativeActionData : IDisposable
    {
        // ActionData 的头部信息，描述整块数据的信息
        public struct Info
        {
            public int statesSize;//state的大小
            public int nodeCount; //node数量
            public int chunkCount;//包含的块数量
            public int chunckCapacity; //块数量（占用内存空间的）

        }


        private NativeArray<ActionStateNode> Nodes; //node的状态数据



        public void Dispose()
        {
        }
    }
}
