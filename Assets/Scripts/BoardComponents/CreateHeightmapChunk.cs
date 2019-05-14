using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public class CreateHeightmapChunk : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var arch = dstManager.CreateArchetype(typeof(HeightmapCell));
        var chunks = new NativeArray<ArchetypeChunk>(1, Allocator.Temp);

        dstManager.CreateChunk(arch, chunks, BoardUtility.BoardSize.x);

        dstManager.LockChunk(chunks);

        chunks.Dispose();
    }
}
