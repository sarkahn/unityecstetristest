using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteAlways]
public class CameraManager : MonoBehaviour
{
    [SerializeField]
    Camera[] cameras_;

    [SerializeField]
    Canvas canvas_;

    [SerializeField]
    [HideInInspector]
    int currentCam_ = 0;

    private void OnEnable()
    {
        if( cameras_.Length != 2 || cameras_[0] == null || cameras_[1] == null )
        {
            Debug.LogError("Cameras not set properly", this);
            enabled = false;
        }

        if( canvas_ == null )
        {
            Debug.LogError("Canvas not set");
            enabled = false;
        }
        
        for( int i = 0; i < 2; ++i )
            if( cameras_[i].enabled )
            {
                currentCam_ = i;
                return;
            }
    }

    [ContextMenu("Switch Cameras")]
    public void SwitchCameras()
    {
        var oldcam = cameras_[currentCam_];

        currentCam_ = 1 - currentCam_;

        var newCam = cameras_[currentCam_];

        oldcam.gameObject.SetActive(false);
        newCam.gameObject.SetActive(true);

        canvas_.worldCamera = newCam;
        
    }

}
