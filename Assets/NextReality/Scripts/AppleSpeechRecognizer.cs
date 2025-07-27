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
    private static AppleSpeechRecognizer _instance;
    public static AppleSpeechRecognizer Instance => _instance;

    private delegate void TranscriptCallback(string transcript);
    private static TranscriptCallback _callbackDelegate;
    private static GCHandle _callbackHandle;

    // Editor fallback
#if UNITY_EDITOR
    private string editorInput = "";
    private bool showEditorInput = false;
#endif

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    public void StartRecognition()
    {
#if UNITY_IOS || UNITY_VISIONOS || UNITY_STANDALONE_OSX
        _callbackDelegate = OnTranscript;
        _callbackHandle = GCHandle.Alloc(_callbackDelegate);
        startRecognition(Marshal.GetFunctionPointerForDelegate(_callbackDelegate));
#elif UNITY_EDITOR
        showEditorInput = true;
#endif
    }

    public void StopRecognition()
    {
#if UNITY_IOS || UNITY_VISIONOS || UNITY_STANDALONE_OSX
        stopRecognition();
        if (_callbackHandle.IsAllocated) _callbackHandle.Free();
#elif UNITY_EDITOR
        showEditorInput = false;
#endif
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
            GUILayout.BeginArea(new Rect(10, 10, 400, 80), GUI.skin.box);
            GUILayout.Label("Type a command and press Enter (Editor Speech Fallback):");
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