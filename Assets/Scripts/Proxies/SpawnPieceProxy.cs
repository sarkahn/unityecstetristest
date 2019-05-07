using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class SpawnPieceProxy : MonoBehaviour, IConvertGameObjectToEntity
{

    public enum PieceType
    {
        IPiece = 0,
        JPiece = 1,
        LPiece = 2,
        OPiece = 3,
        SPiece = 4,
        TPiece = 5,
        ZPiece = 6
    }

    public PieceType pieceType_;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SpawnPiece { pieceType = (int)pieceType_ });
    }
}
