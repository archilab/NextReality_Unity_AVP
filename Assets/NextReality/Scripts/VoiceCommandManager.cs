using UnityEngine;

public class VoiceCommandManager : MonoBehaviour
{
    public QRManager qrManager;
    public SceneDataHandler sceneHandler;
    public SettingsUIManager settingsUI;
    private AppleSpeechRecognizer recognizer;

    void Start()
    {
        recognizer = AppleSpeechRecognizer.Instance;
        if (recognizer == null)
        {
            recognizer = gameObject.AddComponent<AppleSpeechRecognizer>();
        }
        recognizer.OnTranscriptReceived += ProcessCommand;
        recognizer.StartRecognition();
    }

    void ProcessCommand(string cmd)
    {
        cmd = cmd.ToLowerInvariant().Trim();
        Debug.Log($"[Voice] Recognized: {cmd}");
        switch (cmd)
        {
            case "save scene":
                sceneHandler.SaveScene();
                break;
            case "scene load":
                sceneHandler.LoadSceneNetworked();
                break;
            case "set qr":
                qrManager.RecenterFromQR();
                break;
            case "open settings":
                settingsUI.Show();
                break;
            default:
                Debug.Log($"[Voice] Unhandled command: {cmd}");
                break;
        }
    }

    private void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.OnTranscriptReceived -= ProcessCommand;
            recognizer.StopRecognition();
        }
    }
} 