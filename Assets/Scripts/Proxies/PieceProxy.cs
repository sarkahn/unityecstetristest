using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
[SelectionBase]
public class PieceProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public PieceType pieceType_;
    public float2 snapOffset_;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Piece { pieceType = pieceType_, snapOffset = snapOffset_ });
        

        var buffer = dstManager.AddBuffer<PieceTiles>(entity);
        for( int i = 0; i < transform.childCount; ++i )
        {
            var child = transform.GetChild(i);
            buffer.Add(new float3(child.localPosition));
        }

        //List<Board> boards = new List<Board>();

        //dstManager.GetAllUniqueSharedComponentData<Board>(boards);


    }
}
