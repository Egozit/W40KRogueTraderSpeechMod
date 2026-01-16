using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Localization;
using SpeechMod.Voice;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(DialogVM), nameof(DialogVM.HandleOnCueShow))]
public static class Dialog_Patch
{
    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(DialogVM)}_HandleOnCueShow_Postfix");
#endif

        if (!Main.Settings!.AutoPlay)
        {
#if DEBUG
            Debug.Log($"{nameof(DialogVM)}: AutoPlay is disabled!");
#endif
            return;
        }

        var key = Game.Instance?.DialogController?.CurrentCue?.Text?.Key;
        if (string.IsNullOrWhiteSpace(key))
            key = Game.Instance?.DialogController?.CurrentCue?.Text?.Shared?.String?.Key;

        if (string.IsNullOrWhiteSpace(key))
        {
            Main.Logger?.Warning("[DialogPatch] No dialogue key found!");
            return;
        }

        var text = Game.Instance?.DialogController?.CurrentCue?.DisplayText;
        var msg1 = $"[DialogPatch] ========== DIALOGUE STARTED ==========";
        var msg2 = $"[DialogPatch] UUID/Key: {key}";
        var msg3 = $"[DialogPatch] Text: {text}";
        Main.Logger?.Log(msg1);
        Main.Logger?.Log(msg2);
        Main.Logger?.Log(msg3);
        Main.FileLog(msg1);
        Main.FileLog(msg2);
        Main.FileLog(msg3);

        // Stop playing and don't play if the dialog is voice acted.
        if (!Main.Settings.AutoPlayIgnoreVoice && !string.IsNullOrWhiteSpace(LocalizationManager.Instance.SoundPack?.GetText(key!, false)))
        {
            var msg = $"[DialogPatch] Dialogue is voice acted, stopping TTS";
            Main.Logger?.Log(msg);
            Main.FileLog(msg);
            Main.Speech.Stop();
            return;
        }

        // Try to play audio file first
        if (Main.AudioFileLoader != null && Main.AudioFilePlayer != null)
        {
            var msgAttempt = $"[DialogPatch] Attempting to load audio file for UUID: {key}";
            Main.Logger?.Log(msgAttempt);
            Main.FileLog(msgAttempt);
            
            // Check if file exists first
            var filePath = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath, 
                "UnityModManager", 
                "W40KSpeechMod", 
                "Localization", 
                "Audio", 
                Main.CurrentLanguage, 
                $"{key}.wav"
            );
            
            if (System.IO.File.Exists(filePath))
            {
                var msgSuccess = $"[DialogPatch] ✓ Audio file found for UUID: {key}";
                Main.Logger?.Log(msgSuccess);
                Main.FileLog(msgSuccess);
                
                // Start async loading and playback
                Main.AudioFilePlayer.LoadAndPlayAudioFile(filePath);
                
                Main.Logger?.Log($"[DialogPatch] ========== DIALOGUE ENDED ==========");
                Main.FileLog($"[DialogPatch] ========== DIALOGUE ENDED ==========");
                return; // Don't play TTS
            }
            else
            {
                var msgFailed = $"[DialogPatch] ✗ FAILED: No audio file found for UUID: {key}";
                var msgPath = $"[DialogPatch] Expected path: SpeechMod\\Localization\\Audio\\{Main.CurrentLanguage}\\{key}.wav";
                Main.Logger?.Log(msgFailed);
                Main.Logger?.Log(msgPath);
                Main.FileLog(msgFailed);
                Main.FileLog(msgPath);
            }
        }
        else
        {
            if (Main.AudioFileLoader == null)
            {
                var msgNull1 = "[DialogPatch] Audio file loader is NULL!";
                Main.Logger?.Warning(msgNull1);
                Main.FileLog(msgNull1);
            }
            if (Main.AudioFilePlayer == null)
            {
                var msgNull2 = "[DialogPatch] Audio file player is NULL!";
                Main.Logger?.Warning(msgNull2);
                Main.FileLog(msgNull2);
            }
        }

        var msgFallback = $"[DialogPatch] Using TTS fallback for UUID: {key}";
        Main.Logger?.Log(msgFallback);
        Main.FileLog(msgFallback);

        // Fallback to TTS
        Main.Speech?.SpeakDialog(text, 0.5f, key);
        
        var msgEnd = $"[DialogPatch] ========== DIALOGUE ENDED ==========";
        Main.Logger?.Log(msgEnd);
        Main.FileLog(msgEnd);
    }
}