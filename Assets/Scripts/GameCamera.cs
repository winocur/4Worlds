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

    public void AnimateEnterSlide ()
    {
        LeanTween.moveLocal(cameraObject, this.initialCameraPosition + new Vector3(0, -1, 0), 0.3f)
                .setEaseOutQuad();
    }

    public void AnimateExitSlide ()
    {
        LeanTween.moveLocal(cameraObject, this.initialCameraPosition, 0.3f)
                .setEaseOutQuad();
    }

}