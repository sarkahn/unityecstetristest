using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

// Used to track when a piece no longer becomes active
[Serializable]
public struct ActivePieceState : ISystemStateComponentData
{
}
