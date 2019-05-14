using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public class BoardSystem : JobComponentSystem
{
    EntityQuery activePieceQuery_;

    //NativeArray<Entity> board_;
    //NativeArray<int> heightMap_;
    //public NativeArray<int> HeightMap_ => heightMap_;

    EndPresentationEntityCommandBufferSystem initBufferSystem_;
    
    protected override void OnCreate()
    {
        //board_ = new NativeArray<Entity>(BoardUtility.BoardCellCount, Allocator.Persistent);
        //heightMap_ = new NativeArray<int>(BoardUtility.BoardSize.x, Allocator.Persistent);

        activePieceQuery_ = GetEntityQuery(typeof(ActivePiece));
        initBufferSystem_ = World.GetOrCreateSystem<EndPresentationEntityCommandBufferSystem>();

        RequireForUpdate(activePieceQuery_);
    }

    protected override void OnDestroy()
    {
        //board_.Dispose();
        //heightMap_.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Clear the board
        //for (int i = 0; i < board_.Length; ++i)
        //    board_[i] = Entity.Null;
        //for (int i = 0; i < heightMap_.Length; ++i)
        //    heightMap_[i] = 0;

        var boardJob = inputDeps;
        

        //// Initialize piece tiles
        //boardJob = new InitializePieceTilesJob
        //{
        //    childLookup = GetBufferFromEntity<Child>(true),
        //    tilesLookup = GetBufferFromEntity<PieceTiles>(false),
        //    translationLookup = GetComponentDataFromEntity<Translation>(false),
        //}.Schedule(this, boardJob);


        //// Initialize the board with previously placed pieces
        //boardJob = new InitializeBoardJob
        //{
        //    board = board_,
        //    childLookup = GetBufferFromEntity<Child>(true),
        //    tilesLookup = GetBufferFromEntity<PieceTiles>(true),
        //}.Schedule(this, boardJob);


        //// Build the height map - Note this doesn't use the board so it has no dependencies in this system
        //boardJob = new BuildHeightmapJob
        //{
        //    heightMap = heightMap_,
        //    tilesLookup = GetBufferFromEntity<PieceTiles>(true),
        //}.ScheduleSingle(this, boardJob);


        //// Handle rotation
        //int rot = InputHandling.GetRotationInput();

        //if (rot != 0)
        //{
        //    int activePieces = activePieceQuery_.CalculateLength();

        //    boardJob = new PieceRotationJob
        //    {
        //        board = board_,
        //        tilesLookup = GetBufferFromEntity<PieceTiles>(false),
        //        inputRot = rot,
        //        posBuffer = new NativeArray<float3>(4 * activePieces, Allocator.TempJob),
        //    }.Schedule(this, boardJob);
        //    return boardJob;
        //}


        //// Handle horizontal and vertical piece movement
        //float3 vel = float3.zero;

        //vel.y = InputHandling.GetFallTimer() <= 0 ? -1 : 0;
        //vel.x = InputHandling.GetHorizontalInput();

        //if (math.lengthsq(vel) != 0)
        //{
        //    boardJob = new PieceMovementJob
        //    {
        //        board = board_,
        //        tilesLookup = GetBufferFromEntity<PieceTiles>(true),
        //        vel = (int3)vel,
        //        commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
        //    }.Schedule(this, boardJob);

        //    initBufferSystem_.AddJobHandleForProducer(boardJob);
        //}


        ////JobHandle boardJob = inputDeps;

        //// Drop job
        //if ( InputHandling.InstantDrop() )
        //{
        //    boardJob = new PieceDropJob
        //    {
        //        board = board_,
        //        heightMap = heightMap_,
        //        commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
        //        tilesLookup = GetBufferFromEntity<PieceTiles>(true),
        //    }.Schedule(this, boardJob);

        //    initBufferSystem_.AddJobHandleForProducer(boardJob);
        //}

        //// Build the height map - Note this doesn't use the board so it has no dependencies in this system
        //boardJob = new BuildHeightmapJob
        //{
        //    heightMap = heightMap_,
        //    tilesLookup = GetBufferFromEntity<PieceTiles>(true),
        //}.ScheduleSingle(this, boardJob);

        //boardJob = new LineClearJob
        //{
        //    board = board_
        //}.Schedule(boardJob);

        return boardJob;
    }
    




}