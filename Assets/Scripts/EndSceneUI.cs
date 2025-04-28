using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneUI : MonoBehaviour
{
    public void RestartGame()
    {
        // reset all stats…
        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();

        // …then go back to your first scene (build index 0)
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}