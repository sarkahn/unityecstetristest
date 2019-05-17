using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public class GhostPieceSystemProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [SerializeField]
    List<GameObject> goPrefabs_;

    NativeArray<Entity> entityPrefabs_;
    public NativeArray<Entity> EntityPrefabs_ => entityPrefabs_;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        entityPrefabs_ = new NativeArray<Entity>(goPrefabs_.Count, Allocator.Persistent);
        for (int i = 0; i < goPrefabs_.Count; i++)
        {
            entityPrefabs_[i] = conversionSystem.GetPrimaryEntity(goPrefabs_[i]);
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(goPrefabs_);
    }
}
