using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ConvertHierarchyTest : MonoBehaviour
{
    Entity original;

    // Update is called once per frame
    void Start()
    {
        original = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObject, World.Active);
    }

    private void Update()
    {
        if( Input.GetButtonDown("Jump"))
        {
            var e = World.Active.EntityManager.Instantiate(original);
            World.Active.EntityManager.SetComponentData(e, new Translation { Value = new float3(0, 0, 0) });
        }
    }
}
