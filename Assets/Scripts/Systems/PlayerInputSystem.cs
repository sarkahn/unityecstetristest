using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(GameLoopSystem))]
public class PlayerInputSystem : JobComponentSystem
{
    public const float repeatDelay_ = .05f;

    float repeatDelayTimer_ = 0;

    [BurstCompile]
    struct PlayerInputSystemJob : IJobForEach<PlayerInput>
    {
        public int rotation;
        public int movement;
        public bool instanceDrop;
        public bool fastFall;
        
        
        public void Execute(ref PlayerInput input)
        {
            input.rotation = rotation;
            input.movement = movement;
            input.instantDrop = instanceDrop;
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

        int mov = (int)Input.GetAxisRaw("Horizontal");

        if (mov == lastMov && lastMov != 0)
        {
            //Debug.Log("REPEAT MOVE. DELAY TIMER : " + repeatDelayTimer_) ;
            if (repeatDelayTimer_ > 0f)
            {
                mov = 0;
            }
            else
                repeatDelayTimer_ = repeatDelay_;

            repeatDelayTimer_ -= Time.deltaTime;
        }
        else
        {
            repeatDelayTimer_ = repeatDelay_;
            lastMov = mov;
        }

        bool drop = Input.GetButtonDown("Drop");
        bool fastFall = Input.GetAxisRaw("Vertical") == -1 ? true : false;

        var job = new PlayerInputSystemJob
        {
            rotation = rot,
            movement = mov,
            instanceDrop = drop,
            fastFall = fastFall,
        };

        
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}