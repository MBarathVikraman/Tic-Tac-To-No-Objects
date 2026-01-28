using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;

public class MultiplayerOLD : MonoBehaviour
{
    private string serverUrl = "ws://10.187.91.48:8080/ws";

    private WebSocket ws;
    private GameManager board;
    private InputDisabler input;

    public int MyValue { get; private set; } // 1=X, 2=O
    public bool InMatch { get; private set; }

    private void Awake()
    {
        board = GameManager.Instance;
        input = FindFirstObjectByType<InputDisabler>();
    }

    private async void Start()
    {
        ws = new WebSocket(serverUrl);

        ws.OnOpen += () =>
        {
            Debug.Log("WS Connected");
            FindMatch();
        };

        ws.OnError += (e) => Debug.LogError("WS Error: " + e);
        ws.OnClose += (e) => Debug.Log("WS Closed");

        ws.OnMessage += (bytes) =>
        {
            var json = Encoding.UTF8.GetString(bytes);
            HandleMessage(json);
        };

        await ws.Connect();
    }

    private void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
                ws?.DispatchMessageQueue();
        #endif
    }

    public async void FindMatch()
    {
        if (ws == null) return;
        await ws.SendText("{\"type\":\"find_match\"}");
        Debug.Log("Sent find_match");
    }

    public async void SendMove(int cellId)
    {
        if (!InMatch) return;
        if (ws == null) return;

        await ws.SendText($"{{\"type\":\"move\",\"cell\":{cellId}}}");
        Debug.Log("Sent move: " + cellId);

        // Lock input until server responds
        input?.SetInputEnabled(false);
    }

    private void HandleMessage(string json)
    {
        Debug.Log("WS <- " + json);
        var msg = JsonUtility.FromJson<WsMsg>(json);

        if (msg.type == "match_found")
        {
            InMatch = true;
            MyValue = msg.youAre;

            board.ResetGame();
            bool myTurn = MyValue == 1;
            input?.SetInputEnabled(myTurn);

            Debug.Log($"Match found. I am {(MyValue == 1 ? "X" : "O")}");
        }
        else if (msg.type == "move")
        {
            board.TryPlayMove(msg.cell, msg.value);
            bool myTurn = msg.nextTurn == MyValue;
            input?.SetInputEnabled(myTurn);
        }
        else if (msg.type == "game_over")
        {
            input?.SetInputEnabled(false);
        }
        else if (msg.type == "opponent_left")
        {
            Debug.Log("Opponent left!");
            input.SetInputEnabled(false);
            //restart option

        }
        else if (msg.type == "error")
        {
            Debug.LogError("Server error: " + msg.message);
            input?.SetInputEnabled(true);
        }
    }
    public async void CloseSocket()
    {
        if (ws != null)
            await ws.Close();
    }

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
    }

    private async void OnApplicationQuit()
    {
        if (ws != null)
            await ws.Close();
    }
}
