﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class InitializeBoardSystem : JobComponentSystem, IBoardSystem
{
    EntityQuery boardQuery_;

    NativeArray<bool> board_;
    NativeList<JobHandle> boardJobs_;
    int2 boardSize_ = new int2(10, 20);

    //[BurstCompile]
    //[RequireComponentTag(typeof(UpdateBoardPosition))]
    [ExcludeComponent(typeof(ActivePiece))]
    struct BoardSystemJob : IJobForEachWithEntity<Piece, Translation>
    {
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<bool> board;

        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;

        public int boardSizeX;

        public void Execute(Entity entity, int index, ref Piece piece, [ReadOnly] ref Translation translation)
        {
            var boardPos = (int3)floor(translation.Value);

            var tiles = tilesLookup[entity];
            //UnityEngine.Debug.Log("Piece " + piece.pieceType + " tile positions at " + boardPos);
            for (int i = 0; i < tiles.Length; ++i)
            {
                float3 localTilePos = tiles[i];
                localTilePos -= piece.pivotOffset;
                int3 tilePos = (int3)math.floor(localTilePos) + boardPos;
                int idx = tilePos.y * boardSizeX + tilePos.x;
                
                if( idx >= 0 && idx < board.Length )
                    board[idx] = true;
                //UnityEngine.Debug.Log("TilePos " + tilePos + ", Idx: " + idx);
            }
        }
    }

    protected override void OnCreate()
    {
        boardQuery_ = GetEntityQuery(typeof(Board), ComponentType.Exclude(typeof(BoardInitialized)));
        board_ = new NativeArray<bool>(10 * 20, Allocator.Persistent);
        boardJobs_ = new NativeList<JobHandle>(5, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        board_.Dispose();
        boardJobs_.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        for (int i = 0; i < board_.Length; ++i)
            board_[i] = false;

        var job = new BoardSystemJob
        {
            board = board_,
            tilesLookup = GetBufferFromEntity<PieceTiles>(true),
            boardSizeX = 10
        }.Schedule(this, inputDependencies);

        boardJobs_.Add(job);

        // Now that the job is set up, schedule it to be run. 
        return job;
    }

    public NativeArray<bool> GetBoard()
    {
        return board_;
    }
    
    public NativeList<JobHandle> GetBoardJobs()
    {
        return boardJobs_;
    }

    public int2 GetBoardSize()
    {
        return boardSize_;
    }
}