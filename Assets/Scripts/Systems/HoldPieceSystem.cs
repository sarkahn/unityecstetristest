using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class HoldPieceSystem : JobComponentSystem
{
    EntityQuery holdPiecePointQuery_;
    EntityQuery activePieceQuery_;

    BeginInitializationEntityCommandBufferSystem initBufferSystem_;
    
    struct HoldPieceJob : IJob
    {
        [ReadOnly]
        public EntityCommandBuffer commandBuffer;
        
        [ReadOnly]
        public ComponentDataFromEntity<Translation> posFromEntity;
        [ReadOnly]
        public ComponentDataFromEntity<NonUniformScale> scaleFromEntity;

        public ComponentDataFromEntity<HoldPiecePoint> holdPieceFromEntity;

        public Entity pieceToHold;
        public Entity pieceHolder;

        public float3 spawnPoint;

        public void Execute()
        {
            var holdPiece = holdPieceFromEntity[pieceHolder];
            
            if (pieceToHold == holdPiece.lastHeldPiece)
                return;

            if (holdPiece.heldPiece == Entity.Null)
            {
                commandBuffer.AddComponent<SpawnNextPiece>(pieceHolder, new SpawnNextPiece());
            }
            else
            {
                var heldPiece = holdPiece.heldPiece;
                commandBuffer.RemoveComponent(heldPiece, typeof(NonUniformScale));
                commandBuffer.AddComponent<ActivePiece>(heldPiece, new ActivePiece());
                commandBuffer.AddComponent<SnapToGrid>(heldPiece, new SnapToGrid());
                commandBuffer.SetComponent(heldPiece, new Translation { Value = spawnPoint });
            }

            holdPieceFromEntity[pieceHolder] = new HoldPiecePoint {
                lastHeldPiece = holdPiece.heldPiece,
                heldPiece = pieceToHold, };

            var holdPiecePos = posFromEntity[pieceHolder];
            var holdPieceScale = scaleFromEntity[pieceHolder];

            commandBuffer.AddComponent(pieceToHold, holdPieceScale);
            commandBuffer.SetComponent(pieceToHold, holdPiecePos);
            
            commandBuffer.RemoveComponent<ActivePiece>(pieceToHold);
        }
    }
    

    protected override void OnCreate()
    {
        holdPiecePointQuery_ = GetEntityQuery(typeof(HoldPiecePoint));
        activePieceQuery_ = GetEntityQuery(typeof(ActivePiece));
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = inputDeps;

        if( Input.GetButtonDown("HoldPiece") )
        {
            var holdPieceEntity = GetSingletonEntity<HoldPiecePoint>();

            var activePieces = activePieceQuery_.ToEntityArray(Allocator.TempJob);
            if (activePieces.Length != 0)
            {
                // How to handle multiple active pieces?
                var pieceToHold = activePieces[UnityEngine.Random.Range(0, activePieces.Length)];

                Entity spawnPointEntity = GetSingletonEntity<PieceSpawnPoint>();
                float3 spawnPointPos = EntityManager.GetComponentData<Translation>(spawnPointEntity).Value;

                job = new HoldPieceJob
                {
                    commandBuffer = initBufferSystem_.CreateCommandBuffer(),
                    pieceHolder = holdPieceEntity,
                    pieceToHold = pieceToHold,
                    holdPieceFromEntity = GetComponentDataFromEntity<HoldPiecePoint>(false),
                    posFromEntity = GetComponentDataFromEntity<Translation>(true),
                    scaleFromEntity = GetComponentDataFromEntity<NonUniformScale>(true),
                    spawnPoint = spawnPointPos,
                }.Schedule(job);

                initBufferSystem_.AddJobHandleForProducer(job);

            }
            activePieces.Dispose();



        }


        return job;
    }
}