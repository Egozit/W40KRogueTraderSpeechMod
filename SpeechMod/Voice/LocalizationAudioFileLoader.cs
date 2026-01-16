using UnityEngine;
using UnityEngine.Networking;
using SpeechMod.Unity.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SpeechMod.Voice;

/// <summary>
/// Loads audio files from the mod folder's Localization directory.
/// Audio files are organized as: Localization/{language}/Audio/{uuid}.wav
/// </summary>
public class LocalizationAudioFileLoader : IAudioFileLoader
{
    private readonly string _language;
    private readonly string _modFolderPath;
    private readonly Dictionary<string, AudioClip> _audioCache = new();
    private int _loadedCount = 0;
    private int _failedCount = 0;

    /// <summary>
    /// Creates a loader for a specific language.
    /// </summary>
    /// <param name="language">Language code (e.g., "enGB", "ruRU")</param>
    public LocalizationAudioFileLoader(string language)
    {
        _language = language;
        // Application.persistentDataPath = AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader
        // We need: AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod
        _modFolderPath = Path.Combine(Application.persistentDataPath, "UnityModManager", "W40KSpeechMod");
        
        var msg1 = $"Audio Loader initialized for language: {_language}";
        var msg2 = $"Mod folder path: {_modFolderPath}";
        var msg3 = $"Audio folder path: {Path.Combine(_modFolderPath, "Localization", "Audio", _language)}";
        
        Main.Logger?.Log(msg1);
        Main.Logger?.Log(msg2);
        Main.Logger?.Log(msg3);
    }

    public AudioClip LoadAudioFile(string dialogueId)
    {
        if (string.IsNullOrEmpty(dialogueId))
        {
            var msgNull = "DialogueId is null or empty";
            Main.Logger?.Warning(msgNull);
            Main.FileLog(msgNull);
            return null;
        }

        // Check cache first
        if (_audioCache.TryGetValue(dialogueId, out var cachedClip))
        {
            var msgCache = $"[AudioLoad] Cache hit for: {dialogueId}";
            Main.Logger?.Log(msgCache);
            Main.FileLog(msgCache);
            return cachedClip;
        }

        // Build file path: ModFolder/Localization/Audio/{language}/{uuid}.wav
        var audioFilePath = Path.Combine(_modFolderPath, "Localization", "Audio", _language, $"{dialogueId}.wav");
        
        var msg1 = $"[AudioLoad] Attempting to load: {dialogueId}";
        var msg2 = $"[AudioLoad] Full path: {audioFilePath}";
        var msg3 = $"[AudioLoad] File exists: {File.Exists(audioFilePath)}";
        
        Main.Logger?.Log(msg1);
        Main.Logger?.Log(msg2);
        Main.Logger?.Log(msg3);
        Main.FileLog(msg1);
        Main.FileLog(msg2);
        Main.FileLog(msg3);

        if (!File.Exists(audioFilePath))
        {
            _failedCount++;
            var msgNotFound = $"[AudioLoad] File NOT found: {audioFilePath}";
            Main.Logger?.Warning(msgNotFound);
            Main.FileLog(msgNotFound);
            return null;
        }

        // Load audio file using WWW (works with file:// URLs)
        try
        {
            var msgLoading = $"[AudioLoad] Loading audio file from: {audioFilePath}";
            Main.Logger?.Log(msgLoading);
            Main.FileLog(msgLoading);
            
            var clip = LoadAudioFromFile(audioFilePath);
            if (clip != null)
            {
                _audioCache[dialogueId] = clip;
                _loadedCount++;
                var msgCached = $"[AudioLoad] Clip cached for future use: {dialogueId}";
                Main.FileLog(msgCached);
                return clip;
            }
        }
        catch (Exception ex)
        {
            var msgError = $"Failed to load audio file {audioFilePath}: {ex.Message}";
            Main.Logger?.Error(msgError);
            Main.FileLog(msgError);
            Main.FileLog($"Exception details: {ex}");
        }

        _failedCount++;
        return null;
    }

    /// <summary>
    /// Loads an AudioClip from a file path using UnityWebRequest via coroutine.
    /// Note: This method is now deprecated. Use AudioFilePlayer.LoadAndPlayAudioFile instead.
    /// This is kept for compatibility but will return null immediately.
    /// </summary>
    private AudioClip LoadAudioFromFile(string filePath)
    {
        try
        {
            // Check file info
            var fileInfo = new System.IO.FileInfo(filePath);
            var msgSize = $"[AudioLoad] File size: {fileInfo.Length} bytes";
            Main.Logger?.Log(msgSize);
            Main.FileLog(msgSize);
            
            if (fileInfo.Length == 0)
            {
                var msgEmpty = $"[AudioLoad] File is empty (0 bytes): {filePath}";
                Main.Logger?.Error(msgEmpty);
                Main.FileLog(msgEmpty);
                return null;
            }

            // Loading is now done asynchronously through AudioFilePlayer.LoadAndPlayAudioFile
            var msgAsync = $"[AudioLoad] File will be loaded asynchronously by AudioFilePlayer";
            Main.Logger?.Log(msgAsync);
            Main.FileLog(msgAsync);
            
            // Return a placeholder clip to indicate we'll load it asynchronously
            // The actual clip loading happens in AudioFilePlayerUnity
            return AudioClip.Create("placeholder", 1, 1, 44100, false);
        }
        catch (System.Exception ex)
        {
            var msgException = $"[AudioLoad] Exception: {ex.Message}";
            Main.Logger?.Error(msgException);
            Main.FileLog(msgException);
            Main.FileLog($"[AudioLoad] Full exception: {ex}");
            return null;
        }
    }

    public string GetLoadStatus()
    {
        return $"Language: {_language}, Loaded: {_loadedCount}, Failed: {_failedCount}, ModPath: {_modFolderPath}";
    }
}
