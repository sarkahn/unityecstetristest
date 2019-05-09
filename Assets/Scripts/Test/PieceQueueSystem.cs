using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;
using ManagedIntList = System.Collections.Generic.List<int>;
using Random = Unity.Mathematics.Random;

[DisableAutoCreation]
public class PieceQueueSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;

    NativeArray<float3> spawnPositions_;
    NativeQueue<Entity> queuedPieces_;
    NativeArray<Entity> piecePrefabs_;
    ManagedIntList shuffleBag_;

    Random rand;

    public const int QueueSize = 6;
    
    [RequireComponentTag(typeof(GetNextPiece))]
    struct PieceQueueSystemJob : IJobForEachWithEntity<Translation, PlayerInput>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent buffer;

        [ReadOnly]
        public NativeArray<float3> spawnPositions;

        public Entity nextPiece;


        public void Execute(Entity entity, int index, ref Translation pos, ref PlayerInput input)
        {
            buffer.RemoveComponent(index, entity, typeof(GetNextPiece));
        }

        
    }

    void FillQueue()
    {
        while (queuedPieces_.Count < QueueSize)
        {
            var pfb = piecePrefabs_[PullFromShuffleBag()];
            var newPiece = EntityManager.Instantiate(pfb);
            queuedPieces_.Enqueue(pfb);

            var queuePos = spawnPositions_[queuedPieces_.Count - 1];
            EntityManager.SetComponentData(newPiece, new Translation { Value = queuePos });

            //EntityManager.SetComponentData(newPiece, )
        }
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
        for (int i = 0; i < piecePrefabs_.Length; ++i)
            shuffleBag_.Add(i);

        for (int i = shuffleBag_.Count - 1; i > 0; --i)
        {
            int j = rand.NextInt(piecePrefabs_.Length);
            int swap = shuffleBag_[j];
            shuffleBag_[j] = shuffleBag_[i];
            shuffleBag_[i] = swap;
        }

        //PrintShuffleBagContents();
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        
        SceneManager.sceneLoaded += OnSceneLoaded;

        shuffleBag_ = new ManagedIntList();
        rand = new Random(1);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var pieceQueue = GameObject.FindObjectOfType<NextPieceQueue>();
        var spawnPositions = pieceQueue.QueuePositions_;
        var prefabs = pieceQueue.PiecePrefabs_;

        queuedPieces_ = new NativeQueue<Entity>(Allocator.Persistent);
        piecePrefabs_ = new NativeArray<Entity>(prefabs.Count, Allocator.Persistent);
        spawnPositions_ = new NativeArray<float3>(spawnPositions.Count, Allocator.Persistent);

        for (int i = 0; i < spawnPositions.Count; ++i)
            spawnPositions_[i] = spawnPositions[i].position;

        for (int i = 0; i < prefabs.Count; ++i)
            piecePrefabs_[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabs[i], World);

        FillQueue();
    }

    protected override void OnDestroy()
    {
        queuedPieces_.Dispose();
        piecePrefabs_.Dispose();
        spawnPositions_.Dispose();
    }


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var nextPiece = queuedPieces_.Dequeue();

        FillQueue();

        var job = new PieceQueueSystemJob
        {
            buffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            spawnPositions = spawnPositions_,
            nextPiece = nextPiece,
        };


        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}