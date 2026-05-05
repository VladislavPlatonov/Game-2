using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu3DController : MonoBehaviour
{
    [Header("Scene names")]
    [SerializeField] private string gameSceneName = "bckup";

    public void ExecuteAction(Menu3DActionType actionType)
    {
        MenuAudioManager.Instance?.PlayClick();

        switch (actionType)
        {
            case Menu3DActionType.NewGame:
                NewGame();
                break;

            case Menu3DActionType.Load:
                Debug.Log("LOAD button clicked");
                break;

            case Menu3DActionType.Settings:
                Debug.Log("SETTINGS button clicked");
                break;

            case Menu3DActionType.Exit:
                Debug.Log("EXIT button clicked");
                Application.Quit();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    public void NewGame()
    {
        Debug.Log("Loading scene: " + gameSceneName);
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }
}
