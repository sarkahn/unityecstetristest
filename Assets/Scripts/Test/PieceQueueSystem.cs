using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using ManagedList = System.Collections.Generic.List<int>;

[DisableAutoCreation]
public class PieceQueueSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;
    NativeArray<int3> queuePositions_;
    NativeQueue<Entity> pieceQueue_;
    NativeArray<Entity> piecePrefabs_;
    ManagedList shuffleBag_;

    EntityQuery sceneDataQuery_;

    Random rand;

    int shuffleBagSize_ = 7;
    int queueSize_ = 6;

    [BurstCompile]
    [RequireComponentTag(typeof(GetNextPiece))]
    struct PieceQueueSystemJob : IJobForEachWithEntity<Translation, PlayerInput>
    {
        public EntityCommandBuffer buffer;
        public NativeQueue<Entity> pieceQueue;

        public void Execute(Entity entity, int index, ref Translation pos, ref PlayerInput input)
        {
            buffer.RemoveComponent<GetNextPiece>(entity);
            var nextPiece = pieceQueue.Dequeue();
            buffer.SetComponent(nextPiece, pos);
        }

        
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

        pieceQueue_ = new NativeQueue<Entity>(Allocator.Persistent);
        shuffleBag_ = new ManagedList(shuffleBagSize_);

        rand = new Random(1);
        sceneDataQuery_ = GetEntityQuery(ComponentType.ReadWrite<PieceQueueData>());
        EntityManager.GetComponentObject<PieceQueueData>(sceneDataQuery_.GetSingletonEntity());
    }

    int PullFromShuffleBag()
    {
        if (shuffleBag_.Count == 0)
            FillShuffleBag();

        int i = shuffleBag_[shuffleBag_.Count - 1];
        shuffleBag_.RemoveAt(shuffleBag_.Count - 1);
        
        return i;
    }

    void FillShuffleBag()
    {
        for (int i = 0; i < shuffleBagSize_; ++i)
            shuffleBag_.Add(i);

        for( int i = shuffleBag_.Count - 1; i > 0; --i )
        {
            int j = rand.NextInt(i);
            shuffleBag_[j] = shuffleBag_[i];
        }
    }
    

    protected override void OnDestroy()
    {
        piecePrefabs_.Dispose();
        pieceQueue_.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        List<PieceQueueData> sharedComponents = new List<PieceQueueData>(3);

        var data = EntityManager.CreateEntityQuery(typeof(PieceQueueData)).GetSingleton<PieceQueueData>();

        UnityEngine.Debug.Log("Scaling: " + data.scaling);

        //for( int i = 0; i < data.positions.Count; ++i )
        //{
        //    UnityEngine.Debug.LogFormat("Position {0}: {1}", i, data.positions[i]);
        //}


        while(pieceQueue_.Count < queueSize_ )
        {
            var prefab = piecePrefabs_[PullFromShuffleBag()];
            var piece = EntityManager.Instantiate(prefab);
            pieceQueue_.Enqueue(piece);
        }
        

        var job = new PieceQueueSystemJob
        {
            buffer = initBufferSystem_.CreateCommandBuffer(),
            pieceQueue = pieceQueue_,
        };

    
        
        
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}