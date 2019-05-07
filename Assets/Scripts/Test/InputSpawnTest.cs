using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class InputSpawnTest : JobComponentSystem
{

    BeginInitializationEntityCommandBufferSystem initBufferSystem_;
    Random rand;

    //[BurstCompile]
    [RequireComponentTag(typeof(PieceSpawner))]
    [ExcludeComponent(typeof(SpawnPiece))]
    struct InputSpawnTestJob : IJobForEachWithEntity<PlayerInput>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;
        [ReadOnly]
        public EntityCommandBuffer buffer;

        public float3 mousePos;
        public int randInt;
        public bool mouseClicked;

        public void Execute(Entity entity, int index, ref PlayerInput input )
        {
            if( input.drop )
            {
                buffer.AddComponent(entity, new SpawnPiece { pieceType = randInt });
            }

            if( mouseClicked )
            {
                var spawnEntity = buffer.CreateEntity();
                buffer.AddComponent(spawnEntity, new SpawnPiece { pieceType = randInt });
                buffer.AddComponent(spawnEntity, new Translation { Value = mousePos });
            }
        }
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        rand = new Random((uint)UnityEngine.Random.Range(1, 500));
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var mousePos = UnityEngine.Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        mousePos.z = -1;
        bool mouseClicked = UnityEngine.Input.GetMouseButtonDown(0);

        if( mouseClicked )
        {
            //UnityEngine.Debug.Log("Input spawn at mousePos " + mousePos);
        }

        var job = new InputSpawnTestJob
        {
            buffer = initBufferSystem_.CreateCommandBuffer(),
            randInt = rand.NextInt(6),
            mousePos = new float3(mousePos),
            mouseClicked = mouseClicked,
        }.Schedule(this, inputDependencies);

        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;
        initBufferSystem_.AddJobHandleForProducer(job);


        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}