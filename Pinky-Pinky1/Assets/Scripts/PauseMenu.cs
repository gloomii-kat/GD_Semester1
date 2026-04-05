using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false; // Static variable to track if the game is paused

    public GameObject pauseMenuUI; // Reference to the pause menu UI GameObject
    public GameObject tutorialPanelUI; // Reference to the tutorial panel UI GameObject
    public Button playTutorialButton; // Reference to the play button inside tutorial panel

    void Start()
    {
        // Show tutorial panel when scene loads
        if (tutorialPanelUI != null)
        {
            tutorialPanelUI.SetActive(true);
            // Pause the game when tutorial is showing
            Time.timeScale = 0f;
            GameIsPaused = true;
        }

        // Setup play button click event for tutorial panel
        if (playTutorialButton != null)
        {
            playTutorialButton.onClick.AddListener(CloseTutorial);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only handle Escape if the game hasn't ended
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    private void HandleEscapeKey()
    {
        // If tutorial panel is open
        if (tutorialPanelUI != null && tutorialPanelUI.activeSelf)
        {
            CloseTutorial();
        }
        // If pause menu is open
        else if (pauseMenuUI != null && pauseMenuUI.activeSelf)
        {
            Resume();
        }
        // If game is not paused and not ended, pause the game
        else if (!GameIsPaused)
        {
            Pause();
        }
        // If game is paused but nothing is open (edge case), resume
        else if (GameIsPaused)
        {
            Resume();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Deactivate the pause menu UI
        Time.timeScale = 1f; // Set the time scale back to normal
        GameIsPaused = false; // Update the paused state
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true); // Activate the pause menu UI
        Time.timeScale = 0f; // Freeze the game by setting time scale to 0
        GameIsPaused = true; // Update the paused state
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game..."); // Log message for debugging
        Application.Quit(); // Quit the application
    }

    public void LoadMenu()
    {
        Debug.Log("Loading Main Menu..."); // Log message for debugging
        Time.timeScale = 1f; // Ensure time scale is reset before loading the menu
        SceneManager.LoadScene("0"); // Load the main menu scene
    }

    // Tutorial panel methods
    public void ShowTutorial()
    {
        if (tutorialPanelUI != null)
        {
            tutorialPanelUI.SetActive(true);
            Time.timeScale = 0f;
            GameIsPaused = true;

            // If pause menu is open, close it
            if (pauseMenuUI != null && pauseMenuUI.activeSelf)
            {
                pauseMenuUI.SetActive(false);
            }
        }
    }

    public void CloseTutorial()
    {
        if (tutorialPanelUI != null)
        {
            tutorialPanelUI.SetActive(false);

            // Check if we should resume the game or show pause menu
            if (pauseMenuUI != null && pauseMenuUI.activeSelf)
            {
                // If pause menu is open, stay paused
                Time.timeScale = 0f;
                GameIsPaused = true;
            }
            else
            {
                // Resume game
                Time.timeScale = 1f;
                GameIsPaused = false;
            }
        }
    }

    void OnDestroy()
    {
        // Clean up button listeners
        if (playTutorialButton != null)
        {
            playTutorialButton.onClick.RemoveListener(CloseTutorial);
        }
    }
}
