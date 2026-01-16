# Audio File Organization Reference

## Directory Structure

**Installation Path (from README):**
```
%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\
```

**Inside the mod folder:**
```
W40KSpeechMod/
├── W40KRTSpeechMod.dll
├── Info.json
├── PhoneticDictionary.json
└── Localization/
    ├── enGB.json              (copy from game)
    ├── ruRU.json              (copy from game)
    ├── deDE.json              (copy from game)
    └── Audio/
        ├── enGB/
        │   ├── abada903-ebd9-44f0-afe1-5066224cd195.wav
        │   └── ...
        ├── ruRU/
        │   ├── abada903-ebd9-44f0-afe1-5066224cd195.wav
        │   └── ...
        └── deDE/
            └── ...
```

**Game's Localization Files (reference):**
```
C:\Program Files (x86)\Steam\steamapps\common\Warhammer 40,000 Rogue Trader\WH40KRT_Data\StreamingAssets\Localization\
```

Copy the JSON files from game to your mod folder's `Localization/` directory.

## Localization JSON File Format

**File**: `Assets/Resources/Localization/enGB.json`

```json
{
  "abada903-ebd9-44f0-afe1-5066224cd195.wav": {
    "Offset": 0,
    "Text": "Потрясенный Абеляр делает несколько шагов вперед, после чего тяжело опирается о край стола. Взгляд офицера прикован к бездыханному телу. \"Лорд-капитан... да кто же посмел... неужели... крыса Войгтвир...\""
  },
  "12345678-1234-1234-1234-123456789abc.wav": {
    "Offset": 0,
    "Text": "Another dialogue line..."
  }
}
```

## Key Points

### 1. UUID Naming Convention
- Audio files use UUID format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- **Example**: `abada903-ebd9-44f0-afe1-5066224cd195.wav`
- Case-sensitive: matches exactly as in JSON

### 2. Language Folders
Create separate language folders for each supported language:
- `enGB/` - English (British)
- `ruRU/` - Russian
- `deDE/` - German
- `frFR/` - French
- `esES/` - Spanish
- etc.

Each language folder must have:
- An `Audio/` subdirectory with WAV files
- A corresponding `{LanguageCode}.json` in parent directory

### 3. Localization JSON Structure
- **Key**: UUID with `.wav` extension (e.g., `"abada903-ebd9-44f0-afe1-5066224cd195.wav"`)
- **Offset**: Byte offset in the JSON (can be 0 for simplicity)
- **Text**: Exact dialogue text that appears in-game

### 4. Text Matching Process
The resolver will:
1. Remove HTML tags: `<color=#...>`, `</color>`, `<b>`, `<i>`, etc.
2. Remove narrator markers: `<color=#3c2d0a>` (NARRATOR_COLOR_CODE)
3. Normalize whitespace (collapse multiple spaces)
4. Trim leading/trailing whitespace
5. Find matching entry in JSON

### 5. Audio File Specifications
- **Format**: WAV (Waveform Audio File Format)
- **Codec**: PCM (uncompressed) recommended
- **Sample Rate**: 44100 Hz, 48000 Hz, or 96000 Hz
- **Channels**: Mono or Stereo
- **Bit Depth**: 16-bit or 24-bit
- **File Size**: Keep under 10MB per file for performance

## Example Setup

### Step 1: Create Folder Structure

In your mod folder:
```
Localization/
├── Audio/
│   ├── enGB/
│   ├── ruRU/
│   └── deDE/
```

On your system:
```
C:\Users\{Username}\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KRTSpeechMod\Localization\Audio\enGB\
```

### Step 2: Place Audio Files

Copy your pre-created audio files to the Audio/Language folders:
```
Localization/Audio/enGB/abada903-ebd9-44f0-afe1-5066224cd195.wav
Localization/Audio/enGB/12345678-1234-1234-1234-123456789abc.wav
```

### Step 3: Create Localization JSON

Copy the JSON from game files or create your own:
```
Localization/enGB.json
Localization/ruRU.json
```

File contents:
```json
{
  "abada903-ebd9-44f0-afe1-5066224cd195.wav": {
    "Offset": 0,
    "Text": "This is the dialogue text for this UUID"
  },
  "12345678-1234-1234-1234-123456789abc.wav": {
    "Offset": 0,
    "Text": "Another line of dialogue"
  }
}
```

### Step 4: Enable in Mod Settings
In-game settings:
- Set `UseAudioFiles` = true
- Set `CurrentLanguage` = "enGB"

## UUID Extraction From Game Files

The game's localization files contain the UUID→text mappings:

**Game Localization Path:**
```
C:\Program Files (x86)\Steam\steamapps\common\Warhammer 40,000 Rogue Trader\WH40KRT_Data\StreamingAssets\Localization\ruRU.json
C:\Program Files (x86)\Steam\steamapps\common\Warhammer 40,000 Rogue Trader\WH40KRT_Data\StreamingAssets\Localization\enGB.json
```

**How the mod works:**

1. **Mod automatically reads** the game's localization JSON at runtime
2. **Extracts UUID→text mappings** (e.g., `"abada903-ebd9-44f0-afe1-5066224cd195": { "Text": "..." }`)
3. **When dialogue plays**, matches the text to find the UUID
4. **Loads the audio file** from mod folder: `Localization/Audio/{language}/{uuid}.wav`

**Example from game file:**
```json
{
  "abada903-ebd9-44f0-afe1-5066224cd195": {
    "Offset": 0,
    "Text": "Потрясенный Абеляр делает несколько шагов..."
  },
  "12345678-1234-1234-1234-123456789abc": {
    "Offset": 0,
    "Text": "Another dialogue line..."
  }
}
```

So for each dialogue you want to voice:
- **Find** the UUID in game's localization file (e.g., `abada903-ebd9-44f0-afe1-5066224cd195`)
- **Create** audio file with that UUID as filename: `abada903-ebd9-44f0-afe1-5066224cd195.wav`
- **Place** in mod folder: `Localization/Audio/ruRU/abada903-ebd9-44f0-afe1-5066224cd195.wav`

## Switching Languages at Runtime

The mod automatically:
1. Loads the localization JSON for `CurrentLanguage`
2. Looks for audio files in `Localization/{Language}/Audio/`
3. Reloads when language setting changes

No application restart needed!

## Troubleshooting Checklist

- [ ] Audio file named exactly as UUID (case-sensitive)
- [ ] Audio file in correct folder: `Assets/Resources/Localization/{Language}/Audio/`
- [ ] JSON file placed at: `Assets/Resources/Localization/{Language}.json`
- [ ] JSON key includes `.wav` extension
- [ ] Text in JSON matches in-game text (after removing formatting)
- [ ] Audio file is valid WAV format
- [ ] `UseAudioFiles` setting enabled
- [ ] `CurrentLanguage` setting matches folder name
- [ ] Check mod log for specific missing audio file messages

## Performance Optimization

### Caching
- Audio files are cached after first load
- Subsequent plays use cached AudioClip (no disk I/O)
- Cache is cleared only when language changes

### Memory Management
- Disable unused languages to save memory
- Consider using compressed formats (requires loader update)
- Profile audio memory usage with many dialogues

## Future Support

The current implementation supports:
- ✅ WAV files via Unity Resources.Load
- ❌ MP3/OGG (would need additional codec support)
- ❌ Dynamic folder loading (would need file I/O)
- ❌ Streaming playback (loads entire file)

These can be added by extending `IAudioFileLoader`.
