using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PieceQueueData : IComponentData
{
    //public List<int3> positions;
    public float scaling;
}
