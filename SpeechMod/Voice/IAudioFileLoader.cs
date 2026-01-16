using UnityEngine;

namespace SpeechMod.Voice;

/// <summary>
/// Loads audio files by dialogue ID (UUID).
/// Audio files are organized by language and named with their ID (e.g., abada903-ebd9-44f0-afe1-5066224cd195.wav).
/// </summary>
public interface IAudioFileLoader
{
    /// <summary>
    /// Loads an audio file by its dialogue ID.
    /// </summary>
    /// <param name="dialogueId">The UUID of the dialogue (e.g., "abada903-ebd9-44f0-afe1-5066224cd195")</param>
    /// <returns>AudioClip if found, null otherwise</returns>
    AudioClip LoadAudioFile(string dialogueId);

    /// <summary>
    /// Gets the current load status (how many audio files are available, etc.)
    /// </summary>
    string GetLoadStatus();
}
