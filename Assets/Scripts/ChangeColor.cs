using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Material mat_;

    private void OnEnable()
    {
        mat_ = GetComponent<Material>();
        if (mat_ == null)
            enabled = false;
    }

    

    public Color a_ = Color.white;
    public Color b_ = Color.black;

    MaterialPropertyBlock props;

    // Update is called once per frame
    void Update()
    {
        if( props == null )
        {
            props = new MaterialPropertyBlock();
        }

        float t = Mathf.Repeat(Time.time, 1);
        Color col = Color.Lerp(a_, b_, t);

        props.SetColor("_Albedo", col);

        



    }
}
