
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//TODO: Make this a reactive system?
[DisableAutoCreation]
public class PieceSpawnSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;

    Random rand;

    NativeArray<Entity> piecePrefabs_;
    
    public struct SpawnJob : IJobForEachWithEntity<SpawnPiece, Translation>
    {
        [ReadOnly]
        public EntityCommandBuffer commandBuffer;
        [ReadOnly]
        public NativeArray<Entity> prefabs;

        public int3 randPos;

        public void Execute([ReadOnly] Entity entity, int index, ref SpawnPiece spawnPiece, ref Translation pos )
        {

            var prefab = prefabs[spawnPiece.pieceType];

            var newPiece = commandBuffer.Instantiate(prefab);
            
            commandBuffer.SetComponent(newPiece, pos);

            commandBuffer.DestroyEntity(entity);
            
        }
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        rand = new Random((uint)UnityEngine.Random.Range(1,100));


        var pieces = UnityEngine.Resources.LoadAll<UnityEngine.GameObject>("Prefabs/Pieces");

        if (pieces != null && pieces.Length != 0)
        {
            piecePrefabs_ = new NativeArray<Entity>(pieces.Length, Allocator.Persistent);

            for (int i = 0; i < pieces.Length; ++i)
                piecePrefabs_[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(pieces[i], World);
            
        }
        else
            UnityEngine.Debug.LogError("Error loading pieces");
    }

    protected override void OnDestroy()
    {
        piecePrefabs_.Dispose();
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpawnJob
        {
            commandBuffer = initBufferSystem_.CreateCommandBuffer(),
            randPos = rand.NextInt3(10),
            prefabs = piecePrefabs_,
        };

        var handle = job.Schedule(this, inputDeps);

        initBufferSystem_.AddJobHandleForProducer(handle);

        return handle;
    }

}
