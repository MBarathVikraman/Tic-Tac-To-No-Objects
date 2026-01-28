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
                break;

            case GameMode.Bot:
                botRoot.SetActive(true);
                break;

            case GameMode.Online:
                onlineNetwork.SetActive(true);
                onlineManager.SetActive(true);
                break;
        }

        Debug.Log("Loaded Mode: " + GameModeConfig.Mode);
    }
}

