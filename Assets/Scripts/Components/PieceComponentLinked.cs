using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PieceComponentLinked : MonoBehaviour
{
    public PieceType pieceType_;
    public float snapOffset_;

    public bool active_;
    public bool snapToGrid_;
    public bool snapToHeightmap_;

    // Start is called before the first frame update
    void Start()
    {
        var w = World.Active;
        var em = w.EntityManager;
        var e = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObject, w);

        if (active_)
            em.AddComponentData(e, new ActivePiece());
        if (snapToGrid_)
            em.AddComponentData(e, new SnapToGrid());
        if (snapToHeightmap_)
            em.AddComponentData(e, new SnapToHeightmap());

        em.AddComponentData(e, new Piece { pieceType = pieceType_, snapOffset = snapOffset_ });

        Destroy(gameObject);
    }
    
}
