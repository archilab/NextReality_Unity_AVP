using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Provides fallback input methods for voice commands when speech recognition is unavailable.
/// Supports keyboard shortcuts, UI buttons, and gesture-based commands.
/// </summary>
public class FallbackCommandManager : MonoBehaviour
{
    [Header("Dependencies")]
    public VoiceCommandManager voiceCommandManager;
    public SettingsUIManager settingsUI;
    
    [Header("Keyboard Shortcuts")]
    public bool enableKeyboardShortcuts = true;
    public KeyCode saveSceneKey = KeyCode.F1;
    public KeyCode loadSceneKey = KeyCode.F2;
    public KeyCode clearSceneKey = KeyCode.F3;
    public KeyCode recenterKey = KeyCode.F4;
    public KeyCode settingsKey = KeyCode.F5;
    
    [Header("Status")]
    public bool isVoiceAvailable = false;
    public bool isFallbackActive = false;
    
    void Start()
    {
        if (voiceCommandManager != null)
        {
            // Check if voice recognition is available
            var speechRecognizer = voiceCommandManager.GetComponent<AppleSpeechRecognizer>();
            isVoiceAvailable = speechRecognizer?.IsRecognitionAvailable ?? false;
            isFallbackActive = !isVoiceAvailable;
            
            Debug.Log($"[Fallback] Voice available: {isVoiceAvailable}, Fallback active: {isFallbackActive}");
        }
    }
    
    void Update()
    {
        if (!enableKeyboardShortcuts) return;
        
        // Keyboard shortcuts for voice commands
        if (Input.GetKeyDown(saveSceneKey))
        {
            ExecuteCommand("save scene");
        }
        else if (Input.GetKeyDown(loadSceneKey))
        {
            ExecuteCommand("scene load");
        }
        else if (Input.GetKeyDown(clearSceneKey))
        {
            ExecuteCommand("clear scene");
        }
        else if (Input.GetKeyDown(recenterKey))
        {
            ExecuteCommand("set qr");
        }
        else if (Input.GetKeyDown(settingsKey))
        {
            ExecuteCommand("open settings");
        }
    }
    
    /// <summary>
    /// Execute a command through the voice command manager or directly
    /// </summary>
    public void ExecuteCommand(string command)
    {
        if (voiceCommandManager != null)
        {
            voiceCommandManager.ExecuteCommand(command);
        }
        else
        {
            // Direct execution if voice command manager is not available
            Debug.Log($"[Fallback] Executing command: {command}");
            // Add direct command execution logic here if needed
        }
    }
    
    /// <summary>
    /// Public methods for UI buttons
    /// </summary>
    public void SaveScene() => ExecuteCommand("save scene");
    public void LoadScene() => ExecuteCommand("scene load");
    public void ClearScene() => ExecuteCommand("clear scene");
    public void RecenterQR() => ExecuteCommand("set qr");
    public void OpenSettings() => ExecuteCommand("open settings");
    
    /// <summary>
    /// Get the current status of voice recognition and fallback
    /// </summary>
    public (bool voiceAvailable, bool fallbackActive) GetStatus()
    {
        return (isVoiceAvailable, isFallbackActive);
    }
    
    /// <summary>
    /// Show help information about available commands
    /// </summary>
    public void ShowHelp()
    {
        string helpText = isVoiceAvailable 
            ? "Voice commands available. You can also use keyboard shortcuts:\n" +
              $"F1: Save Scene\nF2: Load Scene\nF3: Clear Scene\nF4: Recenter QR\nF5: Open Settings"
            : "Voice recognition not available. Using keyboard shortcuts:\n" +
              $"F1: Save Scene\nF2: Load Scene\nF3: Clear Scene\nF4: Recenter QR\nF5: Open Settings";
        
        Debug.Log($"[Fallback] {helpText}");
    }
} 