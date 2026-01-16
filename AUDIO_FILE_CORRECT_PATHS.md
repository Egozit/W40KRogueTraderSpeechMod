# Audio File Mod - Corrected Installation Instructions

## Correct Folder Paths

Based on the mod's README installation instructions, here are the **exact correct paths**:

### Mod Installation Folder
```
%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\
```

Expands to:
```
C:\Users\{YourUsername}\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\
```

### Inside Your Mod Folder - Folder Structure

Create this structure inside your W40KSpeechMod folder:

```
W40KSpeechMod/
├── W40KRTSpeechMod.dll
├── Info.json
├── PhoneticDictionary.json
└── Localization/
    ├── enGB.json              ← Copy from game
    ├── ruRU.json              ← Copy from game
    ├── deDE.json              ← Copy from game (optional)
    └── Audio/
        ├── enGB/
        │   ├── abada903-ebd9-44f0-afe1-5066224cd195.wav
        │   ├── 12345678-1234-1234-1234-123456789abc.wav
        │   └── ... (more audio files)
        ├── ruRU/
        │   ├── abada903-ebd9-44f0-afe1-5066224cd195.wav
        │   └── ... (more audio files)
        └── deDE/
            └── ... (optional)
```

## Step-by-Step Setup

### Step 1: Create Audio Folder Structure

In your mod folder, create:
```
Localization\Audio\enGB\
Localization\Audio\ruRU\
Localization\Audio\deDE\  (optional)
```

Full paths:
```
%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\Audio\enGB\
%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\Audio\ruRU\
```

### Step 2: Get UUIDs from Game's Localization Files

The game stores localization at:
```
C:\Program Files (x86)\Steam\steamapps\common\Warhammer 40,000 Rogue Trader\WH40KRT_Data\StreamingAssets\Localization\
```

Files in that folder:
- `enGB.json` - English dialogues with UUIDs
- `ruRU.json` - Russian dialogues with UUIDs
- `deDE.json` - German dialogues with UUIDs
- etc.

### Step 3: Copy JSON to Your Mod Folder

Copy the JSON files from game to your mod:

**From:**
```
C:\Program Files (x86)\Steam\steamapps\common\Warhammer 40,000 Rogue Trader\WH40KRT_Data\StreamingAssets\Localization\enGB.json
C:\Program Files (x86)\Steam\steamapps\common\Warhammer 40,000 Rogue Trader\WH40KRT_Data\StreamingAssets\Localization\ruRU.json
```

**To:**
```
%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\enGB.json
%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\ruRU.json
```

### Step 4: Extract UUIDs and Create Audio Files

For each dialogue you want to voice:

1. **Open the game's localization JSON** (e.g., `ruRU.json`)
2. **Find the UUID** for the text you want to voice:
   ```json
   "abada903-ebd9-44f0-afe1-5066224cd195": {
     "Offset": 0,
     "Text": "Your dialogue text here..."
   }
   ```
3. **Create an audio file** with that UUID as the filename:
   ```
   abada903-ebd9-44f0-afe1-5066224cd195.wav
   ```
4. **Place it** in the correct language folder:
   ```
   %userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\Audio\ruRU\abada903-ebd9-44f0-afe1-5066224cd195.wav
   ```

### Step 5: Enable in Game

1. Launch Warhammer 40K: Rogue Trader
2. Press **Ctrl+F10** to open Mod Manager
3. Find "W40KSpeechMod" and open its settings
4. Set:
   - **UseAudioFiles** = `true`
   - **CurrentLanguage** = `ruRU` (or `enGB` for English)
5. Restart game

## How the Mod Works

1. **Mod loads game's localization JSON** from `WH40KRT_Data\StreamingAssets\Localization\{Language}.json`
2. **Reads UUID→Text mappings** to understand what text corresponds to which UUID
3. **When dialogue plays**, it finds the UUID for that text
4. **Loads audio file** from your mod folder: `Localization\Audio\{Language}\{UUID}.wav`
5. **Plays the audio** instead of generating TTS

## File Organization Reference

### Game Localization Files (Reference - READ ONLY)
```
C:\Program Files (x86)\Steam\steamapps\common\Warhammer 40,000 Rogue Trader\WH40KRT_Data\StreamingAssets\Localization\
├── enGB.json
├── ruRU.json
├── deDE.json
└── ... (other languages)
```

### Your Mod Folder (Where You Put Audio Files)
```
%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\
├── W40KRTSpeechMod.dll
├── Info.json
├── PhoneticDictionary.json
└── Localization\
    ├── enGB.json              (COPY from game)
    ├── ruRU.json              (COPY from game)
    └── Audio\
        ├── enGB\
        │   ├── {uuid}.wav
        │   ├── {uuid}.wav
        │   └── ...
        ├── ruRU\
        │   ├── {uuid}.wav
        │   └── ...
        └── deDE\
            └── ...
```

## Important Notes

- ✅ **Copy** JSON files from game to mod folder (mod reads them at runtime)
- ✅ **UUIDs** come from the JSON keys in the game's localization files
- ✅ **Audio files** go in your mod's `Localization/Audio/{Language}/` folder
- ✅ **File names** must match the UUID exactly: `abada903-ebd9-44f0-afe1-5066224cd195.wav`
- ❌ **Don't edit** the game's localization files
- ❌ **Don't** try to use `Assets/Resources/` paths (mod loads from mod folder at runtime)

## Verification Checklist

- [ ] Created `Localization\Audio\enGB\` folder in mod directory
- [ ] Created `Localization\Audio\ruRU\` folder in mod directory
- [ ] Copied `enGB.json` from game to `ModFolder\Localization\`
- [ ] Copied `ruRU.json` from game to `ModFolder\Localization\`
- [ ] Created audio files with UUID names (e.g., `abada903-ebd9-44f0-afe1-5066224cd195.wav`)
- [ ] Placed audio files in correct language subfolder
- [ ] Enabled `UseAudioFiles = true` in mod settings
- [ ] Set correct `CurrentLanguage` in mod settings
