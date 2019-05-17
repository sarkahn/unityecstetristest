using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverRestart : MonoBehaviour
{
    float t_ = 0;
    [SerializeField]
    float restartButtonDelay_ = 0.5f;

    // Update is called once per frame
    void Update()
    {
        t_ += Time.deltaTime;

        if( t_ >= restartButtonDelay_ && Input.anyKeyDown)
        {

            //SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
    }
}
