using UnityEngine;
using SpeechMod.Unity.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SpeechMod.Voice;

/// <summary>
/// Resolves dialogue text back to ID by loading game's localization JSON files.
/// Reads from the game installation: WH40KRT_Data\StreamingAssets\Localization\{language}.json
/// </summary>
public class LocalizationDialogueIdResolver : IDialogueIdResolver
{
    private readonly string _language;
    private readonly Dictionary<string, string> _textToIdMap = new();
    private bool _isLoaded = false;

    public LocalizationDialogueIdResolver(string language)
    {
        _language = language;
    }

    public string ResolveDialogueId(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        // Load localization on first access
        if (!_isLoaded)
        {
            LoadLocalizationData();
        }

        // Normalize the text (remove formatting tags, etc.)
        var normalizedText = NormalizeText(text);

        // Try to find matching text in our map
        if (_textToIdMap.TryGetValue(normalizedText, out var id))
        {
            return id;
        }

        // Log a warning if not found
        Main.Logger?.Warning($"Could not find dialogue ID for text (first 50 chars): {text.Substring(0, Math.Min(50, text.Length))}");
        return null;
    }

    public void RegisterDialogue(string id, string text)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(text))
            return;

        var normalizedText = NormalizeText(text);
        _textToIdMap[normalizedText] = id;
    }

    /// <summary>
    /// Loads localization JSON file from game installation.
    /// Path: WH40KRT_Data\StreamingAssets\Localization\{language}.json
    /// </summary>
    private void LoadLocalizationData()
    {
        _isLoaded = true;

        try
        {
            // Get the game's data path from Kingmaker game instance
            var gameDataPath = GetGameLocalizationPath();
            if (string.IsNullOrEmpty(gameDataPath))
            {
                Main.Logger?.Warning("Could not determine game localization path");
                return;
            }

            var localizationFilePath = Path.Combine(gameDataPath, $"{_language}.json");

            if (!File.Exists(localizationFilePath))
            {
                Main.Logger?.Warning($"Localization file not found: {localizationFilePath}");
                return;
            }

            // Read the JSON file
            var jsonContent = File.ReadAllText(localizationFilePath);
            ParseLocalizationJson(jsonContent);
        }
        catch (Exception ex)
        {
            Main.Logger?.Error($"Error loading localization data: {ex.Message}");
        }
    }

    /// <summary>
    /// Determines the game's localization folder path.
    /// </summary>
    private string GetGameLocalizationPath()
    {
        try
        {
            // Get game data path from Kingmaker
            var gameDataPath = UnityEngine.Application.streamingAssetsPath;
            var localizationPath = Path.Combine(gameDataPath, "Localization");
            
            if (Directory.Exists(localizationPath))
                return localizationPath;

            Main.Logger?.Warning($"Localization directory not found at: {localizationPath}");
            return null;
        }
        catch (Exception ex)
        {
            Main.Logger?.Error($"Error getting game localization path: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Parses localization JSON to extract dialogue IDs and their text.
    /// Expected format:
    /// {
    ///   "uuid.wav": { "Offset": 0, "Text": "Dialogue text..." },
    ///   ...
    /// }
    /// </summary>
    private void ParseLocalizationJson(string jsonContent)
    {
        if (string.IsNullOrEmpty(jsonContent))
            return;

        try
        {
            // Simple regex to extract id and text pairs
            // Pattern: "uuid.wav": { ... "Text": "text content" ...}
            var idPattern = @"""([a-f0-9\-]+)\.wav""\s*:\s*\{[^}]*""Text""\s*:\s*""([^""]*)""";
            var matches = Regex.Matches(jsonContent, idPattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    var id = match.Groups[1].Value;
                    var text = match.Groups[2].Value;
                    
                    // Unescape JSON string
                    text = System.Text.RegularExpressions.Regex.Unescape(text);
                    
                    RegisterDialogue(id, text);
                }
            }

            Main.Logger?.Log($"Loaded {_textToIdMap.Count} dialogue entries from {_language} localization");
        }
        catch (System.Exception ex)
        {
            Main.Logger?.Error($"Error parsing localization JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Normalizes dialogue text for matching.
    /// Removes formatting tags, special characters, etc.
    /// </summary>
    private string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove color tags like <color=#...>, </color>
        text = Regex.Replace(text, @"<color=#[0-9a-fA-F]{6}>", "");
        text = Regex.Replace(text, @"</color>", "");

        // Remove other formatting tags
        text = Regex.Replace(text, @"<[^>]+>", "");

        // Remove narrator color markers
        text = text.Replace($"<i><color=#{Constants.NARRATOR_COLOR_CODE}>", "");
        text = text.Replace("</i>", "");

        // Normalize whitespace
        text = Regex.Replace(text, @"\s+", " ").Trim();

        return text;
    }
}
