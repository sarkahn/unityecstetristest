using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PieceQueueDataProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<Transform> piecePositions_;
    public float scaling_ = 1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(PieceQueue));
        var buffer = dstManager.AddBuffer<QueueSpawnPosition>(entity);
        for (int i = 0; i < transform.childCount; ++i)
            buffer.Add((float3)transform.GetChild(i).position);

        //conversionSystem.SetSingleton(new PieceQueueData { scaling = scaling_ });

        //List<int3> positions = new List<int3>();
        //foreach (var t in piecePositions_)
        //    positions.Add(new int3(t.position));

        //dstManager.AddSharedComponentData(entity, new PieceQueueData { positions = positions, scaling = scaling_ });

        // From Docs:
        //To create a singleton, create an entity with the singleton component as its only component, and then 
        // use SetSingleton() to assign a value.

        //dstManager.AddComponent(entity, typeof(PieceQueueData));
        //dstManager.CreateEntityQuery(typeof(PieceQueueData)).SetSingleton(new PieceQueueData { positions = positions, scaling = scaling_ });
    }
}
