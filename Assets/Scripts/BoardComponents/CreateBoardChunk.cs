using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Creates a single chunk of <seealso cref="BoardCell"/> we will use
/// to represent our game board. We can access/modify the board by doing 
/// chunk iteration on our board chunk.
/// </summary>
public class CreateBoardChunk : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var arch = dstManager.CreateArchetype(typeof(BoardCell));

        NativeArray<ArchetypeChunk> chunks = new NativeArray<ArchetypeChunk>(1, Allocator.Temp);

        dstManager.CreateChunk(arch, chunks, BoardUtility.BoardCellCount);

        dstManager.LockChunk(chunks);

        chunks.Dispose();
    }
}
