// WhisperVoiceCommandManager.cs
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

/// <summary>
/// Records short audio clips continuously, sends them to OpenAI Whisper,
/// and fires commands based on the returned transcript.
/// </summary>
public class WhisperVoiceCommandManager : MonoBehaviour
{
    [Header("Recording Settings")]
    [Tooltip("Seconds per audio clip")]
    public int     clipLength       = 5;
    [Tooltip("Sample rate for Whisper (max 16kHz)")]
    public int     recordingFreq    = 16000;

    [Header("OpenAI Settings")]
    [Tooltip("Your OpenAI API Key (saved via Settings UI)")]
    public string  openAIKey;

    [Header("Command Hooks")]
    public QRManager          qrManager;
    public SceneDataHandler   sceneHandler;
    public SettingsUIManager  settingsUI;

    private string micDevice;
    private bool   isRecording;

    void Start()
    {
        micDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (string.IsNullOrEmpty(micDevice))
            Debug.LogError("[Whisper] No microphone found!");

        // Load API key from prefs if empty
        if (string.IsNullOrEmpty(openAIKey))
            openAIKey = PlayerPrefs.GetString("OpenAIAPIKey", "");

        StartCoroutine(RecordAndTranscribeLoop());
    }

    IEnumerator RecordAndTranscribeLoop()
    {
        while (true)
        {
            if (!isRecording && !string.IsNullOrEmpty(micDevice) && !string.IsNullOrEmpty(openAIKey))
            {
                isRecording = true;
                var clip = Microphone.Start(micDevice, false, clipLength, recordingFreq);
                yield return new WaitForSeconds(clipLength);

                Microphone.End(micDevice);
                yield return StartCoroutine(UploadClip(clip));
                isRecording = false;
            }
            else
            {
                // wait a bit before retrying
                yield return new WaitForSeconds(1f);
            }
        }
    }

    IEnumerator UploadClip(AudioClip clip)
    {
        // Convert AudioClip to WAV byte[]
        var wavData = WavUtility.FromAudioClip(clip, out string tmpPath, true);

        // Prepare form
        var form = new WWWForm();
        form.AddField("model", "whisper-1");
        form.AddBinaryData("file", wavData, "voice.wav", "audio/wav");

        using var www = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", form);
        www.SetRequestHeader("Authorization", $"Bearer {openAIKey}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Whisper] Error: {www.error}");
            yield break;
        }

        // Parse response JSON
        var json = www.downloadHandler.text;
        var obj  = JObject.Parse(json);
        var text = obj["text"]?.ToString().Trim().ToLower();

        if (!string.IsNullOrEmpty(text))
            ProcessCommand(text);
    }

    void ProcessCommand(string cmd)
    {
        Debug.Log($"[Whisper] Recognized: {cmd}");

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
                Debug.Log($"[Whisper] Unhandled command: {cmd}");
                break;
        }
    }
}
