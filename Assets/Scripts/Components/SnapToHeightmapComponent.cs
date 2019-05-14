using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// When added to an entity <seealso cref="OnPieceSpawnedSystem"/> will
/// snap that entity to the heightmap.
/// </summary>
[Serializable]
public struct SnapToHeightmap : IComponentData
{
}

public class SnapToHeightmapComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(SnapToHeightmap));
    }
}
