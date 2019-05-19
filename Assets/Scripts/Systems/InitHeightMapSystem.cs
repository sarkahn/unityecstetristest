using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class InitHeightMapSystem : JobComponentSystem
{
    EntityQuery heightMapQuery_;
    
    //[BurstCompile]
    [RequireComponentTag(typeof(Child))]
    [ExcludeComponent(typeof(ActivePiece), typeof(SnapToHeightmap), typeof(GhostPiece))]
    struct GetHeightmapDataJob : IJobForEachWithEntity<Piece>
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<int> heightMap;

        [ReadOnly]
        public ComponentDataFromEntity<Translation> posFromEntity;
        [ReadOnly]
        public BufferFromEntity<Child> tilesFromEntity;

        public void Execute(Entity entity, int index, ref Piece c0)
        {
            float3 piecePos = posFromEntity[entity].Value;

            var tiles = tilesFromEntity[entity];
            for( int i = 0; i < tiles.Length; ++i )
            {

                if (!posFromEntity.Exists(tiles[i].Value))
                {
                    //Debug.LogFormat("Tile {0} in pice {1} has no translation", tiles[i].Value, entity);
                    //throw new System.Exception("Tile without translation...how is this possible?");
                    continue;
                }

                float3 tilePos = posFromEntity[tiles[i].Value].Value;


                int3 cell = BoardUtility.CellFromWorldPos(piecePos + tilePos);

                if (cell.x < 0 || cell.x >= heightMap.Length)
                    return;

                heightMap[cell.x] = math.max(heightMap[cell.x], cell.y + 1);
            }
        }
    }

    [BurstCompile]
    struct WriteHeightMapDataJob : IJobChunk
    {
        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<int> heightMap;

        public ArchetypeChunkComponentType<HeightmapCell> cellType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            //Debug.LogFormat("Writing heightmap, From cell 3: {0}, {1}, {2}, {3}",
            //    heightMap[3], heightMap[4], heightMap[5], heightMap[6]);
            var cells = chunk.GetNativeArray(cellType);
            for (int i = 0; i < cells.Length; ++i)
                cells[i] = heightMap[i];
        }
    }

    protected override void OnCreate()
    {
        heightMapQuery_ = GetEntityQuery(typeof(HeightmapCell));
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = inputDependencies;

        NativeArray<int> heightMap = new NativeArray<int>(10, Allocator.TempJob);

        job = new GetHeightmapDataJob
        {
            heightMap = heightMap,
            posFromEntity = GetComponentDataFromEntity<Translation>(true),
            tilesFromEntity = GetBufferFromEntity<Child>(true),
        }.ScheduleSingle(this, job);


        job = new WriteHeightMapDataJob
        {
            cellType = GetArchetypeChunkComponentType<HeightmapCell>(false),
            heightMap = heightMap,
        }.Schedule(heightMapQuery_, job);
        

        return job;
    }
}