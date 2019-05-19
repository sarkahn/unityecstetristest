using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class DropPieceSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if( InputHandling.InstantDrop() )
        {
            Entities.WithAll<Piece>().WithAll<ActivePiece>().ForEach(
                (Entity e)=>
                {
                    PostUpdateCommands.AddComponent<SnapToHeightmap>(e, new SnapToHeightmap());
                });
        }
    }
}