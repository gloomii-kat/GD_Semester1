using UnityEngine;
using System.Collections;

public class SecondChildActivator : MonoBehaviour
{
    [Header("Second Child")]
    [Tooltip("Drag the second child GameObject here - should start inactive")]
    public GameObject secondChild;

    [Header("Delay")]
    [Tooltip("How many seconds before the second child appears")]
    public float activationDelay = 15f;

    void Start()
    {
        // Make sure second child starts hidden
        if (secondChild != null)
        {
            secondChild.SetActive(false);
            StartCoroutine(ActivateAfterDelay());
        }
        else
        {
            Debug.LogError("Second child not assigned in SecondChildActivator!");
        }
    }

    IEnumerator ActivateAfterDelay()
    {
        Debug.Log("Second child will activate in " + activationDelay + " seconds");
        yield return new WaitForSeconds(activationDelay);
        secondChild.SetActive(true);
        Debug.Log("Second child activated");
    }
}
