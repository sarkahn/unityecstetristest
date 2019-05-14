using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct HeightMap : IComponentData
{
    public int value;
}
