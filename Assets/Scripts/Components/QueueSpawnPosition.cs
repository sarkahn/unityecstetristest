using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct QueueSpawnPosition : IBufferElementData
{
    public float3 data;
    public static implicit operator float3(QueueSpawnPosition component) { return component.data; }
    public static implicit operator QueueSpawnPosition(float3 data) { return new QueueSpawnPosition { data = data }; }
}
