using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspicionScript : MonoBehaviour
{
    public Slider slider;
    [SerializeField] private TMP_Text suspicionText;

    public void SetMaxSuspicion(int value)
    {
        slider.maxValue = value;
        slider.value = 0;
        UpdateText();
    }

    public void SetSuspicion(int value)
    {
        slider.value = value;
        UpdateText();
    }

    public void AddSuspicion(int amount)
    {
        if (slider == null) return;

        slider.value += amount;
        slider.value = Mathf.Clamp(slider.value, slider.minValue, slider.maxValue);
        UpdateText();
    }

    public bool IsFull()
    {
        return slider != null && slider.value >= slider.maxValue;
    }

    void UpdateText()
    {
        if (suspicionText != null && slider != null)
        {
            suspicionText.text = Mathf.RoundToInt(slider.value) + " / " + Mathf.RoundToInt(slider.maxValue);
        }
    }
}
