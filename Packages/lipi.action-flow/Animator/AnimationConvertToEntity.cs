using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ActionFlow
{
    [RequireComponent(typeof(AnimationPlayer))]
    public class AnimationConvertToEntity : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentObject(entity, GetComponent<AnimationPlayer>());
            dstManager.AddComponent(entity, ComponentType.ReadOnly<CopyTransformToGameObject>());
        }

    }
}

