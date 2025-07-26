// SettingsUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the in‑app settings panel: WebSocket config, scene operations, connection status.
/// </summary>
public class SettingsUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsCanvas;
    public TMP_InputField apiKeyInput; // new field in the Inspector
    public TMP_InputField ipInput;
    public TMP_InputField portInput;
    public Button saveConfigBtn;
    public Button connectBtn;
    public Button saveSceneBtn;
    public Button loadSceneBtn;
    public Button clearSceneBtn;
    public Button closeBtn;
    public Image  statusIndicator;
    public Button recenterQrBtn;




    [Header("Systems")]
    public AsyncNetworkManager networkManager;
    public SceneDataHandler    sceneDataHandler;
    public QRManager           qrManager;           // make sure this is assigned    
    private readonly Color green  = Color.green;
    private readonly Color yellow = Color.yellow;
    private readonly Color red    = Color.red;


    void Awake()
    {
        // Load persisted IP/port or defaults
        ipInput.text   = PlayerPrefs.GetString("ServerIP",   "192.168.1.100");
        portInput.text = PlayerPrefs.GetInt   ("ServerPort", 8080).ToString();
        apiKeyInput.text = PlayerPrefs.GetString("OpenAIAPIKey", "");

        // Button wiring
        saveConfigBtn.onClick.AddListener(OnSaveConfig);
        connectBtn   .onClick.AddListener(OnConnectClicked);
        saveSceneBtn .onClick.AddListener(sceneDataHandler.SaveScene);
        loadSceneBtn .onClick.AddListener(sceneDataHandler.LoadSceneNetworked);
        clearSceneBtn.onClick.AddListener(sceneDataHandler.ClearSceneNetworked);
        closeBtn     .onClick.AddListener(Hide);
        recenterQrBtn.onClick.AddListener(() => qrManager.RecenterFromQR());

        // Update indicator on connection events
        networkManager.OnConnectionStateChanged += UpdateStatusIndicator;
    }

    void OnDestroy()
    {
        networkManager.OnConnectionStateChanged -= UpdateStatusIndicator;
    }

    /// <summary>
    /// Show the settings panel.
    /// </summary>
    public void Show()
    {
        settingsCanvas.SetActive(true);
        UpdateStatusIndicator(networkManager.State);
    }

    /// <summary>
    /// Hide the settings panel.
    /// </summary>
    public void Hide()
    {
        settingsCanvas.SetActive(false);
    }

    private void OnSaveConfig()
    {
        PlayerPrefs.SetString("ServerIP",     ipInput.text);
        if (int.TryParse(portInput.text, out int p))
            PlayerPrefs.SetInt("ServerPort", p);
        PlayerPrefs.SetString("OpenAIAPIKey", apiKeyInput.text); // save key
        PlayerPrefs.Save();
        Debug.Log("[Settings] Configuration saved.");
    }

    private void OnConnectClicked()
    {
        networkManager.ConnectToServer();
    }

    private void UpdateStatusIndicator(AsyncNetworkManager.ConnectionState state)
    {
        statusIndicator.color = state switch
        {
            AsyncNetworkManager.ConnectionState.Connected => green,
            AsyncNetworkManager.ConnectionState.Connecting => yellow,
            _ =>                                        red
        };
    }
}
