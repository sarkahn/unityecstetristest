using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI counterText_;

    [SerializeField]
    float counterValue_ = 3;

    private void Awake()
    {
        foreach (var system in World.Active.Systems)
            system.Enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        counterText_.text = ((int)(counterValue_)).ToString();
        counterValue_ -= Time.deltaTime;

        if( counterValue_ <= 0 )
        {
            gameObject.SetActive(false);


            foreach (var system in World.Active.Systems)
                system.Enabled = true;
        }
    }
}
