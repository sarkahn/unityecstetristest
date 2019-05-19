using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGame : MonoBehaviour
{
    [SerializeField]
    float hideQuitTime_ = 3.5f;

    float timer_ = 0;

    [SerializeField]
    string quitSceneName_ = "QuitGameScene";
    

    // Update is called once per frame
    void Update()
    {
        bool sceneLoaded = QuitSceneLoaded();

        if (sceneLoaded)
            timer_ -= Time.deltaTime;
        else
            timer_ = hideQuitTime_;

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (sceneLoaded)
                Application.Quit();
            else
            {
                SceneManager.LoadScene(quitSceneName_, LoadSceneMode.Additive);
            }
        }

        if (timer_ <= 0)
        {
            SceneManager.UnloadSceneAsync(quitSceneName_);
        }
    }

    bool QuitSceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name == quitSceneName_)
            {
                return true;
            }
        }

        return false;
    }
}
