using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UpdateLineClearUISystem : ComponentSystem
{
    LineClearUI lineClearUI_;

    protected override void OnCreate()
    {
        // We can't access any gameobjects until the scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lineClearUI_ = GameObject.FindObjectOfType<LineClearUI>();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach(
            (Entity e, ref UpdateLineClearUI updateUIComponent)=>
            {
                lineClearUI_.Value_ += updateUIComponent.linesClearedCount;
                PostUpdateCommands.DestroyEntity(e);
            });

    }
}