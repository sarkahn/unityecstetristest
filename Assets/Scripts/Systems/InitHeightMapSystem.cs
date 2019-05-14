using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(SnapToGridSystem))]
public class InitHeightMapSystem : JobComponentSystem
{
    EntityQuery heightMapQuery_;

    [RequireComponentTag(typeof(PieceTile))]
    [ExcludeComponent(typeof(ActiveTile))]
    [BurstCompile]
    struct GetHeightmapDataJob : IJobForEach<LocalToWorld>
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<int> heightMap;

        public void Execute([ReadOnly] ref LocalToWorld t)
        {
            int3 cell = (int3)math.floor(t.Position);
            heightMap[cell.x] = math.max(heightMap[cell.x], cell.y + 1);
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
        }.ScheduleSingle(this, job);


        job = new WriteHeightMapDataJob
        {
            cellType = GetArchetypeChunkComponentType<HeightmapCell>(false),
            heightMap = heightMap,
        }.Schedule(heightMapQuery_, job);

        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}