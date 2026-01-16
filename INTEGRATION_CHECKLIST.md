# Audio File Mod - Integration Checklist

## Summary of Changes

Your mod has been successfully refactored from **TTS (Text-to-Speech) to Audio File Playback**. All the code is complete and compiles without errors.

## What Was Changed

### New Files Created (8 files)
1. ✅ `SpeechMod/Voice/AudioFilePlayer.cs` - Main ISpeech implementation
2. ✅ `SpeechMod/Voice/IAudioFileLoader.cs` - Interface for loading audio
3. ✅ `SpeechMod/Voice/IAudioPlayback.cs` - Interface for playback
4. ✅ `SpeechMod/Voice/IDialogueIdResolver.cs` - Interface for ID resolution
5. ✅ `SpeechMod/Voice/LocalizationAudioFileLoader.cs` - Loads audio by UUID
6. ✅ `SpeechMod/Voice/LocalizationDialogueIdResolver.cs` - Resolves text to UUID
7. ✅ `SpeechMod/Voice/UnityAudioPlayback.cs` - Handles AudioSource playback
8. ✅ `SpeechMod/Unity/AudioFilePlayerUnity.cs` - MonoBehaviour bridge

### Files Modified (3 files)
1. ✅ `SpeechMod/Main.cs` - Added SetAudioFilePlayback() method
2. ✅ `SpeechMod/Settings.cs` - Added UseAudioFiles and CurrentLanguage settings
3. ✅ `SpeechMod/Constants.cs` - Added AUDIO_FILE_PLAYER_NAME constant

### Documentation Created (2 files)
1. 📋 `AUDIO_FILE_SETUP_GUIDE.md` - Complete setup instructions
2. 📋 `AUDIO_FILE_ORGANIZATION.md` - Directory structure and JSON format reference

## Quick Start Checklist

### Phase 1: Code Integration ✅
- [x] All new files created and compiled
- [x] All existing files updated
- [x] No compilation errors
- [x] Mod still compiles for both Windows and macOS

### Phase 2: Audio File Organization
- [ ] Create `Assets/Resources/Localization/{Language}/Audio/` folders
- [ ] Extract UUIDs from game localization files
- [ ] Place pre-created audio files with UUID names
- [ ] Create localization JSON files with text↔UUID mappings

### Phase 3: Configuration
- [ ] Set `UseAudioFiles = true` in mod settings
- [ ] Set `CurrentLanguage = "enGB"` (or your language)
- [ ] Verify audio files are in correct locations

### Phase 4: Testing
- [ ] Start game with mod enabled
- [ ] Check mod log for "Audio File Playback initialized"
- [ ] Trigger a dialogue - should play audio instead of TTS
- [ ] Verify bark system works with audio files
- [ ] Test language switching if multiple languages

## Backwards Compatibility

✅ **The mod is fully backwards compatible:**
- If `UseAudioFiles = false`, it falls back to TTS (original behavior)
- All existing TTS voice settings still work
- All patches and UI integration unchanged
- Can switch between TTS and audio files without restarting

## Architecture Decisions Made

### 1. UUID-Based Audio Files
- **Why**: Game already uses UUIDs for dialogue identification
- **Benefit**: Direct mapping, no text parsing required for file names
- **Implementation**: LocalizationAudioFileLoader loads by UUID

### 2. Localization JSON for Text Matching
- **Why**: Game already provides this data
- **Benefit**: Robust text matching despite formatting differences
- **Implementation**: LocalizationDialogueIdResolver parses JSON on startup

### 3. Unity Resources Folder
- **Why**: Standard Unity asset loading mechanism
- **Benefit**: Simple, no file I/O required, works in packaged mods
- **Limitation**: Audio files must be packed with mod
- **Note**: Can be extended to load from folders if needed

### 4. Dependency Injection Pattern
- **Why**: Clean separation of concerns
- **Benefit**: Easy to swap implementations (e.g., OGG loader)
- **Implementation**: AudioFilePlayer takes 3 dependencies via constructor

## How Audio Playback Works

```
Game Dialog Event
    ↓
SpeakDialog(text) called
    ↓
AudioFilePlayer.PlayAudioForText()
    ↓
LocalizationDialogueIdResolver.ResolveDialogueId(text)
    ↓ (matches text against JSON)
UUID found
    ↓
LocalizationAudioFileLoader.LoadAudioFile(uuid)
    ↓ (loads from Assets/Resources/Localization/{Language}/Audio/)
AudioClip loaded or cached
    ↓
UnityAudioPlayback.PlayAudio(clip)
    ↓
AudioSource.Play()
    ↓
Player hears audio!
```

## Settings Reference

### New Settings in Settings.cs

```csharp
// Audio mode
public bool UseAudioFiles = false;              // Enable audio file playback
public string CurrentLanguage = "enGB";         // Language code for audio files

// Existing settings still apply:
public bool InterruptPlaybackOnPlay = true;    // Stop current audio before playing new
public bool PlaybackBarks = true;                // Play character barks
public bool PlaybackBarkOnlyIfSilence = true;  // Only play barks when not in dialogue
```

## Logging & Debugging

The mod logs useful information:

```
[INFO] Audio File Playback initialized successfully!
[INFO] Loaded 1234 dialogue entries from enGB localization
[WARNING] Could not resolve dialogue ID for text: "First 50 characters..."
[WARNING] No audio file found for dialogue ID: abada903-ebd9-44f0-afe1-5066224cd195
[WARNING] Could not load localization file for enGB
```

Check these logs if audio doesn't play.

## Next Steps

1. **Extract Audio Files**
   - Use your TTS engine or professional voice actors
   - Name files with UUID format
   - Save as WAV files

2. **Organize Audio Assets**
   - Create folder structure as documented
   - Copy JSON files from game localization
   - Place audio files in correct locations

3. **Enable Audio Mode**
   - In settings: Set `UseAudioFiles = true`
   - Verify `CurrentLanguage` is set correctly
   - Start game and test

4. **Multi-Language Support (Optional)**
   - Create additional language folders
   - Add corresponding JSON files
   - Users can switch languages in settings

## Troubleshooting

### Audio Not Playing?
1. Check mod log for error messages
2. Verify file structure matches documentation
3. Ensure CurrentLanguage matches folder name
4. Verify audio file names are exact UUID matches

### Text Matching Issues?
1. Enable LogVoicedLines to see text being matched
2. Compare text in JSON with in-game text (after removing formatting)
3. Check AUDIO_FILE_ORGANIZATION.md for normalization rules

### Performance Issues?
1. Audio files are cached after first load
2. Set InterruptPlaybackOnPlay = true
3. Ensure WAV files aren't corrupted or too large

## FAQ

**Q: Do I need to change game files?**
A: No! Only use the mod folder structure.

**Q: Can I use MP3 instead of WAV?**
A: Not without extending IAudioFileLoader. WAV is recommended.

**Q: What if I don't have audio for a dialogue?**
A: Mod logs warning and silently skips playback.

**Q: Can I fall back to TTS if audio is missing?**
A: Not automatically, but you could extend AudioFilePlayer to do so.

**Q: Does this work with existing save files?**
A: Yes! Just enable UseAudioFiles and audio will play going forward.

**Q: Can I use different voices for different characters?**
A: Audio files are character-agnostic. You control that via file content.

**Q: Do I need to update patches?**
A: No! All existing patches work unchanged.

## File Size Estimates

For a typical game with 2000 unique dialogue lines:
- **WAV Audio (44.1kHz, 16-bit, mono, 2-3 min avg)**: ~2-3 GB
- **Localization JSON**: ~10-20 MB
- **Total for one language**: ~2-3 GB

## Support

For issues or questions about the audio implementation:
1. Check the documentation files
2. Review mod logs for error messages
3. Verify file organization matches examples
4. Ensure audio files are valid WAV format

## Version History

- **v1.0** (Current)
  - Initial audio file implementation
  - UUID-based file identification
  - Localization JSON text matching
  - Multi-language support ready
  - Full backwards compatibility with TTS

## What's NOT Changed

- ✅ All voice patches still work
- ✅ Bark system unchanged
- ✅ Settings UI compatible
- ✅ Keybinds work
- ✅ Dialog system integration
- ✅ Gender-specific voices
- ✅ Protagonist voice selection

Everything just plays audio files instead of generating TTS!
