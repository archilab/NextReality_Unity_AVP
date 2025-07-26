// AsyncNetworkManager.cs
using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// Handles asynchronous, lossless WebSocket messaging.
/// </summary>
public class AsyncNetworkManager : MonoBehaviour
{
    public enum ConnectionState { Disconnected, Connecting, Connected }
    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
    public event Action<ConnectionState> OnConnectionStateChanged;

    [Tooltip("ObjectManager reference for applying remote messages")]
    public ObjectManager objectManager;

    private ClientWebSocket socket;
    private CancellationTokenSource cts;
    private readonly ConcurrentQueue<string> sendQueue = new();

    void Awake()
    {
        if (objectManager == null)
            Debug.LogError("[AsyncNetworkManager] ObjectManager is not assigned.");
    }

    // Call this from UI to connect/reconnect
    public async void ConnectToServer()
    {
        string ip   = PlayerPrefs.GetString("ServerIP",   "192.168.1.100");
        int    port = PlayerPrefs.GetInt   ("ServerPort", 8080);
        Uri    uri  = new($"ws://{ip}:{port}");

        // Clean up any existing connection
        cts?.Cancel();
        socket?.Dispose();

        cts = new CancellationTokenSource();
        socket = new ClientWebSocket();

        UpdateState(ConnectionState.Connecting);
        try
        {
            await socket.ConnectAsync(uri, cts.Token);
            UpdateState(ConnectionState.Connected);
            Debug.Log($"[AsyncNet] Connected to {uri}");

            _ = SendLoop();
            _ = ReceiveLoop();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AsyncNet] Connection failed: {ex.Message}");
            UpdateState(ConnectionState.Disconnected);
        }
    }

    // Enqueue a network message for sending
    public void Enqueue(NetworkMessage msg)
    {
        if (msg == null) return;
        string json = JsonConvert.SerializeObject(msg);
        sendQueue.Enqueue(json);
    }

    // Continuously send queued messages
    private async Task SendLoop()
    {
        var buffer = new byte[0];
        while (State == ConnectionState.Connected && !cts.Token.IsCancellationRequested)
        {
            if (sendQueue.TryDequeue(out var json))
            {
                buffer = Encoding.UTF8.GetBytes(json);
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cts.Token);
                }
                catch
                {
                    // On failure, re-enqueue and break to allow reconnect
                    sendQueue.Enqueue(json);
                    break;
                }
            }
            else
            {
                await Task.Delay(10, cts.Token);
            }
        }

        if (State == ConnectionState.Connected)
            UpdateState(ConnectionState.Disconnected);
    }

    // Continuously receive incoming messages
    private async Task ReceiveLoop()
    {
        var buffer = new byte[4096];
        while (State == ConnectionState.Connected && !cts.Token.IsCancellationRequested)
        {
            try
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;

                string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var netMsg = JsonConvert.DeserializeObject<NetworkMessage>(msg);
                objectManager.HandleMessage(netMsg);
            }
            catch
            {
                break;
            }
        }

        if (State == ConnectionState.Connected)
            UpdateState(ConnectionState.Disconnected);
    }

    private void UpdateState(ConnectionState newState)
    {
        State = newState;
        OnConnectionStateChanged?.Invoke(State);
    }

    void OnDestroy()
    {
        cts?.Cancel();
        socket?.Dispose();
    }
}
