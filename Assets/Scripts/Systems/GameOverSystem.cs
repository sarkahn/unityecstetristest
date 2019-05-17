using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSystem : ComponentSystem
{

    protected override void OnCreate()
    {
        // Calling GetEntityQuery will add the query to this system's list of queries it
        // uses to determine when to call OnUpdate.
        GetEntityQuery(typeof(GameOver));
    }

    protected override void OnUpdate()
    {
        //foreach (var system in World.Systems)
        //    system.Enabled = false;

        //SceneManager.LoadSceneAsync("GameOverScene", LoadSceneMode.Additive);
    }
}