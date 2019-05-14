using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ApplyRotation : IComponentData
{
    public float angle;
}

public class ApplyRotationComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public float angle_;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ApplyRotation { angle = angle_ });
    }
}
