using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Text_Buttons : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject containerPanel;
    [SerializeField] private TextMeshProUGUI showTurn;
    [SerializeField] private InputDisabler inputCtrl;
    [SerializeField] private MultiplayerManager mp;
    private GameManager board;
    private void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
    }


    private void OnEnable()
    {
        board = GameManager.Instance;
        if (board == null) return;
        if (GameModeConfig.Mode==GameMode.Online && mp == null)
            mp = FindFirstObjectByType<MultiplayerManager>();

        board.OnGameFinished += HandleGameFinished;
        board.OnReset += ResetUI;
        board.OnMoveMade+=HandleHeaderChange;
    }

    private void OnDisable()
    {
        if (board == null) return;

        board.OnGameFinished -= HandleGameFinished;
        board.OnReset -= ResetUI;
        board.OnMoveMade-=HandleHeaderChange;

        board = null;
    }
    private void HandleGameFinished(int value, bool isWin, int[] cells)
    {
        if (isWin)
        {
            string winner = value==1? "X":"O";
            showTurn.text=$"Player {winner} Wins!";
        }
        else
        {
            showTurn.text="It's a Draw!";
        }
        containerPanel.SetActive(true);
    }
    public void HandleDisconnect()
    {
        showTurn.text="Opponent Left!";
        containerPanel.SetActive(true);
    }
    public void HandleHeaderChange(int cellId, int valuePlayed)
    {
        if (containerPanel.activeSelf) return;

        if (GameModeConfig.Mode == GameMode.Online && mp != null && mp.InMatch)
        {
            showTurn.text = board.isMyTurn ? "Your turn":"Opponent's turn";
            return;
        }

        showTurn.text = (valuePlayed == 1) ? "O's Turn" : "X's Turn";
    }

    private void ResetUI()
    {
        if (GameModeConfig.Mode == GameMode.Online)
            showTurn.text="Finding Opponent...";
        else
            showTurn.text = "Tic-Tac-Toe";
        containerPanel.SetActive(false);
    }
    private void RestartGame()
    {
        if (GameModeConfig.Mode == GameMode.Online)
        {
            if (mp==null) mp = FindFirstObjectByType<MultiplayerManager>();
            mp.RestartOnline();
            return;
        }
        GameManager.Instance.ResetGame();
    }

}
