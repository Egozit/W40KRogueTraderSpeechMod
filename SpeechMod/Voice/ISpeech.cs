namespace SpeechMod.Voice;

public interface ISpeech
{
    string GetStatusMessage();
    string[] GetAvailableVoices();
    bool IsSpeaking();
    void SpeakPreview(string text, VoiceType voiceType);
    
    /// <summary>
    /// Plays dialogue with optional dialogue ID (UUID).
    /// If dialogueId is provided, it will be used directly instead of text matching.
    /// </summary>
    void SpeakDialog(string text, float delay = 0f, string dialogueId = null);
    
    void SpeakAs(string text, VoiceType type, float delay = 0f, string dialogueId = null);
    void Speak(string text, float delay = 0f, string dialogueId = null);
    void Stop();
}