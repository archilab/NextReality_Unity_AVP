# Fallback Guide - Voice Commands ohne Spracherkennung

## Übersicht

Das NextReality-Projekt bietet mehrere Fallback-Optionen für den Fall, dass die Spracherkennung nicht verfügbar ist oder fehlschlägt. Alle Sprachbefehle können auch über alternative Eingabemethoden ausgeführt werden.

## Verfügbare Fallback-Methoden

### 1. **Tastatur-Shortcuts (Editor & Standalone)**
- **F1**: Save Scene
- **F2**: Load Scene  
- **F3**: Clear Scene
- **F4**: Recenter QR
- **F5**: Open Settings

### 2. **UI-Buttons im Settings Panel**
Das Settings UI wurde erweitert um Voice Command Buttons:
- Save Scene Button
- Load Scene Button
- Clear Scene Button
- Recenter QR Button

### 3. **Editor-Fallback (Unity Editor)**
Im Unity Editor wird automatisch ein Textfeld angezeigt, in das Sprachbefehle eingegeben werden können.

### 4. **Status-Anzeige**
- **Grüner Indikator**: Spracherkennung verfügbar
- **Roter Indikator**: Spracherkennung nicht verfügbar, Fallback aktiv

## Implementierte Scripts

### AppleSpeechRecognizer.cs
- **Status-Tracking**: `IsRecognitionAvailable`, `IsRecognitionActive`
- **Fehlerbehandlung**: Try-catch für native Aufrufe
- **Editor-Fallback**: Textfeld für manuelle Eingabe
- **Events**: `OnRecognitionStatusChanged` für Status-Updates

### VoiceCommandManager.cs
- **Robuste Initialisierung**: Fehlerbehandlung beim Start
- **Status-Tracking**: `_isVoiceAvailable`
- **UI-Integration**: `voiceStatusIndicator`
- **Public Methods**: Alle Befehle als öffentliche Methoden für UI-Buttons

### SettingsUIManager.cs
- **Voice Command Buttons**: Zusätzliche Buttons für alle Sprachbefehle
- **Status-Anzeige**: Voice-Status im Settings Panel
- **Fallback-Integration**: Alle Voice-Commands über UI verfügbar

### FallbackCommandManager.cs
- **Tastatur-Shortcuts**: F1-F5 für alle Hauptbefehle
- **Status-Management**: Tracking von Voice/Fallback-Status
- **Help-System**: `ShowHelp()` für verfügbare Befehle

## Konfiguration

### Inspector-Einstellungen
1. **VoiceCommandManager**:
   - `voiceStatusIndicator`: Optional GameObject für Status-Anzeige
   - Alle Dependencies (QRManager, SceneDataHandler, SettingsUIManager)

2. **SettingsUIManager**:
   - Voice Command Buttons zuweisen
   - `voiceStatusIndicator` und `voiceStatusText` zuweisen
   - `voiceCommandManager` Referenz setzen

3. **FallbackCommandManager**:
   - `enableKeyboardShortcuts`: Tastatur-Shortcuts aktivieren/deaktivieren
   - Key-Codes anpassen falls nötig

## Verwendung

### Automatischer Fallback
Das System erkennt automatisch, ob Spracherkennung verfügbar ist:
```csharp
// Automatische Erkennung
bool isAvailable = AppleSpeechRecognizer.Instance.IsRecognitionAvailable;
```

### Manuelle Befehlsausführung
Alle Befehle können programmatisch ausgeführt werden:
```csharp
// Über VoiceCommandManager
voiceCommandManager.ExecuteCommand("save scene");

// Über FallbackCommandManager  
fallbackManager.ExecuteCommand("save scene");

// Direkte Methoden
voiceCommandManager.SaveScene();
fallbackManager.SaveScene();
```

### Status-Abfrage
```csharp
var (voiceAvailable, fallbackActive) = fallbackManager.GetStatus();
Debug.Log($"Voice: {voiceAvailable}, Fallback: {fallbackActive}");
```

## Fehlerbehandlung

### Spracherkennung nicht verfügbar
- Automatischer Fallback auf Tastatur/UI
- Status-Anzeige wird rot
- Debug-Logs zeigen den Status

### Native Plugin Fehler
- Try-catch in AppleSpeechRecognizer
- Graceful degradation zu Fallback-Methoden
- Detaillierte Error-Logs

### UI-Fallback
- Alle Voice-Commands als UI-Buttons verfügbar
- Settings Panel zeigt Voice-Status
- Buttons funktionieren auch ohne Spracherkennung

## Best Practices

1. **Immer Fallback testen**: Teste das System ohne Mikrofon/Spracherkennung
2. **UI-Buttons zuweisen**: Alle Voice Command Buttons im Inspector setzen
3. **Status überwachen**: Logs und UI-Indikatoren für Voice-Status nutzen
4. **Fehlerbehandlung**: Try-catch für alle Voice-bezogenen Aufrufe
5. **Dokumentation**: Help-System für verfügbare Befehle nutzen

## Troubleshooting

### Spracherkennung funktioniert nicht
1. Prüfe Platform-Support (iOS/VisionOS/macOS)
2. Prüfe Mikrofon-Berechtigungen
3. Nutze Fallback-Methoden (Tastatur/UI)

### UI-Buttons funktionieren nicht
1. Prüfe Inspector-Zuweisungen
2. Prüfe VoiceCommandManager Referenz
3. Prüfe Event-Listener Verbindungen

### Tastatur-Shortcuts funktionieren nicht
1. Prüfe `enableKeyboardShortcuts` Flag
2. Prüfe Key-Code Zuweisungen
3. Prüfe Input System Integration 