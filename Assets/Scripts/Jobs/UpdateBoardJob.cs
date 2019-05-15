using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public struct UpdateBoardJob : IJobChunk
{
    public ArchetypeChunkComponentType<BoardCell> cellType;

    [DeallocateOnJobCompletion]
    public NativeArray<BoardCell> newValues;

    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
    {
        var cells = chunk.GetNativeArray<BoardCell>(cellType);
        for (int i = 0; i < cells.Length; ++i)
           cells[i] = newValues[i];
    }
}
