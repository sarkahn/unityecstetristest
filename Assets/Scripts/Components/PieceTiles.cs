using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
[InternalBufferCapacity(4)]
public struct PieceTiles : IBufferElementData
{
    public float3 tilePos;
    public static implicit operator float3(PieceTiles data) { return data.tilePos; }
    public static implicit operator PieceTiles(float3 tile) { return new PieceTiles { tilePos = tile }; }
}
