using UnityEngine;

public class ModeInitializer : MonoBehaviour
{
    [SerializeField] private GameObject botRoot;
    [SerializeField] private GameObject onlineNetwork;
    [SerializeField] private GameObject onlineManager;


    private void Awake()
    {
        botRoot.SetActive(false);
        onlineNetwork.SetActive(false);
        onlineManager.SetActive(false);
        switch (GameModeConfig.Mode)
        {
            case GameMode.Local:
                GameModeConfig.boardSize=3;
                break;

            case GameMode.Bot:
                GameModeConfig.boardSize=3;
                botRoot.SetActive(true);
                break;

            case GameMode.Online:
                GameModeConfig.boardSize=3;
                onlineNetwork.SetActive(true);
                onlineManager.SetActive(true);
                break;
        }

        Debug.Log("Loaded Mode: " + GameModeConfig.Mode);
    }
}

