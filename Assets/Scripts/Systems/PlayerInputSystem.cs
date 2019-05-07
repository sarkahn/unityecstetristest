using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerInputSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    [BurstCompile]
    struct PlayerInputSystemJob : IJobForEach<PlayerInput>
    {
        public int rotation;
        public int movement;
        public bool drop;
        
        
        public void Execute(ref PlayerInput input)
        {
            input.rotation = rotation;
            input.movement = movement;
            input.drop = drop;
        }
    }

    int lastRot = 0;
    int lastMov = 0;
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        int rot = (int)Input.GetAxisRaw("Rotate");

        if (rot != 0 && lastRot != 0)
            rot = 0;
        else
            lastRot = rot;

        int mov = lastMov != 0 ? 0 : (int)Input.GetAxisRaw("Horizontal");

        bool drop = Input.GetButtonDown("Drop");

        var job = new PlayerInputSystemJob
        {
            rotation = rot,
            movement = mov,
            drop = drop,
        };
        
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}