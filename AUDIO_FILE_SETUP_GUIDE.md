# Audio File Speech Mod - Setup Guide

## Overview
Your mod has been successfully converted from TTS (Text-to-Speech) to **audio file playback**. The system now:
- Loads pre-created audio files by dialogue ID (UUID)
- Maps dialogue text back to IDs using localization JSON files
- Plays audio files using Unity's AudioSource instead of generating speech

## Architecture

### Core Components Created

1. **AudioFilePlayer.cs** - Main ISpeech implementation
   - Resolves dialogue IDs from text
   - Loads corresponding audio files
   - Handles playback through the IAudioPlayback interface

2. **LocalizationAudioFileLoader.cs** - Audio asset loader
   - Loads audio files from Resources by UUID
   - Caches loaded audio clips
   - Tracks load success/failure

3. **LocalizationDialogueIdResolver.cs** - Dialogue ID resolver
   - Parses localization JSON files to extract text↔ID mappings
   - Normalizes text for matching (removes formatting tags)
   - Provides fallback logging for missing audio files

4. **UnityAudioPlayback.cs** - Audio playback controller
   - Manages AudioSource playback
   - Supports delayed playback with coroutines
   - Respects InterruptPlaybackOnPlay setting

5. **AudioFilePlayerUnity.cs** - MonoBehaviour bridge
   - Creates and manages AudioSource component
   - Marked as DontDestroyOnLoad for persistence

### Interfaces

- **IAudioFileLoader** - Load audio by dialogue ID
- **IAudioPlayback** - Play/stop audio clips
- **IDialogueIdResolver** - Map text to dialogue IDs

## Setup Instructions

### 1. Create Audio Asset Folder Structure

Create this folder structure in your Unity project:

```
Assets/
├── Resources/
│   └── Localization/
│       ├── enGB/
│       │   └── Audio/
│       │       ├── abada903-ebd9-44f0-afe1-5066224cd195.wav
│       │       ├── 12345678-1234-1234-1234-123456789abc.wav
│       │       └── (more audio files...)
│       ├── ruRU/
│       │   └── Audio/
│       │       └── (Russian audio files...)
│       └── (other languages...)
```

**Important:** Audio files MUST be named exactly as the UUID from the localization JSON (without the .wav extension in the ID part of the JSON key).

### 2. Prepare Localization JSON Files

Ensure your localization JSON files follow this format:

```json
{
  "abada903-ebd9-44f0-afe1-5066224cd195.wav": {
    "Offset": 0,
    "Text": "Потрясенный Абеляр делает несколько шагов вперед..."
  },
  "12345678-1234-1234-1234-123456789abc.wav": {
    "Offset": 0,
    "Text": "Another dialogue line..."
  }
}
```

The JSON files should be placed at:
```
Assets/Resources/Localization/{LanguageCode}.json
```

Examples: `enGB.json`, `ruRU.json`, `deDE.json`, etc.

### 3. Enable Audio File Mode in Settings

In the mod settings, enable:
- **UseAudioFiles**: `true`
- **CurrentLanguage**: `"enGB"` (or your desired language)

### 4. Audio File Requirements

Your WAV files should have these specifications:
- **Format**: WAV (PCM, preferably)
- **Sample Rate**: 44100 Hz or higher
- **Channels**: Mono or Stereo
- **Bit Depth**: 16-bit or higher
- **Name**: Exact UUID from localization JSON

## How It Works

### Dialogue Playback Flow

1. Game calls `Main.Speech.SpeakDialog(text, delay)`
2. `AudioFilePlayer.PlayAudioForText()` is invoked
3. `LocalizationDialogueIdResolver` searches localization data for matching text
4. `LocalizationAudioFileLoader` loads the audio file by UUID from Resources
5. `UnityAudioPlayback` plays the AudioClip through AudioSource

### Text Matching

The dialogue resolver:
- Removes all HTML/color formatting tags
- Removes newlines and extra whitespace
- Normalizes the text for fuzzy matching
- Falls back to logging if no match is found

**Note:** If your text in code differs from localization JSON (due to formatting), the system will log a warning with the first 50 characters to help you debug.

## Troubleshooting

### Audio files not playing?

1. **Check the log**: Look for warnings about unresolved dialogue IDs
2. **Verify file placement**: Ensure audio files are in `Assets/Resources/Localization/{Language}/Audio/`
3. **Verify JSON format**: Use the exact format shown above
4. **Check audio file names**: Names must match the UUID exactly (without .wav)
5. **Verify CurrentLanguage setting**: Must match the folder name

### Text matching issues?

The resolver normalizes text by removing:
- Color tags: `<color=#...>`, `</color>`
- Formatting tags: `<i>`, `<b>`, etc.
- Narrator color markers: `<color=#3c2d0a>`
- Extra whitespace

If text still doesn't match, check your localization JSON for the exact text string.

### Performance Issues?

- Audio files are cached after first load
- Set `InterruptPlaybackOnPlay = true` to stop previous audio before playing new
- Ensure audio files aren't too large (< 10MB recommended)

## Settings

New settings added to Settings.cs:

```csharp
public bool UseAudioFiles = false;        // Enable audio file playback
public string CurrentLanguage = "enGB";   // Language for audio files
```

## Fallback to TTS

If audio files aren't found:
1. Set `UseAudioFiles = false` in settings
2. Mod will automatically fall back to TTS (Windows/Apple voices)
3. No code changes needed

## Switching Languages

To support multiple languages:

1. Create folder: `Assets/Resources/Localization/{LanguageCode}/Audio/`
2. Place audio files with correct UUIDs
3. Ensure localization JSON exists: `Assets/Resources/Localization/{LanguageCode}.json`
4. Change `CurrentLanguage` setting
5. Mod will automatically reload audio files for new language

## Future Enhancements

Consider implementing:
- Dynamic loading from mod folders (not just Resources)
- Audio file format conversion (MP3, OGG support)
- Per-speaker audio directory organization
- Audio length caching for UI preview timing
- Volume adjustment per voice type
- Fallback to TTS if audio file missing

## Migration Notes

### What Changed?

- **Removed**: Dependency on TTS engines (Windows SAPI, macOS say command)
- **Added**: Audio file loading and playback system
- **Kept**: All patches, UI integration, bark system
- **Kept**: Settings and configuration UI compatibility

### What Stayed the Same?

- All voice patches still work (but map to audio files instead of TTS)
- Bark playback still respects gender
- Dialog playback still respects gender-specific voices
- Settings UI remains compatible
- Keybinds (Stop, Toggle) still work

## Files Created

1. `SpeechMod/Voice/AudioFilePlayer.cs` - Main player
2. `SpeechMod/Voice/IAudioFileLoader.cs` - Loader interface
3. `SpeechMod/Voice/IAudioPlayback.cs` - Playback interface
4. `SpeechMod/Voice/IDialogueIdResolver.cs` - Resolver interface
5. `SpeechMod/Voice/LocalizationAudioFileLoader.cs` - Loader implementation
6. `SpeechMod/Voice/LocalizationDialogueIdResolver.cs` - Resolver implementation
7. `SpeechMod/Voice/UnityAudioPlayback.cs` - Playback implementation
8. `SpeechMod/Unity/AudioFilePlayerUnity.cs` - MonoBehaviour bridge

## Files Modified

1. `SpeechMod/Main.cs` - Added SetAudioFilePlayback() method
2. `SpeechMod/Settings.cs` - Added UseAudioFiles and CurrentLanguage settings
3. `SpeechMod/Constants.cs` - Added AUDIO_FILE_PLAYER_NAME constant
