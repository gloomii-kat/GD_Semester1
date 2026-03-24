using UnityEngine;

public class EndGameCondition : MonoBehaviour
{
    [SerializeField] private AwarenessScript awarenessScript; // Reference to your awareness script
    [SerializeField] private SceneLoader sceneLoader; // Reference to your scene loader

    [Header("End Game Settings")]
    public bool checkOnUpdate = true; // Check every frame
    public float delayBeforeLoading = 0f; // Optional delay before loading scene
    public int endGameSceneID = 1; // The scene ID to load when game ends

    private bool hasEnded = false;
    public GirlSounds girlSounds;

    void Start()
    {
        // Try to find AwarenessScript if not assigned
        if (awarenessScript == null)
        {
            awarenessScript = FindObjectOfType<AwarenessScript>();
        }

        // Try to find SceneLoader if not assigned
        if (sceneLoader == null)
        {
            sceneLoader = FindObjectOfType<SceneLoader>();
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
                if (girlSounds != null)
                {
                    girlSounds.PlayScream(); // Or PlayMultipleBreath() for a different reaction
                    Debug.Log("Playing girl scream sound");
                }

                TriggerEndGame();
            }
        }

        // Optional: Press ESC to instantly end game (for testing)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TriggerEndGame();
        }
    }

    public void TriggerEndGame()
    {
        if (hasEnded || sceneLoader == null) return;

        hasEnded = true;

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

    private System.Collections.IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(delayBeforeLoading);
        LoadEndScene();
    }

    private void LoadEndScene()
    {
        if (sceneLoader != null)
        {
            // Call MoveToScene with the end game scene ID
            sceneLoader.ChangeScene(endGameSceneID);
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