using System;
using System.Collections.Generic;

namespace SpeechMod.Voice;

/// <summary>
/// Audio file based speech playback, replacing TTS.
/// Plays pre-created audio files mapped by dialogue ID (UUID).
/// </summary>
public class AudioFilePlayer : ISpeech
{
    private readonly IAudioFileLoader _audioFileLoader;
    private readonly IAudioPlayback _audioPlayback;
    private readonly IDialogueIdResolver _dialogueIdResolver;

    public AudioFilePlayer(IAudioFileLoader audioFileLoader, IAudioPlayback audioPlayback, IDialogueIdResolver dialogueIdResolver)
    {
        _audioFileLoader = audioFileLoader ?? throw new ArgumentNullException(nameof(audioFileLoader));
        _audioPlayback = audioPlayback ?? throw new ArgumentNullException(nameof(audioPlayback));
        _dialogueIdResolver = dialogueIdResolver ?? throw new ArgumentNullException(nameof(dialogueIdResolver));
    }

    public string GetStatusMessage()
    {
        var loadStatus = _audioFileLoader.GetLoadStatus();
        return $"AudioFilePlayer ready! {loadStatus}";
    }

    public string[] GetAvailableVoices()
    {
        // Audio file player doesn't use voices - files are pre-selected by content creator
        return new[] { "AudioFile-" + Constants.APPLE_VOICE_NAME, "AudioFile-" + Constants.WINDOWS_VOICE_NAME };
    }

    public bool IsSpeaking()
    {
        return _audioPlayback.IsPlaying();
    }

    public void SpeakPreview(string text, VoiceType voiceType)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to play!");
            return;
        }

        // Preview might not have an ID, so we log a warning
        Main.Logger?.Warning("Preview playback not supported in AudioFilePlayer mode - no dialogue ID available");
    }

    public void SpeakDialog(string text, float delay = 0f, string dialogueId = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to play!");
            return;
        }

        PlayAudioForDialogueId(text, VoiceType.Narrator, delay, dialogueId);
    }

    public void SpeakAs(string text, VoiceType type, float delay = 0f, string dialogueId = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to play!");
            return;
        }

        PlayAudioForDialogueId(text, type, delay, dialogueId);
    }

    public void Speak(string text, float delay = 0f, string dialogueId = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to play!");
            return;
        }

        PlayAudioForDialogueId(text, VoiceType.Narrator, delay, dialogueId);
    }

    /// <summary>
    /// Plays audio for a dialogue ID (UUID).
    /// If dialogueId is provided, uses it directly. Otherwise falls back to text matching.
    /// </summary>
    private void PlayAudioForDialogueId(string text, VoiceType voiceType, float delay, string dialogueId)
    {
        Main.Logger?.Log($"[AudioFilePlayer] PlayAudioForDialogueId called - dialogueId: {dialogueId}");
        
        // If we have a dialogue ID, use it directly (preferred method)
        if (!string.IsNullOrEmpty(dialogueId))
        {
            Main.Logger?.Log($"[AudioFilePlayer] Attempting to load audio for ID: {dialogueId}");
            var audioClip = _audioFileLoader.LoadAudioFile(dialogueId);
            if (audioClip != null)
            {
                Main.Logger?.Log($"[AudioFilePlayer] Successfully loaded audio clip, playing now");
                _audioPlayback.PlayAudio(audioClip, delay);
                return;
            }
            
            Main.Logger?.Warning($"[AudioFilePlayer] No audio file found for dialogue ID: {dialogueId}");
            return;
        }

        Main.Logger?.Log($"[AudioFilePlayer] No dialogue ID provided, falling back to text matching");
        // Fallback: resolve dialogue ID from text (legacy method)
        PlayAudioForText(text, voiceType, delay);
    }

    public void Stop()
    {
        _audioPlayback.Stop();
    }

    /// <summary>
    /// Resolves dialogue ID from text and plays the corresponding audio file.
    /// </summary>
    private void PlayAudioForText(string text, VoiceType voiceType, float delay = 0f)
    {
        // Try to resolve the dialogue ID from the text
        var dialogueId = _dialogueIdResolver.ResolveDialogueId(text);

        if (string.IsNullOrEmpty(dialogueId))
        {
            Main.Logger?.Warning($"Could not resolve dialogue ID for text: {text.Substring(0, Math.Min(50, text.Length))}...");
            return;
        }

        // Load the audio file
        var audioClip = _audioFileLoader.LoadAudioFile(dialogueId);

        if (audioClip == null)
        {
            Main.Logger?.Warning($"No audio file found for dialogue ID: {dialogueId}");
            return;
        }

        // Play the audio
        _audioPlayback.PlayAudio(audioClip, delay);
    }
}
