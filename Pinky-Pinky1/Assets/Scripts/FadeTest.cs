using UnityEngine;
using System.Collections;

public class FadeTest : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Fade(1f));
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(Fade(0.2f));
        }
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = sr.color.a;
        float timer = 0f;
        float duration = 1f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            Color c = sr.color;

            c.a = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);

            sr.color = c;

            yield return null;
        }
    }
}
