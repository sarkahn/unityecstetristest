using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
[InternalBufferCapacity(4)]
public struct PieceTiles : IBufferElementData
{
    public int2 tile;
    public static implicit operator int2(PieceTiles data) { return data.tile; }
    public static implicit operator PieceTiles(int2 tile) { return new PieceTiles { tile = tile }; }
}
