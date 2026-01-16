using UnityEngine;

namespace SpeechMod.Voice;

/// <summary>
/// Abstracts audio playback using Unity's AudioSource.
/// </summary>
public interface IAudioPlayback
{
    bool IsPlaying();
    void PlayAudio(AudioClip clip, float delay = 0f);
    void Stop();
}
