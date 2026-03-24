using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AwarenessScript : MonoBehaviour
{
    [SerializeField] public Slider slider;
    public Gradient gradient;

    public void SetMaxAwareness(int awareness)
    {
        slider.maxValue = awareness;
        slider.value = awareness;

        gradient.Evaluate(1f);
    }

    public void SetAwareness(int awareness)
    {
        slider.value = awareness;
    }
}