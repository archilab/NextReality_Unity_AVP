using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AppleSpeechRecognizer : MonoBehaviour
{
#if UNITY_IOS || UNITY_VISIONOS || UNITY_STANDALONE_OSX
    [DllImport("__Internal")]
    private static extern void startRecognition(IntPtr callback);

    [DllImport("__Internal")]
    private static extern void stopRecognition();
#endif

    public Action<string> OnTranscriptReceived;
    public Action<bool> OnRecognitionStatusChanged; // New event for status changes
    
    private static AppleSpeechRecognizer _instance;
    public static AppleSpeechRecognizer Instance => _instance;

    private delegate void TranscriptCallback(string transcript);
    private static TranscriptCallback _callbackDelegate;
    private static GCHandle _callbackHandle;

    // Status tracking
    private bool _isRecognitionAvailable = false;
    private bool _isRecognitionActive = false;
    public bool IsRecognitionAvailable => _isRecognitionAvailable;
    public bool IsRecognitionActive => _isRecognitionActive;

    // Editor fallback
#if UNITY_EDITOR
    private string editorInput = "";
    private bool showEditorInput = false;
#endif

    private void Awake()
    {
        if (_instance == null) _instance = this;
        CheckRecognitionAvailability();
    }

    private void CheckRecognitionAvailability()
    {
#if UNITY_IOS || UNITY_VISIONOS || UNITY_STANDALONE_OSX
        // Check if we're on a supported platform
        _isRecognitionAvailable = true;
#elif UNITY_EDITOR
        // Always available in editor with fallback
        _isRecognitionAvailable = true;
#else
        // Not available on other platforms
        _isRecognitionAvailable = false;
#endif
        
        OnRecognitionStatusChanged?.Invoke(_isRecognitionAvailable);
        Debug.Log($"[Speech] Recognition available: {_isRecognitionAvailable}");
    }

    public void StartRecognition()
    {
        if (!_isRecognitionAvailable)
        {
            Debug.LogWarning("[Speech] Recognition not available on this platform");
            return;
        }

        try
        {
#if UNITY_IOS || UNITY_VISIONOS || UNITY_STANDALONE_OSX
            _callbackDelegate = OnTranscript;
            _callbackHandle = GCHandle.Alloc(_callbackDelegate);
            startRecognition(Marshal.GetFunctionPointerForDelegate(_callbackDelegate));
            _isRecognitionActive = true;
#elif UNITY_EDITOR
            showEditorInput = true;
            _isRecognitionActive = true;
#endif
            Debug.Log("[Speech] Recognition started");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Speech] Failed to start recognition: {e.Message}");
            _isRecognitionActive = false;
        }
    }

    public void StopRecognition()
    {
        try
        {
#if UNITY_IOS || UNITY_VISIONOS || UNITY_STANDALONE_OSX
            stopRecognition();
            if (_callbackHandle.IsAllocated) _callbackHandle.Free();
#elif UNITY_EDITOR
            showEditorInput = false;
#endif
            _isRecognitionActive = false;
            Debug.Log("[Speech] Recognition stopped");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Speech] Failed to stop recognition: {e.Message}");
        }
    }

    [AOT.MonoPInvokeCallback(typeof(TranscriptCallback))]
    private static void OnTranscript(string transcript)
    {
        if (_instance != null && _instance.OnTranscriptReceived != null)
            _instance.OnTranscriptReceived.Invoke(transcript);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (showEditorInput)
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 100), GUI.skin.box);
            GUILayout.Label("Speech Recognition (Editor Fallback):");
            GUILayout.Label("Type a command and press Enter:");
            GUI.SetNextControlName("SpeechInputField");
            editorInput = GUILayout.TextField(editorInput, 128);
            GUI.FocusControl("SpeechInputField");
            Event e = Event.current;
            if ((e.isKey && e.keyCode == KeyCode.Return) || GUILayout.Button("Send"))
            {
                if (!string.IsNullOrWhiteSpace(editorInput))
                {
                    OnTranscript(editorInput);
                    editorInput = "";
                }
            }
            GUILayout.EndArea();
        }
    }
#endif
} 