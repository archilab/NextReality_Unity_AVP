import Foundation
import Speech
import AVFoundation

@objc public class SpeechRecognizerPlugin: NSObject, SFSpeechRecognizerDelegate {
    private let speechRecognizer = SFSpeechRecognizer(locale: Locale(identifier: "en-US"))
    private var recognitionRequest: SFSpeechAudioBufferRecognitionRequest?
    private var recognitionTask: SFSpeechRecognitionTask?
    private let audioEngine = AVAudioEngine()
    private var unityCallback: ((String) -> Void)?

    @objc public static let shared = SpeechRecognizerPlugin()

    @objc public func startRecognition(_ callback: @escaping (String) -> Void) {
        self.unityCallback = callback
        SFSpeechRecognizer.requestAuthorization { authStatus in
            guard authStatus == .authorized else { return }
            DispatchQueue.main.async {
                self.startSession()
            }
        }
    }

    private func startSession() {
        recognitionRequest = SFSpeechAudioBufferRecognitionRequest()
        let inputNode = audioEngine.inputNode
        guard let recognitionRequest = recognitionRequest else { return }

        recognitionRequest.shouldReportPartialResults = true

        recognitionTask = speechRecognizer?.recognitionTask(with: recognitionRequest) { result, error in
            if let result = result {
                let transcript = result.bestTranscription.formattedString
                self.unityCallback?(transcript)
            }
            if error != nil || (result?.isFinal ?? false) {
                self.audioEngine.stop()
                inputNode.removeTap(onBus: 0)
                self.recognitionRequest = nil
                self.recognitionTask = nil
            }
        }

        let recordingFormat = inputNode.outputFormat(forBus: 0)
        inputNode.installTap(onBus: 0, bufferSize: 1024, format: recordingFormat) { buffer, _ in
            self.recognitionRequest?.append(buffer)
        }

        audioEngine.prepare()
        try? audioEngine.start()
    }

    @objc public func stopRecognition() {
        audioEngine.stop()
        recognitionRequest?.endAudio()
    }
}

// C interface for Unity
@_cdecl("startRecognition")
public func startRecognition(callbackPtr: UnsafeMutableRawPointer) {
    let callback: (String) -> Void = { transcript in
        typealias CallbackType = @convention(c) (UnsafePointer<CChar>) -> Void
        let cb = unsafeBitCast(callbackPtr, to: CallbackType.self)
        cb(transcript.cString(using: .utf8)!)
    }
    SpeechRecognizerPlugin.shared.startRecognition(callback)
}

@_cdecl("stopRecognition")
public func stopRecognition() {
    SpeechRecognizerPlugin.shared.stopRecognition()
} 