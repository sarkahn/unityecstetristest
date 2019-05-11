
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
struct LineClearJob : IJob
{
    [WriteOnly]
    [NativeDisableParallelForRestriction]
    public NativeArray<Entity> board;

    public void Execute()
    {
    }
}
