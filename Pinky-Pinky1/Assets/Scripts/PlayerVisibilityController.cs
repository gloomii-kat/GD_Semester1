using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerVisibilityController : MonoBehaviour
{
    [Header("Visibility")]
    public SpriteRenderer playerRenderer;

    [Range(0f, 1f)]
    public float hiddenAlpha = 0.55f;

    [Range(0f, 1f)]
    public float visibleAlpha = 1f;

    [Header("Fade Animation")]
    public float fadeDuration = 1f;

    [Header("Reveal")]
    public float visibleDuration = 10f;

    [Header("Night Manager")]
    public NightManager nightManager;

    [Header("Optional Events")]
    public UnityEvent onPlayerRevealed;
    public UnityEvent onPlayerHidden;

    private bool isVisible = false;
    private float visibleTimer;
    private bool nightEndTriggered = false;

    private Coroutine fadeCoroutine;

    void Start()
    {
        isVisible = false;

        if (playerRenderer != null)
        {
            Color c = playerRenderer.color;
            c.a = hiddenAlpha;
            playerRenderer.color = c;
        }
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.R))
        {
            RevealPlayer();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            SetHidden();
        }*/

        if (isVisible)
        {
            visibleTimer -= Time.deltaTime;

            if (visibleTimer <= 0f)
            {
                EndChase();
            }
        }
    }

    // Called when child panics
    public void RevealPlayer()
    {
        isVisible = true;
        nightEndTriggered = false;
        visibleTimer = visibleDuration;

        FadeToAlpha(visibleAlpha);

        onPlayerRevealed?.Invoke();
    }

    private void EndChase()
    {
        isVisible = false;

        FadeToAlpha(hiddenAlpha);

        if (!nightEndTriggered)
        {
            nightEndTriggered = true;

            onPlayerHidden?.Invoke();

            if (nightManager != null)
                nightManager.OnNightComplete();
        }
    }

    public void SetHidden()
    {
        isVisible = false;
        FadeToAlpha(hiddenAlpha);
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    private void FadeToAlpha(float targetAlpha)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = playerRenderer.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float newAlpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                timer / fadeDuration
            );

            Color c = playerRenderer.color;
            c.a = newAlpha;
            playerRenderer.color = c;

            yield return null;
        }

        // Ensure exact final value
        Color finalColor = playerRenderer.color;
        finalColor.a = targetAlpha;
        playerRenderer.color = finalColor;
    }
}
