using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AwarenessScript : MonoBehaviour
{
    [SerializeField] public Slider slider;
    public Gradient gradient;

    [SerializeField] private Image fillImage;

    // Optional: For more control over color transitions
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color highColor = Color.blue;
    [SerializeField] private float midPoint = 0.5f;

    void Start()
    {
        if (fillImage == null && slider != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }

        // Create gradient programmatically if desired
        if (gradient == null || gradient.colorKeys.Length == 0)
        {
            CreateSimpleGradient();
        }

        if (fillImage != null)
        {
            fillImage.color = gradient.Evaluate(1f);
        }
    }

    void CreateSimpleGradient()
    {
        gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(lowColor, 0f);
        colorKeys[1] = new GradientColorKey(midColor, midPoint);
        colorKeys[2] = new GradientColorKey(highColor, 1f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, midPoint);
        alphaKeys[2] = new GradientAlphaKey(1f, 1f);

        gradient.SetKeys(colorKeys, alphaKeys);
    }

    public void SetMaxAwareness(int awareness)
    {
        slider.maxValue = awareness;
        slider.value = awareness;

        if (fillImage != null)
        {
            fillImage.color = gradient.Evaluate(1f);
        }
    }

    public void SetAwareness(int awareness)
    {
        slider.value = awareness;

        if (fillImage != null && slider.maxValue > 0)
        {
            float normalizedValue = Mathf.Clamp01(awareness / slider.maxValue);
            fillImage.color = gradient.Evaluate(normalizedValue);
        }
    }
}