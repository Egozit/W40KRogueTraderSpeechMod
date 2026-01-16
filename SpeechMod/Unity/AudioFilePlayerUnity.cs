using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

namespace SpeechMod.Unity;

/// <summary>
/// MonoBehaviour that manages audio file playback for the speech mod.
/// This replaces WindowsVoiceUnity and AppleVoiceUnity for audio file mode.
/// </summary>
public class AudioFilePlayerUnity : MonoBehaviour
{
    private AudioSource _audioSource;
    private AudioClip _currentClip;
    private bool _isLoading = false;
    private AudioClip _loadedClip;

    private void Start()
    {
        // Ensure this GameObject is never destroyed
        DontDestroyOnLoad(gameObject);

        // Get or create AudioSource
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource for speech playback
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _audioSource.spatialBlend = 0f; // 2D audio (not positional)
        _audioSource.volume = 1f;

        Debug.Log("AudioFilePlayerUnity initialized!");
    }

    public AudioSource GetAudioSource()
    {
        return _audioSource;
    }

    /// <summary>
    /// Loads an audio file asynchronously and plays it.
    /// Returns true if loading started successfully.
    /// </summary>
    public bool LoadAndPlayAudioFile(string filePath)
    {
        if (_isLoading)
        {
            Main.Logger?.Warning("[AudioFilePlayer] Already loading an audio file, ignoring request");
            return false;
        }

        StartCoroutine(LoadAudioClipCoroutine(filePath));
        return true;
    }

    private IEnumerator LoadAudioClipCoroutine(string filePath)
    {
        _isLoading = true;
        _loadedClip = null;

        // Convert path to file:// URL with proper encoding for spaces and special characters
        string normalizedPath = filePath.Replace("\\", "/");
        string encodedPath = Uri.EscapeDataString(normalizedPath);
        // Uri.EscapeDataString also encodes slashes, so we need to fix that
        encodedPath = encodedPath.Replace("%2F", "/");
        var fileUrl = "file:///" + encodedPath;
        
        Main.Logger?.Log($"[AudioFilePlayer] Starting coroutine load from: {fileUrl}");
        Main.FileLog($"[AudioFilePlayer] Starting coroutine load from: {fileUrl}");

        using (var request = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.WAV))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                var msgError = $"[AudioFilePlayer] Failed to load audio: {request.error}";
                Main.Logger?.Error(msgError);
                Main.FileLog(msgError);
                _isLoading = false;
                yield break;
            }

            _loadedClip = DownloadHandlerAudioClip.GetContent(request);

            if (_loadedClip == null)
            {
                var msgNull = "[AudioFilePlayer] Loaded clip is null";
                Main.Logger?.Error(msgNull);
                Main.FileLog(msgNull);
                _isLoading = false;
                yield break;
            }

            // Wait for audio clip to be fully decoded
            yield return new WaitForSeconds(0.1f);

            var msgSuccess = $"[AudioFilePlayer] Audio clip loaded: {_loadedClip.name}, Length: {_loadedClip.length}s, Channels: {_loadedClip.channels}, Frequency: {_loadedClip.frequency}Hz";
            Main.Logger?.Log(msgSuccess);
            Main.FileLog(msgSuccess);

            // Play the audio
            _audioSource.clip = _loadedClip;
            _audioSource.Play();

            var msgPlay = $"[AudioFilePlayer] Playing audio clip, duration: {_loadedClip.length}s";
            Main.Logger?.Log(msgPlay);
            Main.FileLog(msgPlay);
        }

        _isLoading = false;
    }
}
