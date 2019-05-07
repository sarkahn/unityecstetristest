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
    public float centerOffset_;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Call methods on 'dstManager' to create runtime components on 'entity' here. Remember that:
        //
        // * You can add more than one component to the entity. It's also OK to not add any at all.
        //
        // * If you want to create more than one entity from the data in this class, use the 'conversionSystem'
        //   to do it, instead of adding entities through 'dstManager' directly.
        //
        // For example,
        //   dstManager.AddComponentData(entity, new Unity.Transforms.Scale { Value = scale });

        //dstManager.AddComponentData(entity,
        //    new Piece
        //    {
        //        tilePositions_ = tiles
        //    });
        dstManager.AddComponentData(entity, new Piece { pieceType = pieceType_, centerOffset = centerOffset_ });

        var buffer = dstManager.AddBuffer<PieceTiles>(entity);
        for( int i = 0; i < transform.childCount; ++i )
        {
            var child = transform.GetChild(i);
            int2 pos = ((int3)math.ceil (child.localPosition)).xy;
            buffer.Add(pos);
        }
        //var buffer = dstManager.GetBuffer<Piece>(entity);

    }
}
