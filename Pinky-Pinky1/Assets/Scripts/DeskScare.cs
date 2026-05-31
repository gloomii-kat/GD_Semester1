using UnityEngine;
using System.Collections;

public class DeskScare : MonoBehaviour
{
    public float shakeDuration = 1f;
    public float shakeStrength = 0.1f;

    private Vector3 startPos;
    private bool used = false;

    void Start()
    {
        startPos = transform.position;
    }

    public void TriggerScare()
    {
        if (used)
            return;

        used = true;

        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            transform.position =
                startPos +
                (Vector3)Random.insideUnitCircle * shakeStrength;

            yield return null;
        }

        transform.position = startPos;
    }
}
