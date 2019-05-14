using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Converts prefab and positional data (set up via the inspector) used for piece spawning so they can be passed to
/// the spawner system after the scene has loaded: <seealso cref="SpawnNextPieceSystem.OnCreate"/>
/// </summary>
public class SpawnSystemConverter : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [SerializeField]
    List<GameObject> prefabs_;

    [SerializeField]
    List<Transform> queuePositions_;

    [SerializeField]
    Transform spawnPiecePosition_;

    public NativeArray<Entity> entityPrefabs_;
    public NativeArray<float3> entityQueuePositions_;

    public Entity entitySpawnPosition_;
    


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        entityPrefabs_ = new NativeArray<Entity>(prefabs_.Count, Allocator.Persistent);
        entityQueuePositions_ = new NativeArray<float3>(queuePositions_.Count, Allocator.Persistent);

        entitySpawnPosition_ = conversionSystem.GetPrimaryEntity(spawnPiecePosition_.gameObject);

        for( int i = 0; i < prefabs_.Count; ++i )
            entityPrefabs_[i] = conversionSystem.GetPrimaryEntity(prefabs_[i]);

        for (int i = 0; i < queuePositions_.Count; ++i)
            entityQueuePositions_[i] = queuePositions_[i].position;
        

    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(prefabs_);
        referencedPrefabs.Add(spawnPiecePosition_.gameObject);
    }
}
