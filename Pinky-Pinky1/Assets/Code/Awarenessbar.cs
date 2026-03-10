using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AwarenessScript : MonoBehaviour
{

    [SerializeField] public int awarenessLevel = 0;
    [SerializeField] public Slider slider;
    public Gradient gradient;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        awarenessLevel = 0;
        slider.value = awarenessLevel;
    }

    // Update is called once per frame
    /*void Update()
    {
        ScaredNiggaz();
    }
    public void ScaredNiggaz()
    {
        if (slider.value >= 20)
        {

            Debug.Log("Im Scared");
            //Maybe play a scared animation or sound effect here
            //Fear bar goes up and unlocks new abilities or changes the behavior for the player,
            //such as making them more visible to enemies or allowing them to interact with certain objects in the environment.

        }
        else
        {
            Debug.Log("Im Not Scared");
            //Maybe play a calm animation or sound effect here
        }
    }*/

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.name);

        awarenessLevel += 10; // Increase awareness by 10 when player enters the trigger
        awarenessLevel = Mathf.Clamp(awarenessLevel, 0, 100); // Clamp between 0 and 100

        slider.value = awarenessLevel; // Update the slider value to match the current awareness level
    }

    /*private void Update()
    {
        slider.value = awarenessLevel; // Update the slider value to match the current awareness level
        slider.fillRect.GetComponent<Image>().color = gradient.Evaluate(slider.normalizedValue); // Change color based on gradient

            if (awarenessLevel >= 10)
        {
            Color redColor = Color.red;
        }
    }*/

}