using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class InitBoardSystem : JobComponentSystem
{
    EntityQuery tilesQuery_;
    EntityQuery boardQuery_;

    //[BurstCompile]
    struct UpdateBoardJob : IJobChunk
    {
        public ArchetypeChunkComponentType<BoardCell> cellsType;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> tiles;

        [ReadOnly]
        public ComponentDataFromEntity<Parent> parentFromEntity;

        [ReadOnly]
        public ComponentDataFromEntity<Translation> posFromEntity;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var cells = chunk.GetNativeArray(cellsType);

            //Debug.Log("INITIALIZING BOARD");

            // Clear the board
            for (int i = 0; i < cells.Length; ++i)
                cells[i] = new BoardCell { value = Entity.Null };
            
            for( int i = 0; i < tiles.Length; ++i )
            {
                var tile = tiles[i];
                var parent = parentFromEntity[tile].Value;
                //float3 tilePos = posFromEntity[tile].Value;
                //float3 piecePos = posFromEntity[]

                // Note: I originally used the localToWorld component from the tile
                // to get the world positions but it failed to take into account the translation 
                // change in SnapToGridSystem, even if I tried running SnapToGrid before this system or 
                // even before the Transform systems.
                float3 tilePos = posFromEntity[tile].Value;
                float3 piecePos = posFromEntity[parent].Value;
                float3 worldPos = tilePos + piecePos;


                int3 cell = BoardUtility.CellFromWorldPos(worldPos);
                int idx = BoardUtility.IndexFromCellPos(cell);

                //Debug.LogFormat("Parent {0}, LTWPos {1}, TilePos {2}, PiecePos {3}, Tile+Piece {4}", parent, ltwPos, tilePos, piecePos, tilePos + piecePos);

                //Debug.LogFormat("Setting board pos {0} to {1} from Piece {2}", cell, tile, parent);
                if ( BoardUtility.InBounds(cell) && BoardUtility.IndexInBounds(idx))
                    cells[idx] = new BoardCell { value = tile };
            }
        }
    }

    struct UpdateBoardJobLTW : IJobChunk
    {
        [ReadOnly]
        [DeallocateOnJobCompletion]
        // These are children of the pieces whose translation I updated in my other job
        public NativeArray<Entity> tiles;

        [ReadOnly]
        public ComponentDataFromEntity<LocalToWorld> ltwFromEntity;

        public ArchetypeChunkComponentType<BoardCell> cellsType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var cells = chunk.GetNativeArray(cellsType);
            for( int i = 0; i < tiles.Length; ++i )
            {
                var tile = tiles[i];

                // Gives the position from the previous frame before I updated the parent's
                // translation
                var ltw = ltwFromEntity[tile];

                int3 cell = (int3)math.floor(ltw.Position);
                int idx = cell.y * 10 + cell.x;
                cells[idx] =  new BoardCell { value = tile };
            }
        }
    }

    protected override void OnCreate()
    {
        boardQuery_ = GetEntityQuery(typeof(BoardCell));
        tilesQuery_ = GetEntityQuery(typeof(PieceTile), typeof(LocalToWorld));
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = inputDependencies;

        JobHandle getEntitiesJob;
        var tiles = tilesQuery_.ToEntityArray(Allocator.TempJob, out getEntitiesJob);

        job = new UpdateBoardJob
        {
            tiles = tiles,
            cellsType = GetArchetypeChunkComponentType<BoardCell>(false),
            parentFromEntity = GetComponentDataFromEntity<Parent>(true),
            posFromEntity = GetComponentDataFromEntity<Translation>(true),
        }.Schedule(boardQuery_, JobHandle.CombineDependencies(job, getEntitiesJob));

        return job;
    }
}