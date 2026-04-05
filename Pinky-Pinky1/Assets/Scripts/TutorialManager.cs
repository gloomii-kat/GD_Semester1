using UnityEngine;
using System.Collections.Generic;
using System.Collections;           

public class TutorialManager : MonoBehaviour
{

    public GameObject[] popups; // Array to hold tutorial step GameObjects
    private int popUpIndex; // Index to track current tutorial step

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for 
            (int i = 0; i < popups.Length; i++)
        {
            if (i == popUpIndex)
            {
                popups[i].SetActive(true); // Show current tutorial step
            }
            else
            {
                popups[i].SetActive(false); // Hide other steps
            }
        }

        if (popUpIndex == 0)
        {
            //if player presses WASD, move to the next tutorial step
        }
    }
}
