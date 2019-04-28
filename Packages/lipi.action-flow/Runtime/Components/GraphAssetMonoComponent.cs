using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace ActionFlow
{
    /// <summary>
    /// 组件转换类
    /// </summary>
    public class GraphAssetMonoComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GraphAsset Asset;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddSharedComponentData(entity, new ActionGraphAsset()
            {
                Asset = Asset
            });
        }
    }

}
