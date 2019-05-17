using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct HoldPiecePoint : IComponentData
{
    public Entity heldPiece;
    public Entity lastHeldPiece;
}

public class HoldPiecePointComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(HoldPiecePoint));
    }
}
