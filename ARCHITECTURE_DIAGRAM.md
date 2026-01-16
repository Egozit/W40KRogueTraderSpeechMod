# Audio File Mod - Architecture Diagram

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     Warhammer 40K: Rogue Trader Game            │
└────────────────────────┬────────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
    Dialog Patch    Bark Patch    Encyclopedia Patch
    (+ 30 more)        (+ more)          (+ more)
        │                │                │
        └────────────────┼────────────────┘
                         │
                    Main.Speech
                (ISpeech interface)
                         │
        ┌────────────────┴────────────────┐
        │                                 │
   [TTS MODE]                    [AUDIO FILE MODE]
   (Original)                     (New)
        │                                 │
    WindowsSpeech              AudioFilePlayer
    / AppleSpeech                  │
        │                          ├─ Dialogue ID Resolver
        │                          │  └─ Parses localization JSON
        │                          │     └─ Matches text → UUID
        │                          │
        │                          ├─ Audio File Loader
        │                          │  └─ Resources.Load<AudioClip>()
        │                          │     └─ Localization/{Lang}/Audio/
        │                          │
        │                          └─ Audio Playback
        │                             └─ UnityAudioPlayback
        │                                └─ AudioSource.Play()
        │
        ├─ WindowsVoiceUnity
        │  └─ WindowsVoice.dll (SAPI)
        │
        └─ AppleVoiceUnity
           └─ /usr/bin/say command
```

## Component Interaction Diagram

```
                    Game Event
                        │
                        ▼
            ┌───────────────────────┐
            │   Main.Speech.Speak   │
            │   /SpeakDialog/       │
            │   /SpeakAs()          │
            └───────────┬───────────┘
                        │
              [UseAudioFiles?]
              /            \
            YES             NO
            │               │
            ▼               ▼
     AudioFilePlayer   WindowsSpeech
            │           AppleSpeech
            │
            ├─────────────────────────┬─────────────────────────┐
            │                         │                         │
            ▼                         ▼                         ▼
    IDialogueIdResolver     IAudioFileLoader         IAudioPlayback
            │                         │                         │
    Local..Resolver          Local..Loader           UnityAudioPlayback
            │                         │                         │
       JSON Parser            Resources.Load         AudioSource
            │                    (Assets/)             │
       Text→UUID               UUID→Clip        PlayAudio()
            │                     │                    │
            └─────────────────────┼────────────────────┘
                                  │
                            ┌──────▼──────┐
                            │ Audio File  │
                            │  Playing    │
                            └─────────────┘
```

## Data Flow: Text to Audio Playback

```
INPUT: Dialogue Text
  │
  │ "Потрясенный Абеляр делает несколько шагов..."
  │
  ▼
LocalizationDialogueIdResolver.ResolveDialogueId()
  │
  ├─ Normalize text:
  │   ├─ Remove <color>#...> tags
  │   ├─ Remove </color> tags
  │   ├─ Remove <b>, <i>, etc.
  │   ├─ Collapse whitespace
  │   └─ Trim
  │
  ├─ Load localization JSON (once per language)
  │   └─ Parse: "uuid.wav": { "Text": "..." }
  │
  └─ Find matching text
      │
      └─→ UUID: "abada903-ebd9-44f0-afe1-5066224cd195"
          │
          ▼
LocalizationAudioFileLoader.LoadAudioFile(uuid)
          │
          ├─ Check cache
          │ ├─ HIT? → Return cached AudioClip
          │ └─ MISS? → Continue
          │
          ├─ Build resource path:
          │   Localization/{Language}/Audio/{uuid}
          │
          ├─ Resources.Load<AudioClip>()
          │
          └─→ AudioClip
              │
              ▼
UnityAudioPlayback.PlayAudio(clip, delay)
              │
              ├─ If delay > 0: StartCoroutine()
              │                └─ Wait(delay)
              │
              └─ AudioSource.Play()
                 │
                 ▼
            [SOUND!] 🔊
```

## Class Dependencies

```
AudioFilePlayer (ISpeech)
    │
    ├─ depends on ─→ IAudioFileLoader
    │                     ▲
    │                     │ implements
    │                     │
    │              LocalizationAudioFileLoader
    │                     │
    │                     └─ uses ─→ Resources.Load
    │
    ├─ depends on ─→ IAudioPlayback
    │                     ▲
    │                     │ implements
    │                     │
    │              UnityAudioPlayback
    │                     │
    │                     └─ uses ─→ AudioSource
    │
    └─ depends on ─→ IDialogueIdResolver
                         ▲
                         │ implements
                         │
                  LocalizationDialogueIdResolver
                         │
                         └─ loads ─→ JSON file
                         └─ uses ─→ Regex
```

## Folder Structure

```
Game Project
│
├─ Assets/
│  │
│  └─ Resources/
│     │
│     └─ Localization/
│        │
│        ├─ enGB.json          (Language JSON)
│        ├─ ruRU.json
│        ├─ deDE.json
│        │
│        ├─ enGB/
│        │  └─ Audio/
│        │     ├─ abada903-....wav      UUID-named audio files
│        │     ├─ 12345678-....wav
│        │     └─ ...
│        │
│        ├─ ruRU/
│        │  └─ Audio/
│        │     ├─ abada903-....wav
│        │     └─ ...
│        │
│        └─ deDE/
│           └─ Audio/
│              └─ ...
│
└─ Warhammer 40,000 Rogue Trader (Game Install)
   │
   └─ WH40KRT_Data/StreamingAssets/
      │
      └─ Localization/
         ├─ ruRU.json        (Source for text extraction)
         ├─ enGB.json
         └─ ...
```

## Sequence Diagram: Playing a Dialogue

```
Player       Game        Patch       Main       AudioPlayer    Loader    Resolver
│            │           │           │          │              │         │
│            │ Dialogue  │           │          │              │         │
│◄───────────┤ Triggered │           │          │              │         │
│            │           │           │          │              │         │
│            │           │ Call      │          │              │         │
│            │           │ SpeakDlg()│          │              │         │
│            │           ├──────────►│          │              │         │
│            │           │           │ Decide  │              │         │
│            │           │           │ Mode?   │              │         │
│            │           │           │         │              │         │
│            │           │           │ Audio?  │              │         │
│            │           │           ├────────►│              │         │
│            │           │           │         │ Resolve ID   │         │
│            │           │           │         ├─────────────►│         │
│            │           │           │         │              │ Normalize
│            │           │           │         │              │ + Parse JSON
│            │           │           │         │◄─────────────┤◄────────┤
│            │           │           │         │ UUID         │         │
│            │           │           │         │              │         │
│            │           │           │         │ Load AudioClip          │
│            │           │           │         ├─────────────────────────┤
│            │           │           │         │ (from Resources)        │
│            │           │         Play()      │                       ◄─┤
│            │           │         ◄───────────┤                         │
│            │           │         Audio ready │                         │
│            │ ◄─────────┤◄────────┤           │                         │
│◄───────────┤ Resume   │ Return  │           │                         │
│            │ Game     │         │           │                         │
│            │          │         │           │                         │
│  🔊 Plays Audio (via AudioSource)           │                         │
│            │          │         │           │                         │
│ (continues playing)   │         │           │                         │
│            │          │         │           │                         │
```

## Settings & Configuration Flow

```
┌─────────────────────────────────┐
│ User Opens Mod Settings         │
├─────────────────────────────────┤
│ UseAudioFiles = [true/false]    │
│ CurrentLanguage = [enGB/ruRU]   │
│ InterruptPlaybackOnPlay = true  │
│ + All existing TTS settings     │
└────────┬────────────────────────┘
         │
         ▼ On Mod Load
   ┌─────────────────┐
   │ Main.Load()     │
   └────────┬────────┘
            │
    ┌───────┴─────────┐
    │                 │
    ▼                 ▼
UseAudioFiles?   (else TTS)
  YES │
      ├─ SetAudioFilePlayback()
      │  ├─ Create LocalizationAudioFileLoader
      │  ├─ Create LocalizationDialogueIdResolver
      │  ├─ Create UnityAudioPlayback
      │  └─ Create AudioFilePlayer
      │
      └─► Main.Speech = AudioFilePlayer instance
```

## Error Handling Flow

```
PlayAudio("Some text")
    │
    ├─ ResolveDialogueId()
    │   ├─ JSON Parse OK?
    │   │  └─ NO ──► LOG: "Error parsing localization"
    │   │             RETURN: null
    │   │
    │   └─ Text Match Found?
    │      └─ NO ──► LOG: "Could not resolve dialogue ID"
    │                RETURN: null
    │
    ├─ LoadAudioFile(uuid)
    │   ├─ In Cache?
    │   │  └─ YES ──► Return cached clip
    │   │
    │   ├─ File Exists?
    │   │  └─ NO ──► LOG: "No audio file found for {uuid}"
    │   │             Return: null
    │   │
    │   └─ Load Successful?
    │      └─ NO ──► LOG: Error details
    │                Return: null
    │
    └─ PlayAudio(clip)
       └─ clip == null?
          └─ YES ──► LOG: "AudioClip is null"
                     RETURN (do nothing)
       
       └─ NO ──► AudioSource.Play()
                 [SUCCESS] 🔊
```

This shows the complete system design for converting your mod from TTS to audio file playback.
