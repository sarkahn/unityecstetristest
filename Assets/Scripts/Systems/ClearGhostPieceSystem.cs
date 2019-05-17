using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//[DisableAutoCreation]
[UpdateBefore(typeof(InitBoardSystem))]
public class ClearGhostPieceSystem : ComponentSystem
{
    EntityQuery ghostPieceQuery_;

    protected override void OnCreate()
    {
        ghostPieceQuery_ = GetEntityQuery(typeof(GhostPiece));
    }

    protected override void OnUpdate()
    {
        EntityManager.DestroyEntity(ghostPieceQuery_);
    }
}