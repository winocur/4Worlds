using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour {


    public GameObject cameraObject;

    Vector3 initialCameraPosition;

    void Start ()
    {
        this.initialCameraPosition = cameraObject.transform.localPosition;
    }
    
    #region ANIMATIONS 
    public void AnimateBraceForJump (float braceTime)  {
        LeanTween.moveLocal(cameraObject, this.initialCameraPosition + new Vector3(0, -0.4f, 0), braceTime)
                .setEaseOutQuad();
    }

    public void AnimateJump (float jumpTime)  {
        LeanTween.moveLocal(cameraObject, this.initialCameraPosition, jumpTime)
                .setEaseInQuad();
    }

    public void AnimateEnterSlide ()  {
        LeanTween.moveLocal(cameraObject, this.initialCameraPosition + new Vector3(0, -1, 0), 0.3f)
                .setEaseOutQuad();
    }

    public void AnimateExitSlide () {
        LeanTween.moveLocal(cameraObject, this.initialCameraPosition, 0.3f)
                .setEaseOutQuad();
    }

    public void AnimateEnterWallrun (bool isRight) {
        LeanTween.rotateLocal(cameraObject, new Vector3 (0 , 0, (isRight) ? 15f : -15f), 0.5f)
            .setEaseOutCubic();
    }

    public void AnimateExitWallrun () {
        LeanTween.rotateLocal(cameraObject, new Vector3 (0 , 0, 0), 0.5f)
            .setEaseOutCubic();
    }
    #endregion

}