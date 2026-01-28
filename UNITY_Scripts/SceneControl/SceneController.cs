using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LocalMultiplayer()
    {
        GameModeConfig.Mode = GameMode.Local;
        SceneManager.LoadScene("GameScene");
    }

    public void PlayVsBot()
    {
        GameModeConfig.Mode = GameMode.Bot;
        SceneManager.LoadScene("GameScene");
    }

    public void Online()
    {
        GameModeConfig.Mode = GameMode.Online;
        SceneManager.LoadScene("GameScene");
    }

    public void GoBack()
    {
        if (GameModeConfig.Mode == GameMode.Online)
        {
            var mp = FindFirstObjectByType<MultiplayerManager>();
            if (mp != null)
                mp.GoBackToMenu();
        }
        GameModeConfig.Mode = GameMode.MainMenu;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
