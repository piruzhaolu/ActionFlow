using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ActionFlow
{

    public struct ActionGraphAsset:ISharedComponentData
    {
        public GraphAsset Asset;
    }


    /// <summary>
    /// ActionGraphAsset 已创建标记
    /// </summary>
    public struct ActionGraphCreated : ISystemStateComponentData
    {
    }




}
