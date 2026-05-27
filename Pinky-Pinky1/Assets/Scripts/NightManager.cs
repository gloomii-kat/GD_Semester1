using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NightManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameOverSceneName = "GameOver";
    public string winSceneName = "WinScreen";
    public string night2SceneName = "Night2";
    //public string night3SceneName = "Night3";

    [Header("Night Settings")]
    public int currentNight = 1;

    [Header("Transition Settings")]
    public float delayBeforeNextNight = 3f;

    [Header("Audio")]
    public bool fadeOutMusic = true;
    public float musicFadeDuration = 2f;

    private bool transitioning = false;

    private AudioManager audioManager;

    void Awake()
    {
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
        {
            audioManager = audioObject.GetComponent<AudioManager>();
        }
    }

    public void OnNightComplete()
    {
        if (transitioning)
            return;

        transitioning = true;

        Debug.Log("Night survived!");

        StartCoroutine(NightCompleteRoutine());
    }

    IEnumerator NightCompleteRoutine()
    {
        if (audioManager != null)
        {
            audioManager.RestoreBackgroundMusic();

            if (fadeOutMusic)
            {
                yield return StartCoroutine(
                    audioManager.FadeOutMusic(musicFadeDuration)
                );
            }
        }

        yield return new WaitForSeconds(delayBeforeNextNight);

        LoadNextNight();
    }

    void LoadNextNight()
    {
        if (currentNight == 1)
        {
            SceneManager.LoadScene(night2SceneName);
        }
        /*else if (currentNight == 2)
        {
            if (!string.IsNullOrEmpty(night3SceneName))
                SceneManager.LoadScene(night3SceneName);
            else
                SceneManager.LoadScene(winSceneName);
        }*/
        else
        {
            SceneManager.LoadScene(winSceneName);
        }
    }

    public void OnGameOver()
    {
        if (transitioning)
            return;

        transitioning = true;

        Debug.Log("Teacher caught player");

        SceneManager.LoadScene(gameOverSceneName);
    }
}
