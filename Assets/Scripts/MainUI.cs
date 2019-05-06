using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public FPPCharacterController player;
    public Text momentumText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        momentumText.text = "Momentum: " + player.forwardMomentum;
    }
}
