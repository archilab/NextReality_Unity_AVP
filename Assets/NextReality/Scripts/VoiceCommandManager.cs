using UnityEngine;

public class VoiceCommandManager : MonoBehaviour
{
    [Header("Dependencies")]
    public QRManager qrManager;
    public SceneDataHandler sceneHandler;
    public SettingsUIManager settingsUI;
    
    [Header("Voice Recognition")]
    private AppleSpeechRecognizer recognizer;
    private bool _isVoiceAvailable = false;
    
    [Header("Fallback UI")]
    public GameObject voiceStatusIndicator; // Optional UI element to show voice status
    
    void Start()
    {
        InitializeSpeechRecognition();
        UpdateVoiceStatusUI();
    }

    private void InitializeSpeechRecognition()
    {
        try
        {
            recognizer = AppleSpeechRecognizer.Instance;
            if (recognizer == null)
            {
                recognizer = gameObject.AddComponent<AppleSpeechRecognizer>();
            }
            
            recognizer.OnTranscriptReceived += ProcessCommand;
            recognizer.OnRecognitionStatusChanged += OnRecognitionStatusChanged;
            
            _isVoiceAvailable = recognizer.IsRecognitionAvailable;
            
            if (_isVoiceAvailable)
            {
                recognizer.StartRecognition();
                Debug.Log("[Voice] Speech recognition initialized successfully");
            }
            else
            {
                Debug.LogWarning("[Voice] Speech recognition not available - using UI fallback only");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Voice] Failed to initialize speech recognition: {e.Message}");
            _isVoiceAvailable = false;
        }
    }

    private void OnRecognitionStatusChanged(bool isAvailable)
    {
        _isVoiceAvailable = isAvailable;
        UpdateVoiceStatusUI();
        Debug.Log($"[Voice] Recognition status changed: {isAvailable}");
    }

    private void UpdateVoiceStatusUI()
    {
        if (voiceStatusIndicator != null)
        {
            // Update UI to show voice availability status
            var renderer = voiceStatusIndicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = _isVoiceAvailable ? Color.green : Color.red;
            }
        }
    }

    void ProcessCommand(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd))
            return;
            
        cmd = cmd.ToLowerInvariant().Trim();
        Debug.Log($"[Voice] Recognized: {cmd}");
        
        ExecuteCommand(cmd);
    }

    /// <summary>
    /// Execute a command by name - can be called from voice or UI
    /// </summary>
    public void ExecuteCommand(string command)
    {
        switch (command.ToLowerInvariant().Trim())
        {
            case "save scene":
            case "save":
                sceneHandler.SaveScene();
                break;
            case "scene load":
            case "load":
                sceneHandler.LoadSceneNetworked();
                break;
            case "set qr":
            case "recenter":
                qrManager.RecenterFromQR();
                break;
            case "open settings":
            case "settings":
                settingsUI.Show();
                break;
            case "clear scene":
            case "clear":
                sceneHandler.ClearSceneNetworked();
                break;
            default:
                Debug.Log($"[Voice] Unhandled command: {command}");
                break;
        }
    }

    /// <summary>
    /// Public methods for UI buttons to trigger voice commands
    /// </summary>
    public void SaveScene() => ExecuteCommand("save scene");
    public void LoadScene() => ExecuteCommand("scene load");
    public void RecenterQR() => ExecuteCommand("set qr");
    public void OpenSettings() => ExecuteCommand("open settings");
    public void ClearScene() => ExecuteCommand("clear scene");

    private void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.OnTranscriptReceived -= ProcessCommand;
            recognizer.OnRecognitionStatusChanged -= OnRecognitionStatusChanged;
            recognizer.StopRecognition();
        }
    }
} 