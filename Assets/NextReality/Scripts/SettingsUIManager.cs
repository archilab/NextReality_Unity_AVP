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

    [Header("Voice Command Fallback Buttons")]
    public Button voiceSaveSceneBtn;
    public Button voiceLoadSceneBtn;
    public Button voiceClearSceneBtn;
    public Button voiceRecenterBtn;
    public GameObject voiceStatusIndicator;
    public TextMeshProUGUI voiceStatusText;

    [Header("Systems")]
    public AsyncNetworkManager networkManager;
    public SceneDataHandler    sceneDataHandler;
    public QRManager           qrManager;           // make sure this is assigned    
    public VoiceCommandManager voiceCommandManager;  // Reference to voice command manager
    
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

        // Voice command fallback buttons
        if (voiceSaveSceneBtn != null)
            voiceSaveSceneBtn.onClick.AddListener(() => voiceCommandManager?.SaveScene());
        if (voiceLoadSceneBtn != null)
            voiceLoadSceneBtn.onClick.AddListener(() => voiceCommandManager?.LoadScene());
        if (voiceClearSceneBtn != null)
            voiceClearSceneBtn.onClick.AddListener(() => voiceCommandManager?.ClearScene());
        if (voiceRecenterBtn != null)
            voiceRecenterBtn.onClick.AddListener(() => voiceCommandManager?.RecenterQR());

        // Update indicator on connection events
        networkManager.OnConnectionStateChanged += UpdateStatusIndicator;
    }

    void Start()
    {
        // Initialize voice status if available
        if (voiceCommandManager != null)
        {
            UpdateVoiceStatus();
        }
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
        UpdateVoiceStatus();
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

    private void UpdateVoiceStatus()
    {
        if (voiceCommandManager != null && voiceStatusIndicator != null)
        {
            bool isVoiceAvailable = voiceCommandManager.GetComponent<AppleSpeechRecognizer>()?.IsRecognitionAvailable ?? false;
            
            // Update status indicator
            var renderer = voiceStatusIndicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = isVoiceAvailable ? green : red;
            }
            
            // Update status text
            if (voiceStatusText != null)
            {
                voiceStatusText.text = isVoiceAvailable ? "Voice: Available" : "Voice: Unavailable";
                voiceStatusText.color = isVoiceAvailable ? green : red;
            }
        }
    }

    /// <summary>
    /// Public methods that can be called from voice commands
    /// </summary>
    public void OpenSettings() => Show();
    public void CloseSettings() => Hide();
}
