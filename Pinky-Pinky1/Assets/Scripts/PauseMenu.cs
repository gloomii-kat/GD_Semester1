using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false; // Static variable to track if the game is paused

    public GameObject pauseMenuUI; // Reference to the pause menu UI GameObject

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Check if the Escape key is pressed
        {
            if (GameIsPaused)
            {
                Resume(); // If the game is paused, resume it
            }
            else
            {
                Pause(); // If the game is not paused, pause it
            }
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
}
