using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[InternalBufferCapacity(10*20)]
public struct BoardCell : IBufferElementData
{
    public bool occupied;
    public static implicit operator bool(BoardCell data) { return data.occupied; }
    public static implicit operator BoardCell(bool val) { return new BoardCell { occupied = val }; }
}
