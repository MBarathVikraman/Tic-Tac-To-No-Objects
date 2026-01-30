using System;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private Networking net;
    private GameManager board;
    [SerializeField] private InputDisabler input;
    [SerializeField] private Text_Buttons UI;

    public int MyValue { get; private set; } // 1=X, 2=O
    public bool InMatch { get; private set; }
    public int CurrentTurnValue { get; private set; } = 1;
    public bool IsMyTurn => InMatch && CurrentTurnValue == MyValue;

    private void Awake()
    {
        board = GameManager.Instance;

        if (input == null)
            input = FindFirstObjectByType<InputDisabler>();
        if (net == null)
            net = FindFirstObjectByType<Networking>();
        if (UI == null)
            UI = FindFirstObjectByType<Text_Buttons>();
    }

    private async void Start()
    {
        if (net == null)
        {
            Debug.LogError("MultiplayerManager: Networking not found in scene.");
            return;
        }

        net.Connected += OnNetConnected;
        Debug.Log("Connect() awaited");
        await net.Connect();
    }

    private async void OnNetConnected()
    {
        await SendFindMatch();
    }

    private void Update()
    {
        if (net == null) return;

        while (net.TryDequeue(out string json))
        {
            HandleMessage(json);
        }
    }

    public async System.Threading.Tasks.Task SendFindMatch()
    {
        int size = GameModeConfig.boardSize;

        string json = JsonUtility.ToJson(new WsMsg {
            type = WsMessageTypes.FindMatch,
            boardSize = size
        });

        await net.Send(json);

        Debug.Log("Sent find_match with size " + size);
    }


    public async void SendMove(int cellId)
    {
        if (!InMatch) return;
        if (net == null) return;

        input?.SetInputEnabled(false);

        await net.Send(
            $"{{\"type\":\"{WsMessageTypes.Move}\",\"cell\":{cellId}}}"
        );

        Debug.Log("Sent move: " + cellId);
    }

    private void HandleMessage(string json)
    {
        Debug.Log("WS <- " + json);
        var msg = JsonUtility.FromJson<WsMsg>(json);
        if (msg == null || string.IsNullOrEmpty(msg.type)) return;

        switch (msg.type)
        {
            case WsMessageTypes.Waiting:
                InMatch = false;
                input?.SetInputEnabled(false);
                break;

            case WsMessageTypes.MatchFound:
                InMatch = true;
                MyValue = msg.youAre;
                CurrentTurnValue = 1;
                if (msg.boardSize > 0)
                GameModeConfig.boardSize = msg.boardSize;
                board.ResetGame();
                board.isMyTurn = IsMyTurn;
                UI.HandleHeaderChange(-1, -1);
                input?.SetInputEnabled(IsMyTurn);
                Debug.Log($"Match found. I am {(MyValue == 1 ? "X" : "O")}");
                break;

            case WsMessageTypes.Move:
                board.TryPlayMove(msg.cell, msg.value);
                CurrentTurnValue = msg.nextTurn;
                input?.SetInputEnabled(IsMyTurn);
                break;

            case WsMessageTypes.GameOver:
                input?.SetInputEnabled(false);
                InMatch = false;
                break;

            case WsMessageTypes.OpponentLeft:
                Debug.Log("Opponent left!");
                UI.HandleDisconnect();
                InMatch = false;
                input?.SetInputEnabled(false);
                break;

            case WsMessageTypes.Error:
                Debug.LogError("Server error: " + msg.message);
                input?.SetInputEnabled(true);
                break;
        }
    }

    public async void GoBackToMenu()
    {
        InMatch = false;
        input?.SetInputEnabled(false);

        if (net != null)
            await net.Close();
    }

    public async void RestartOnline()
    {
        input?.SetInputEnabled(false);
        InMatch = false;

        board.ResetGame();

        await net.Close();
        await net.Connect();
        await SendFindMatch();
    }

    [Serializable]
    private class WsMsg
    {
        public string type;
        public int cell;
        public int value;
        public int nextTurn;
        public string matchId;
        public int youAre;
        public int winner;
        public string message;
        public int boardSize;
    }
    private static class WsMessageTypes
    {
        public const string FindMatch    = "find_match";
        public const string Waiting      = "waiting";
        public const string MatchFound   = "match_found";
        public const string Move         = "move";
        public const string GameOver     = "game_over";
        public const string OpponentLeft = "opponent_left";
        public const string Error        = "error";
    }
}
