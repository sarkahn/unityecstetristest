using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PlayerInput : IComponentData
{
    public int rotation;
    public int movement;
    public bool drop;
}
