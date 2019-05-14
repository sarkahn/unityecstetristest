using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Heightmap : IComponentData
{
}

public class HeightmapComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //dstManager.AddComponent(entity, typeof(Heightmap));
        //var buffer = dstManager.AddBuffer<HeightmapCell>(entity);
        //for (int i = 0; i < 10; ++i)
        //    buffer.Add(0);
    }
}
