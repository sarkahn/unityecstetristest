using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[InternalBufferCapacity(10*20)]
public struct BoardCell : IBufferElementData
{
    public Entity entity;
    public static implicit operator Entity(BoardCell data) { return data.entity; }
    public static implicit operator BoardCell(Entity val) { return new BoardCell { entity = val }; }
}
