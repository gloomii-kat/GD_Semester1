using UnityEngine;
using UnityEngine.SceneManagement;

public class NightManager : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Exact name of your Game Over scene")]
    public string gameOverSceneName = "GameOver";

    [Tooltip("Exact name of your Win scene")]
    public string winSceneName = "WinScreen";

    [Tooltip("Exact name of Night 2 scene")]
    public string night2SceneName = "Night2";

    [Tooltip("Exact name of Night 3 scene — leave empty if not built yet")]
    public string night3SceneName = "Night3";

    [Header("Which night is this?")]
    public int currentNight = 1; // Set this to 1, 2, or 3 in each scene's Inspector

    // Called by PlayerVisibilityController's OnPlayerHidden event when timer runs out
    public void OnNightComplete()
    {
        Debug.Log("Night " + currentNight + " complete");

        if (currentNight == 1)
        {
            if (!string.IsNullOrEmpty(night2SceneName))
                SceneManager.LoadScene(night2SceneName);
            else
                Debug.LogWarning("Night 2 scene name not set in NightManager");
        }
        else if (currentNight == 2)
        {
            if (!string.IsNullOrEmpty(night3SceneName))
                SceneManager.LoadScene(night3SceneName);
            else
                SceneManager.LoadScene(winSceneName); // if Night 3 not built yet, go to win
        }
        else if (currentNight == 3)
        {
            SceneManager.LoadScene(winSceneName);
        }
    }

    // Called by TeacherCatch when teacher catches the player
    public void OnGameOver()
    {
        Debug.Log("Game Over - teacher caught player");
        SceneManager.LoadScene(gameOverSceneName);
    }
}
