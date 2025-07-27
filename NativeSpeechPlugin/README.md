# NativeSpeechPlugin for Unity (Apple Speech Framework)

This plugin enables real-time speech-to-text (voice command) support in Unity apps for Apple Vision Pro, iOS, and macOS using Apple's on-device Speech Framework.

## Files
- `SpeechRecognizerPlugin.swift`: Swift source for the speech recognizer plugin
- `SpeechRecognizerPlugin-Bridging-Header.h`: Objective-C bridging header for Unity/Swift interop

## Integration Steps

1. **Add the Plugin to Your Xcode Project**
   - Copy both files to your Unity project's Plugins/iOS or directly add them to your Xcode project after building from Unity.
   - Ensure the bridging header is set in your Xcode build settings if needed.

2. **Info.plist Changes**
   Add the following keys to your app's `Info.plist`:
   ```xml
   <key>NSSpeechRecognitionUsageDescription</key>
   <string>This app uses speech recognition for voice commands.</string>
   <key>NSMicrophoneUsageDescription</key>
   <string>This app needs microphone access for voice commands.</string>
   ```

3. **Unity C# Integration**
   - Use a C# wrapper (see `AppleSpeechRecognizer.cs` in your Unity project) to call `startRecognition` and `stopRecognition`.
   - Subscribe to transcript events to receive recognized speech in Unity.

4. **Editor Fallback**
   - In the Unity Editor, a debug input box is shown to simulate speech commands.

## Example Usage in Unity

```csharp
public class VoiceCommandManager : MonoBehaviour
{
    void Start()
    {
        AppleSpeechRecognizer.Instance.OnTranscriptReceived += OnTranscript;
        AppleSpeechRecognizer.Instance.StartRecognition();
    }
    void OnTranscript(string transcript)
    {
        Debug.Log("Recognized: " + transcript);
        // Map transcript to commands here
    }
}
```

## Notes
- This plugin only works on Apple platforms (VisionOS, iOS, macOS).
- For Editor testing, use the built-in fallback input.
- Make sure microphone and speech permissions are granted on device. 