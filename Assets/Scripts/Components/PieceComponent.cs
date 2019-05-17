using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public enum PieceType
{
    IPiece = 0,
    JPiece = 1,
    LPiece = 2,
    OPiece = 3,
    SPiece = 4,
    TPiece = 5,
    ZPiece = 6,
};

[Serializable]
public struct Piece : IComponentData
{
    public PieceType pieceType;
    public float snapOffset;
}

[SelectionBase]
public class PieceComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public PieceType pieceType_;
    public float spawnOffset_ = 0;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

        //dstManager.AddComponentData(entity, new Piece { pieceType = pieceType_, snapOffset = spawnOffset_ });
        //var t = transform;
        
        //for( int i = 0; i < t.childCount; ++i )
        //{
        //    var e = conversionSystem.GetPrimaryEntity(t.GetChild(i).gameObject);
        //    dstManager.AddComponent(e, typeof(PieceTile));
            
        //}
        
    }
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        float3 p = transform.position;
        p = math.floor(p);
        p.z = -1;
        Gizmos.DrawWireSphere(p, .5f);
    }
}
