using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum PieceType
{
    IPiece = 0,
    JPiece = 1,
    LPiece = 2,
    OPiece = 3,
    SPiece = 4,
    TPiece = 5,
    ZPiece = 6,
}

[Serializable]
public struct Piece : IComponentData
{
    //public int3 boardPos;
    public PieceType pieceType;
    public float pivotOffset;
}
