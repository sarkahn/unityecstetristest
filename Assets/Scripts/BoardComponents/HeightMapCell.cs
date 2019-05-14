using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


/// <summary>
/// A component representing our heightmap.
/// </summary>
[Serializable]
public struct HeightmapCell : IComponentData
{
    public int value;
    public static implicit operator int(HeightmapCell cell) { return cell.value; }
    public static implicit operator HeightmapCell(int i) { return new HeightmapCell { value = i }; }
}
