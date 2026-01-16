namespace SpeechMod.Voice;

/// <summary>
/// Resolves dialogue text back to its ID (UUID) by matching against loaded localization data.
/// </summary>
public interface IDialogueIdResolver
{
    /// <summary>
    /// Tries to resolve a dialogue ID from the text content.
    /// Uses localization files to find matching dialogue.
    /// </summary>
    /// <param name="text">The dialogue text</param>
    /// <returns>The UUID if found, null or empty string otherwise</returns>
    string ResolveDialogueId(string text);

    /// <summary>
    /// Registers a dialogue text-to-ID mapping.
    /// </summary>
    void RegisterDialogue(string id, string text);
}
