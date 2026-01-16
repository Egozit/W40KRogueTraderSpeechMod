using HarmonyLib;
using SpeechMod.Configuration;
using SpeechMod.KeyBinds;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using SpeechMod.Voice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace SpeechMod;

#if DEBUG
[EnableReloading]
#endif
public static class Main
{
    public static UnityModManager.ModEntry.ModLogger Logger;
    public static Settings Settings;
    public static bool Enabled;
    public static string[] FontStyleNames = Enum.GetNames(typeof(FontStyles));
    
    // File logging
    private static string _logFilePath;
    private static readonly object _logFileLock = new object();

    public static string NarratorVoice => VoicesDict?.ElementAtOrDefault(Settings.NarratorVoice).Key;
    public static string FemaleVoice => VoicesDict?.ElementAtOrDefault(Settings.FemaleVoice).Key;
    public static string MaleVoice => VoicesDict?.ElementAtOrDefault(Settings.MaleVoice).Key;
    public static string ProtagonistVoice => VoicesDict?.ElementAtOrDefault(Settings.ProtagonistVoice).Key;

    public static Dictionary<string, string> VoicesDict => Settings?.AvailableVoices?.Select(v =>
    {
        var splitV = v?.Split('#');
        return splitV?.Length != 2
            ? new { Key = v, Value = "Unknown" }
            : new { Key = splitV[0], Value = splitV[1] };
    }).ToDictionary(p => p.Key, p => p.Value);

    public static ISpeech Speech;
    public static IAudioFileLoader AudioFileLoader;
    public static AudioFilePlayerUnity AudioFilePlayer;
    public static string CurrentLanguage = "ruRU";
    private static bool m_Loaded = false;

    /// <summary>
    /// Initialize file logging to the mod folder
    /// </summary>
    private static void InitializeFileLogging()
    {
        try
        {
            // Log to mod folder: AppData\LocalLow\Owlcat Games\...\UnityModManager\W40KSpeechMod\SpeechMod.log
            var modFolder = Path.Combine(Application.persistentDataPath, "UnityModManager", "W40KSpeechMod");
            Directory.CreateDirectory(modFolder);
            _logFilePath = Path.Combine(modFolder, "SpeechMod.log");
            
            // Clear old log file if it's too large (> 5MB)
            if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > 5242880)
            {
                File.Delete(_logFilePath);
            }
            
            FileLog($"========== SpeechMod Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SpeechMod] Failed to initialize file logging: {ex.Message}");
        }
    }

    /// <summary>
    /// Write a message to the log file (public for use by other classes)
    /// </summary>
    public static void FileLog(string message)
    {
        if (string.IsNullOrEmpty(_logFilePath))
            return;

        try
        {
            lock (_logFileLock)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMessage = $"[{timestamp}] {message}";
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
        }
        catch { /* Silently fail if file logging has issues */ }
    }

    private static bool Load(UnityModManager.ModEntry modEntry)
    {
        Debug.Log("Warhammer 40K: Rogue Trader Speech Mod Initializing...");
        
        InitializeFileLogging();
        FileLog("Load() called");

        Logger = modEntry?.Logger;

        Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
        var msg1 = $"Settings loaded - Language={Settings?.CurrentLanguage}";
        Logger?.Log(msg1);
        FileLog(msg1);
        
        CurrentLanguage = Settings?.CurrentLanguage ?? "ruRU";

        // Initialize audio file loader (always available for fallback)
        AudioFileLoader = new LocalizationAudioFileLoader(CurrentLanguage);
        var msg2 = $"[Main] Audio file loader initialized for language: {CurrentLanguage}";
        Logger?.Log(msg2);
        FileLog(msg2);

        // Initialize audio file player for playback
        var audioGameObject = new GameObject("AudioFilePlayer");
        AudioFilePlayer = audioGameObject.AddComponent<AudioFilePlayerUnity>();
        var msg2b = "[Main] Audio file player initialized";
        Logger?.Log(msg2b);
        FileLog(msg2b);

        // Initialize TTS engine (WindowsSpeech or AppleSpeech)
        if (!SetSpeech())
            return false;

        Hooks.UpdateHoverColor();

        modEntry!.OnToggle = OnToggle;
        modEntry!.OnGUI = OnGui;
        modEntry!.OnSaveGUI = OnSaveGui;

        var harmony = new Harmony(modEntry.Info?.Id);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        ModConfigurationManager.Build(harmony, modEntry, Constants.SETTINGS_PREFIX);
        SetUpSettings();
        harmony.CreateClassProcessor(typeof(SettingsUIPatches)).Patch();

        Logger?.Log(Speech?.GetStatusMessage());

        // Set up voices for TTS
        if (!SetAvailableVoices())
            return false;

        PhoneticDictionary.LoadDictionary();

        Debug.Log("Warhammer 40K: Rogue Trader Speech Mod Initialized!");

        m_Loaded = true;
        return true;
    }

    private static void SetUpSettings()
    {
        if (ModConfigurationManager.Instance.GroupedSettings.TryGetValue("main", out _))
            return;

        ModConfigurationManager.Instance.GroupedSettings.Add("main", [new PlaybackStop(), new ToggleBarks()]);
    }

    private static bool SetAvailableVoices()
    {
        var availableVoices = Speech?.GetAvailableVoices();

        if (availableVoices == null || availableVoices.Length == 0)
        {
            Logger?.Warning("No available voices found! Disabling mod!");
            return false;
        }

        Logger?.Log("Available voices:");
        foreach (var voice in availableVoices)
        {
            Logger?.Log(voice);
        }
        Logger?.Log("Setting available voices list...");

        for (var i = 0; i < availableVoices.Length; i++)
        {
            var splitVoice = availableVoices[i]?.Split('#');
            if (splitVoice?.Length != 2 || string.IsNullOrEmpty(splitVoice[1]))
                availableVoices[i] = availableVoices[i]?.Replace("#", "").Trim() + "#Unknown";
        }

        // Ensure that the selected voice index falls within the available voices range
        if (Settings?.NarratorVoice >= availableVoices.Length)
        {
            Logger?.Log($"{nameof(Settings.NarratorVoice)} was out of range, resetting to first voice available.");
            Settings.NarratorVoice = 0;
        }

        if (Settings?.FemaleVoice >= availableVoices.Length)
        {
            Logger?.Log($"{nameof(Settings.FemaleVoice)} was out of range, resetting to first voice available.");
            Settings.FemaleVoice = 0;
        }

        if (Settings?.MaleVoice >= availableVoices.Length)
        {
            Logger?.Log($"{nameof(Settings.MaleVoice)} was out of range, resetting to first voice available.");
            Settings.MaleVoice = 0;
        }

        Settings!.AvailableVoices = availableVoices.OrderBy(v => v.Split('#').ElementAtOrDefault(1)).ToArray();

        return true;
    }

    private static bool SetSpeech()
    {
        Logger?.Log("[SetSpeech] Initializing TTS engine");
        
        switch (Application.platform)
        {
            case RuntimePlatform.OSXPlayer:
                Logger?.Log("[SetSpeech] Platform: macOS");
                Speech = new AppleSpeech();
                SpeechExtensions.AddUiElements<AppleVoiceUnity>(Constants.APPLE_VOICE_NAME);
                break;
            case RuntimePlatform.WindowsPlayer:
                Logger?.Log("[SetSpeech] Platform: Windows");
                Speech = new WindowsSpeech();
                SpeechExtensions.AddUiElements<WindowsVoiceUnity>(Constants.WINDOWS_VOICE_NAME);
                break;
            default:
                Logger?.Critical($"Warhammer 40K: Rogue Trader SpeechMod is not supported on {Application.platform}!");
                return false;
        }

        return true;
    }

    private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
    {
        Enabled = value;
        return true;
    }

    private static void OnGui(UnityModManager.ModEntry modEntry)
    {
        if (m_Loaded)
            MenuGUI.OnGui();
    }

    private static void OnSaveGui(UnityModManager.ModEntry modEntry)
    {
        Hooks.UpdateHoverColor();
        Settings?.Save(modEntry);
    }
}
