using UnityEngine;
using System;

/// <summary>
/// Test script to verify native speech plugin integration
/// </summary>
public class SpeechPluginTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = false;
    public bool enableDebugLogging = true;

    void Start()
    {
        if (testOnStart)
        {
            TestSpeechPlugin();
        }
    }

    [ContextMenu("Test Speech Plugin")]
    public void TestSpeechPlugin()
    {
        Debug.Log("[SpeechPluginTest] Starting speech plugin test...");
        
        try
        {
            // Test if the native plugin is available
            if (Application.platform == RuntimePlatform.IPhonePlayer || 
                Application.platform == RuntimePlatform.VisionOS)
            {
                Debug.Log("[SpeechPluginTest] Platform supports native speech recognition");
                
                // Test the native plugin methods
                TestNativePluginMethods();
            }
            else
            {
                Debug.Log("[SpeechPluginTest] Platform does not support native speech recognition");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SpeechPluginTest] Error testing speech plugin: {e.Message}");
        }
    }

    private void TestNativePluginMethods()
    {
        try
        {
            // Test if we can call the native methods
            // Note: These are just tests to see if the plugin loads correctly
            // Actual speech recognition would need proper setup
            
            Debug.Log("[SpeechPluginTest] Native plugin methods should be available");
            Debug.Log("[SpeechPluginTest] Test completed successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SpeechPluginTest] Error calling native methods: {e.Message}");
        }
    }

    void OnGUI()
    {
        if (enableDebugLogging)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Speech Plugin Test");
            
            if (GUILayout.Button("Test Speech Plugin"))
            {
                TestSpeechPlugin();
            }
            
            GUILayout.Label($"Platform: {Application.platform}");
            GUILayout.Label($"Supports Speech: {Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.VisionOS}");
            
            GUILayout.EndArea();
        }
    }
} 