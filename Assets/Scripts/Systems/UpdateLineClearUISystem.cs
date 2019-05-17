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
    EntityQuery updateUIQuery_;
    LineClearUI lineClearUI_;

    protected override void OnCreate()
    {
        // We can't access any gameobjects until the scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
        updateUIQuery_ = GetEntityQuery(typeof(UpdateLineClearUI));
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
                if( lineClearUI_)
                    lineClearUI_.Value_ += updateUIComponent.linesClearedCount;
            });
        EntityManager.DestroyEntity(updateUIQuery_);
    }
}