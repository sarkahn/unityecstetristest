using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct ActivePiece : IComponentData
{
}

public class ActivePieceComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(ActivePiece));

        var t = transform;
        for( int i = 0; i < t.childCount; ++i )
        {
            var childEntity = conversionSystem.GetPrimaryEntity(t.GetChild(i));
            dstManager.AddComponent(childEntity, typeof(ActiveTile));
        }
    }
}
