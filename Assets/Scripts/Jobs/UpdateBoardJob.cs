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

        Debug.LogFormat("New board state at 0,0: {0}", newValues[0].value);

        if( cells[0].value != Entity.Null )
        {
            Debug.LogFormat("State of prev 0,0: {0}. New 0,0: {1}", cells[0], newValues[0]);

        }

        for (int i = 0; i < cells.Length; ++i)
           cells[i] = newValues[i];
    }
}
