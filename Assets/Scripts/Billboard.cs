using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    public bool lockToY = true;
    public enum AimModes
    {
        LookAtCamera,
        CopyCameraForward
    }
    public AimModes aimMode = AimModes.LookAtCamera;

    void Update()
    {

        switch(aimMode)
        {
            case AimModes.LookAtCamera:
                transform.LookAt(Camera.main.transform.position, Vector3.up);

                break;
            case AimModes.CopyCameraForward:
                transform.forward = -Camera.main.transform.forward;
                break;
        }
        
        if (lockToY)
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

}
