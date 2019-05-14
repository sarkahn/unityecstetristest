using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct HeightMapCell : IBufferElementData
{
    public int value;
    public static implicit operator int(HeightMapCell h) { return h.value; }
    public static implicit operator HeightMapCell(int i) { return new HeightMapCell { value = i };  }
}
