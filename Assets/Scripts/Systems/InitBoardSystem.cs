using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

//[DisableAutoCreation]
public class InitBoardSystem : JobComponentSystem
{
    EntityQuery tilesQuery_;
    EntityQuery boardQuery_;

    [BurstCompile]
    struct UpdateBoardJob : IJobChunk
    {
        public ArchetypeChunkComponentType<BoardCell> cellsType;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> tiles;

        [ReadOnly]
        public ComponentDataFromEntity<LocalToWorld> ltwFromEntity;
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var cells = chunk.GetNativeArray(cellsType);

            // Clear the board
            for (int i = 0; i < cells.Length; ++i)
                cells[i] = Entity.Null;
            
            for( int i = 0; i < tiles.Length; ++i )
            {
                var tile = tiles[i];
                float3 tilePos = ltwFromEntity[tile].Position;
                int3 cell = (int3)math.floor(tilePos);
                int idx = cell.y * BoardUtility.BoardSize.x + cell.x;
                if (idx > 0 && idx < cells.Length)
                    cells[idx] = tile;
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
            ltwFromEntity = GetComponentDataFromEntity<LocalToWorld>(true),
            cellsType = GetArchetypeChunkComponentType<BoardCell>(false),
        }.Schedule(boardQuery_, JobHandle.CombineDependencies(job, getEntitiesJob));

        return job;
    }
}