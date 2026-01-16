# Audio File Mod - Developer's Guide

## For Advanced Users & Developers

This guide explains how to extend and customize the audio file system.

---

## Architecture Overview

### Core Interfaces

```csharp
// Interface for loading audio files
public interface IAudioFileLoader
{
    AudioClip LoadAudioFile(string dialogueId);
    string GetLoadStatus();
}

// Interface for playing audio
public interface IAudioPlayback
{
    bool IsPlaying();
    void PlayAudio(AudioClip clip, float delay = 0f);
    void Stop();
}

// Interface for resolving dialogue ID
public interface IDialogueIdResolver
{
    string ResolveDialogueId(string text);
    void RegisterDialogue(string id, string text);
}
```

### Main Implementation

```csharp
public class AudioFilePlayer : ISpeech
{
    private readonly IAudioFileLoader _audioFileLoader;
    private readonly IAudioPlayback _audioPlayback;
    private readonly IDialogueIdResolver _dialogueIdResolver;

    public AudioFilePlayer(
        IAudioFileLoader audioFileLoader,
        IAudioPlayback audioPlayback,
        IDialogueIdResolver dialogueIdResolver)
    {
        // Constructor injection for dependency management
    }
}
```

---

## Extending the System

### 1. Add MP3 Support

Create a custom loader:

```csharp
public class AudioFileLoaderWithMP3 : IAudioFileLoader
{
    private readonly string _language;

    public AudioClip LoadAudioFile(string dialogueId)
    {
        // Try to load as MP3 first
        var clip = LoadMP3(dialogueId);
        if (clip != null) return clip;

        // Fallback to WAV
        return LoadWAV(dialogueId);
    }

    private AudioClip LoadMP3(string dialogueId)
    {
        // Use NLayer or NAudio library
        var path = $"Localization/{_language}/Audio/{dialogueId}.mp3";
        // ... MP3 loading logic
    }

    private AudioClip LoadWAV(string dialogueId)
    {
        var path = $"Localization/{_language}/Audio/{dialogueId}";
        return Resources.Load<AudioClip>(path);
    }

    public string GetLoadStatus() => "MP3 + WAV";
}
```

### 2. Add Streaming Support

For large audio files:

```csharp
public class StreamingAudioPlayback : IAudioPlayback
{
    private AudioSource _audioSource;

    public void PlayAudio(AudioClip clip, float delay = 0f)
    {
        // Stream instead of loading entire file
        // Use WWW or UnityWebRequest for streaming
        if (clip.length > 60f) // > 1 minute
        {
            StreamAudio(clip.name, delay);
        }
        else
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    private void StreamAudio(string clipPath, float delay)
    {
        // Use UnityWebRequest to stream
        // StartCoroutine(StreamAudioCoroutine(clipPath, delay));
    }
}
```

### 3. Add Fuzzy Text Matching

For better dialogue matching:

```csharp
public class FuzzyDialogueIdResolver : IDialogueIdResolver
{
    private const float FUZZY_MATCH_THRESHOLD = 0.85f;

    public string ResolveDialogueId(string text)
    {
        // Try exact match first
        if (_textToIdMap.TryGetValue(text, out var id))
            return id;

        // Try fuzzy matching
        var normalized = NormalizeText(text);
        
        foreach (var entry in _textToIdMap)
        {
            var similarity = CalculateSimilarity(normalized, entry.Key);
            if (similarity > FUZZY_MATCH_THRESHOLD)
                return entry.Value;
        }

        return null;
    }

    private float CalculateSimilarity(string a, string b)
    {
        // Implement Levenshtein distance or similar
        // Returns 0-1 where 1 is perfect match
        return LevenshteinDistance(a, b) / (float)Math.Max(a.Length, b.Length);
    }
}
```

### 4. Add Fallback to TTS

For missing audio files:

```csharp
public class HybridAudioFilePlayer : ISpeech
{
    private readonly AudioFilePlayer _audioPlayer;
    private readonly ISpeech _ttsPlayer;
    private readonly IAudioFileLoader _audioLoader;

    public void Speak(string text, float delay = 0f)
    {
        var id = _audioPlayer.ResolveDialogueId(text);
        
        if (id == null || _audioLoader.LoadAudioFile(id) == null)
        {
            // Fallback to TTS
            _ttsPlayer.Speak(text, delay);
        }
        else
        {
            // Use audio file
            _audioPlayer.Speak(text, delay);
        }
    }

    // ... other methods delegate to _audioPlayer
}
```

### 5. Add Per-Character Audio

Organize audio by speaker:

```csharp
public class PerCharacterAudioLoader : IAudioFileLoader
{
    private readonly string _language;
    private string _currentCharacter = "Narrator";

    public AudioClip LoadAudioFile(string dialogueId)
    {
        // Try character-specific path first
        var charPath = $"Localization/{_language}/Audio/{_currentCharacter}/{dialogueId}";
        var clip = Resources.Load<AudioClip>(charPath);
        
        if (clip != null) return clip;

        // Fallback to generic path
        var genericPath = $"Localization/{_language}/Audio/{dialogueId}";
        return Resources.Load<AudioClip>(genericPath);
    }

    public void SetCurrentCharacter(string character)
    {
        _currentCharacter = character;
    }
}
```

### 6. Add Volume Control per Voice Type

```csharp
public class VolumeControlledAudioPlayback : IAudioPlayback
{
    private AudioSource _audioSource;
    private Dictionary<VoiceType, float> _volumeMap = new();

    public void PlayAudio(AudioClip clip, float delay = 0f)
    {
        var voiceType = DetermineVoiceType(clip.name);
        var volume = _volumeMap.ContainsKey(voiceType) 
            ? _volumeMap[voiceType] 
            : 1.0f;

        _audioSource.volume = volume;
        _audioSource.PlayOneShot(clip);
    }

    public void SetVolumeForVoiceType(VoiceType type, float volume)
    {
        _volumeMap[type] = Mathf.Clamp01(volume);
    }
}
```

### 7. Add Subtitle Timing

Cache dialogue length for UI:

```csharp
public class TimedAudioFileLoader : IAudioFileLoader
{
    private Dictionary<string, float> _durationCache = new();

    public AudioClip LoadAudioFile(string dialogueId)
    {
        var clip = LoadClipImpl(dialogueId);
        
        if (clip != null)
        {
            _durationCache[dialogueId] = clip.length;
        }

        return clip;
    }

    public float GetDialogueDuration(string dialogueId)
    {
        return _durationCache.ContainsKey(dialogueId) 
            ? _durationCache[dialogueId] 
            : 0f;
    }
}
```

---

## Custom Implementation Example

Here's how to create a custom loader from scratch:

```csharp
public class CustomAudioFileLoader : IAudioFileLoader
{
    private readonly string _basePath;
    private readonly Dictionary<string, AudioClip> _cache = new();
    private readonly Dictionary<string, string> _idToPathMap = new();

    public CustomAudioFileLoader(string basePath, string mapFile)
    {
        _basePath = basePath;
        LoadMappingFile(mapFile);
    }

    private void LoadMappingFile(string mapFile)
    {
        // Load a CSV or JSON file that maps IDs to paths
        var lines = System.IO.File.ReadAllLines(mapFile);
        
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length >= 2)
            {
                _idToPathMap[parts[0]] = parts[1];
            }
        }
    }

    public AudioClip LoadAudioFile(string dialogueId)
    {
        if (_cache.TryGetValue(dialogueId, out var cached))
            return cached;

        if (!_idToPathMap.TryGetValue(dialogueId, out var filePath))
            return null;

        var clip = LoadFromDisk(filePath);
        if (clip != null)
        {
            _cache[dialogueId] = clip;
        }

        return clip;
    }

    private AudioClip LoadFromDisk(string filePath)
    {
        // Load audio from file path
        // Implementation depends on your audio format
        return null;
    }

    public string GetLoadStatus()
    {
        return $"Cached: {_cache.Count}, Total: {_idToPathMap.Count}";
    }
}
```

---

## Integration in Main.cs

Replacing the default loader:

```csharp
private static bool SetAudioFilePlayback()
{
    var language = Settings?.CurrentLanguage ?? "enGB";

    // Use custom loader instead of default
    IAudioFileLoader audioFileLoader = new CustomAudioFileLoader(
        "path/to/audio/files",
        "path/to/id_mapping.csv"
    );

    // Use fuzzy resolver instead of default
    IDialogueIdResolver dialogueResolver = new FuzzyDialogueIdResolver(language);

    // Add streaming support
    var audioPlayback = new StreamingAudioPlayback(audioSource);

    Speech = new AudioFilePlayer(audioFileLoader, audioPlayback, dialogueResolver);

    return true;
}
```

---

## Testing Custom Implementations

```csharp
[TestFixture]
public class CustomAudioLoaderTests
{
    private IAudioFileLoader _loader;

    [SetUp]
    public void Setup()
    {
        _loader = new CustomAudioFileLoader("test/data", "test/data/mapping.csv");
    }

    [Test]
    public void LoadAudioFile_WithValidId_ReturnsClip()
    {
        var clip = _loader.LoadAudioFile("valid-uuid");
        Assert.IsNotNull(clip);
    }

    [Test]
    public void LoadAudioFile_WithInvalidId_ReturnsNull()
    {
        var clip = _loader.LoadAudioFile("invalid-uuid");
        Assert.IsNull(clip);
    }

    [Test]
    public void LoadAudioFile_CachesResults()
    {
        var clip1 = _loader.LoadAudioFile("uuid");
        var clip2 = _loader.LoadAudioFile("uuid");
        Assert.AreSame(clip1, clip2); // Same object from cache
    }
}
```

---

## Performance Optimization Tips

### 1. Async Loading
```csharp
public class AsyncAudioFileLoader : IAudioFileLoader
{
    private Queue<string> _loadQueue = new();

    public AudioClip LoadAudioFile(string dialogueId)
    {
        // Queue async load
        _loadQueue.Enqueue(dialogueId);
        
        // Return cached or loading placeholder
        return _cache.TryGetValue(dialogueId, out var clip) ? clip : null;
    }
}
```

### 2. Memory Pooling
```csharp
public class PooledAudioPlayback : IAudioPlayback
{
    private Stack<AudioSource> _audioSourcePool = new();

    public void PlayAudio(AudioClip clip, float delay = 0f)
    {
        var source = _audioSourcePool.Count > 0 
            ? _audioSourcePool.Pop() 
            : CreateNewAudioSource();

        source.PlayOneShot(clip);
        // Return to pool after playback
    }
}
```

### 3. Preloading Common Files
```csharp
public void PreloadCommonDialogues(string[] frequentDialogueIds)
{
    foreach (var id in frequentDialogueIds)
    {
        _audioFileLoader.LoadAudioFile(id); // Force cache
    }
}
```

---

## Debugging

Enable verbose logging:

```csharp
public class DebugAudioFileLoader : IAudioFileLoader
{
    private IAudioFileLoader _innerLoader;

    public AudioClip LoadAudioFile(string dialogueId)
    {
        Debug.Log($"Loading audio for ID: {dialogueId}");
        var clip = _innerLoader.LoadAudioFile(dialogueId);
        Debug.Log($"Load result: {(clip == null ? "FAILED" : "SUCCESS")}");
        return clip;
    }

    // Decorator pattern for wrapping existing implementation
}
```

---

## Best Practices

1. **Always implement all three interfaces** - Loader, Resolver, Playback
2. **Use dependency injection** - Don't create instances directly
3. **Cache results** - Avoid repeated loads
4. **Log failures** - Help users debug issues
5. **Handle null gracefully** - Don't throw exceptions
6. **Test with real data** - Use actual dialogue files
7. **Monitor performance** - Profile in real game
8. **Document assumptions** - Comments on expected file format

---

## Common Patterns

### Factory Pattern
```csharp
public static class AudioFilePlayerFactory
{
    public static ISpeech Create(AudioMode mode, string language)
    {
        return mode switch
        {
            AudioMode.AudioFiles => CreateAudioFilePlayer(language),
            AudioMode.TTS => CreateTTSPlayer(),
            AudioMode.Hybrid => CreateHybridPlayer(language),
            _ => throw new ArgumentException("Unknown mode")
        };
    }
}
```

### Decorator Pattern
```csharp
public class CachedAudioFileLoader : IAudioFileLoader
{
    private readonly IAudioFileLoader _inner;
    private readonly Dictionary<string, AudioClip> _cache;

    public AudioClip LoadAudioFile(string dialogueId)
    {
        return _cache.TryGetValue(dialogueId, out var clip) 
            ? clip 
            : (_cache[dialogueId] = _inner.LoadAudioFile(dialogueId));
    }
}
```

### Strategy Pattern
```csharp
public class SelectableAudioFileLoader : IAudioFileLoader
{
    private IAudioFileLoader _strategy;

    public void SetStrategy(IAudioFileLoader strategy)
    {
        _strategy = strategy;
    }

    public AudioClip LoadAudioFile(string dialogueId)
    {
        return _strategy.LoadAudioFile(dialogueId);
    }
}
```

---

## Contributing Extensions

To add custom implementations:

1. Create new file in `SpeechMod/Voice/`
2. Implement required interface
3. Add documentation
4. Test with real game data
5. Submit pull request

---

## Resources

- Unity AudioSource docs: https://docs.unity3d.com/ScriptReference/AudioSource.html
- AudioClip format info: https://docs.unity3d.com/Manual/AudioFiles.html
- C# interfaces: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface

---

This system is designed to be extended! Use these patterns and examples to add your own features.
