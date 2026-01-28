using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class Networking : MonoBehaviour
{
    [SerializeField] private string serverUrl = "ws://10.187.91.48:8080/ws";

    private WebSocket ws;
    public event Action Connected;
    private readonly ConcurrentQueue<string> incoming = new ConcurrentQueue<string>();
    public bool IsConnected { get; private set; }
    public async Task Connect()
    {
        if (ws != null){
            await Close();
        }
        ws = new WebSocket(serverUrl);

        ws.OnOpen += () =>
        {
            IsConnected = true;
            Debug.Log("WS Connected");
            Connected?.Invoke();
        };

        ws.OnError += (e) =>
        {
            Debug.LogError("WS Error: " + e);
        };

        ws.OnClose += (e) =>
        {
            IsConnected = false;
            Debug.Log("WS Closed");
        };

        ws.OnMessage += (bytes) =>
        {
            var json = Encoding.UTF8.GetString(bytes);
            incoming.Enqueue(json);
        };

        await ws.Connect();
    }

    public void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
    }

    public bool TryDequeue(out string json)
    {
        return incoming.TryDequeue(out json);
    }

    public async Task Send(string json)
    {
        if (ws == null) return;
        await ws.SendText(json);
    }

    public async Task Close()
    {
        if (ws == null) return;
        await ws.Close();
        ws = null;
        IsConnected = false;
    }

    private async void OnApplicationQuit()
    {
        await Close();
    }
}
