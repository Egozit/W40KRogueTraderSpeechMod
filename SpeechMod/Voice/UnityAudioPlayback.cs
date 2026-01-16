using UnityEngine;
using SpeechMod.Unity.Extensions;
using System.Collections;

namespace SpeechMod.Voice;

/// <summary>
/// Handles audio playback using Unity's AudioSource.
/// </summary>
public class UnityAudioPlayback : IAudioPlayback
{
    private AudioSource _audioSource;
    private MonoBehaviour _coroutineRunner;

    public UnityAudioPlayback(AudioSource audioSource, MonoBehaviour coroutineRunner)
    {
        _audioSource = audioSource ?? throw new System.ArgumentNullException(nameof(audioSource));
        _coroutineRunner = coroutineRunner ?? throw new System.ArgumentNullException(nameof(coroutineRunner));
    }

    public bool IsPlaying()
    {
        return _audioSource.isPlaying;
    }

    public void PlayAudio(AudioClip clip, float delay = 0f)
    {
        if (clip == null)
        {
            Main.Logger?.Warning("AudioClip is null");
            return;
        }

        if (delay <= 0f)
        {
            PlayAudioImmediate(clip);
        }
        else
        {
            _coroutineRunner.StartCoroutine(PlayAudioDelayed(clip, delay));
        }
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    private void PlayAudioImmediate(AudioClip clip)
    {
        // Stop any currently playing audio
        if (Main.Settings?.InterruptPlaybackOnPlay == true && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.clip = clip;
        _audioSource.Play();
    }

    private IEnumerator PlayAudioDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayAudioImmediate(clip);
    }
}
