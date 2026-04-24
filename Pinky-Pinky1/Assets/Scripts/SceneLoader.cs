using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int sceneID;
    // For buttons (no parameter)
    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneID);
    }

    // For scripts (with parameter)
    public void LoadNextScene(int id)
    {
        SceneManager.LoadScene(id);
    }
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}