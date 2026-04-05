using UnityEngine;
using UnityEngine.Audio;
public class EndGameCondition : MonoBehaviour
{
    [SerializeField] private AwarenessScript awarenessScript; // Reference to your awareness script
    [SerializeField] private SceneLoader sceneLoader; // Reference to your scene loader
    private AudioManager audioManager; // Reference to AudioManager

    [Header("End Game Settings")]
    public bool checkOnUpdate = true; // Check every frame
    public float delayBeforeLoading = 0f; // Optional delay before loading scene
    public int endGameSceneID = 1; // The scene ID to load when game ends

    [Header("Audio Settings")]
    public bool playEvilLaugh = true; // Play evil laugh when game ends
    public bool playChildScream = true; // Play child scream when game ends
    public bool keepMusicPlaying = true; // Set to true to keep background music playing
    public bool fadeOutMusic = false; // Fade out music instead of stopping immediately
    public float musicFadeOutDuration = 2f; // How long to fade out music

    private bool hasEnded = false;

    private void Awake()
    {
        // Find AudioManager by tag
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManager = audioObject.GetComponent<AudioManager>();
        }

        if (audioManager == null)
        {
            Debug.LogError("EndGameCondition: No AudioManager found with tag 'Audio'!");
        }
    }

    void Start()
    {
        // Try to find AwarenessScript if not assigned
        if (awarenessScript == null)
        {
            awarenessScript = FindFirstObjectByType<AwarenessScript>();
        }

        // Try to find SceneLoader if not assigned
        if (sceneLoader == null)
        {
            sceneLoader = FindFirstObjectByType<SceneLoader>();
        }

        if (awarenessScript == null)
        {
            Debug.LogError("EndGameCondition: No AwarenessScript found in scene!");
        }

        if (sceneLoader == null)
        {
            Debug.LogError("EndGameCondition: No SceneLoader found in scene!");
        }
    }

    void Update()
    {
        // Check if awareness is full
        if (checkOnUpdate && !hasEnded && awarenessScript != null)
        {
            if (awarenessScript.slider.value >= awarenessScript.slider.maxValue)
            {
                TriggerEndGame();
            }
        }

        // Use End key for testing
        if (Input.GetKeyDown(KeyCode.End))
        {
            TriggerEndGame();
        }
    }

    public void TriggerEndGame()
    {
        if (hasEnded || sceneLoader == null) return;

        hasEnded = true;

        // Play end game sounds using AudioManager
        PlayEndGameSounds();

        // Handle background music based on settings
        if (!keepMusicPlaying && audioManager != null)
        {
            if (fadeOutMusic && musicFadeOutDuration > 0)
            {
                // Fade out music
                StartCoroutine(audioManager.FadeOutMusic(musicFadeOutDuration));
            }
            else
            {
                // Stop music immediately
                audioManager.StopMusic();
            }
        }
        else if (keepMusicPlaying)
        {
            Debug.Log("Background music continues playing during end game");
        }

        // Unpause the game before loading new scene
        Time.timeScale = 1f;
        PauseMenu.GameIsPaused = false;

        if (delayBeforeLoading > 0)
        {
            // Start coroutine if MonoBehaviour supports it
            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(DelayedLoad());
            }
            else
            {
                // If object is inactive, load immediately
                LoadEndScene();
            }
        }
        else
        {
            LoadEndScene();
        }
    }

    private void PlayEndGameSounds()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("Cannot play end game sounds: AudioManager not found!");
            return;
        }

        // Play child scream if enabled
        if (playChildScream && audioManager.ChildScream != null)
        {
            audioManager.PlaySFX(audioManager.ChildScream);
            Debug.Log("Playing child scream");
        }
        else if (playChildScream)
        {
            Debug.LogWarning("ChildScream clip not assigned in AudioManager!");
        }

        // Play evil laugh if enabled
        if (playEvilLaugh && audioManager.EvilLaugh != null)
        {
            audioManager.PlaySFX(audioManager.EvilLaugh);
            Debug.Log("Playing evil laugh");
        }
        else if (playEvilLaugh)
        {
            Debug.LogWarning("EvilLaugh clip not assigned in AudioManager!");
        }
    }

    private System.Collections.IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(delayBeforeLoading);
        LoadEndScene();
    }

    private void LoadEndScene()
    {
        if (sceneLoader != null)
        {
            // Make sure AudioManager continues to next scene if we want music to keep playing
            if (keepMusicPlaying && audioManager != null)
            {
                // Don't destroy the AudioManager when loading new scene
                DontDestroyOnLoad(audioManager.gameObject);
                Debug.Log("AudioManager persisted to next scene");

                // Optional: Add a listener to handle scene load
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            }

            // Load the end game scene
            sceneLoader.ChangeScene(endGameSceneID);
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Check if we loaded the end game scene
        if (scene.buildIndex == endGameSceneID)
        {
            Debug.Log("End game scene loaded - music continues playing");

            // Optional: Stop music after a few seconds in the end scene
            if (audioManager != null && keepMusicPlaying)
            {
                StartCoroutine(StopMusicAfterDelay(5f));
            }
        }

        // Unsubscribe to avoid memory leaks
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private System.Collections.IEnumerator StopMusicAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use realtime since game might be paused

        if (audioManager != null)
        {
            // Optional: Fade out before stopping
            if (fadeOutMusic)
            {
                yield return StartCoroutine(audioManager.FadeOutMusic(musicFadeOutDuration));
            }
            else
            {
                audioManager.StopMusic();
            }
            Debug.Log("Music stopped after delay in end scene");
        }
    }

    // Public method to check if game has ended
    public bool HasGameEnded()
    {
        return hasEnded;
    }

    // Public method to reset end game state (useful for restarting)
    public void ResetEndGame()
    {
        hasEnded = false;
    }
}