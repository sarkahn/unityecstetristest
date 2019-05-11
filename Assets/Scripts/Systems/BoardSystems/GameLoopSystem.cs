using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class GameLoopSystem : JobComponentSystem
{
    EntityQuery activePieceQuery_;
    EntityQuery activeTilesQuery_;

    NativeArray<Entity> board_;

    EndPresentationEntityCommandBufferSystem initBufferSystem_;
    
    protected override void OnCreate()
    {
        board_ = new NativeArray<Entity>(BoardUtility.BoardCellCount, Allocator.Persistent);
        activePieceQuery_ = GetEntityQuery(typeof(ActivePiece));
        initBufferSystem_ = World.GetOrCreateSystem<EndPresentationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        board_.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        for (int i = 0; i < board_.Length; ++i)
            board_[i] = Entity.Null;

        var job = InitBoard(inputDependencies);
        job = Rotation(job);
        job = Movement(job);
        
        return job;
    }

    JobHandle InitBoard(JobHandle dependencies)
    {
    //    // Initialize our board with existing tile data
        var job = new InitializeBoardJob
        {
            board = board_,
            childLookup = GetBufferFromEntity<Child>(true),
            tilesLookup = GetBufferFromEntity<PieceTiles>(true),
        }.Schedule(this, dependencies);
        return job;
    }

    JobHandle Rotation(JobHandle dependencies)
    {
        // Handle rotation
        int rot = InputHandling.GetRotationInput();

        if (rot != 0)
        {
            int activePieces = activePieceQuery_.CalculateLength();

            var job = new PieceRotationJob
            {
                board = board_,
                tilesLookup = GetBufferFromEntity<PieceTiles>(false),
                inputRot = rot,
                posBuffer = new NativeArray<float3>(4 * activePieces, Allocator.TempJob),
            }.Schedule(this, dependencies);
            return job;
        }

        return dependencies;
    }

    JobHandle Movement(JobHandle dependencies)
    {
        // Handle horizontal and vertical piece movement
        float3 vel = float3.zero;

        vel.y = InputHandling.GetFallTimer() <= 0 ? -1 : 0;
        vel.x = InputHandling.GetHorizontalInput();

        if (math.lengthsq(vel) != 0)
        {
            var job = new PieceMovementJob
            {
                board = board_,
                tilesLookup = GetBufferFromEntity<PieceTiles>(true),
                vel = (int3)vel,
                commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, dependencies);

            initBufferSystem_.AddJobHandleForProducer(job);

            return job;
        }

        return dependencies;
    }


}