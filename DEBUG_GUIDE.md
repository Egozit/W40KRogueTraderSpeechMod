# Debugging Audio File Playback

## Проблема
Аудиофайлы не воспроизводятся, вместо них играет TTS.

## Путь к логам
Откройте логи мода в игре: **Ctrl+F10** → ищите сообщения с префиксом `[AudioLoad]`, `[AudioFilePlayer]`, `[DialogPatch]`, `SetSpeech()`.

## Контрольный список для отладки

### 1. Проверьте, что `UseAudioFiles` включена
В игре в настройках мода проверьте, что параметр **UseAudioFiles** = `true`.

**Ожидаемые логи в консоли:**
```
SetSpeech() - UseAudioFiles: True
Initializing Audio File Playback mode
Audio Loader initialized for language: ruRU
Mod folder path: C:\Users\[YourUsername]\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod
Audio folder path: C:\Users\[YourUsername]\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\ruRU\Audio
Audio File Playback initialized successfully!
```

Если видите `SetSpeech() - UseAudioFiles: False` → **ПРОБЛЕМА: SetUse AudioFiles не включена в настройках!**

---

### 2. Проверьте путь к папке модов

Папка должна находиться здесь (скопируйте путь в Windows Explorer):
```
%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod
```

Структура папок:
```
W40KSpeechMod/
├── Localization/
│   └── ruRU/
│       └── Audio/
│           ├── dialogue_key_1.wav
│           ├── dialogue_key_2.wav
│           └── ... остальные файлы
├── SpeechMod.dll
└── другие файлы мода
```

**Если папки нет → создайте её!** И скопируйте туда `.wav` файлы.

---

### 3. Когда начнется диалог, проверьте логи

**Ожидаемая последовательность логов:**

```
[DialogPatch] Dialogue key: dialogue_npc_guard_001
[DialogPatch] UseAudioFiles: True
[DialogPatch] Calling SpeakDialog with key: dialogue_npc_guard_001
[AudioFilePlayer] PlayAudioForDialogueId called - dialogueId: dialogue_npc_guard_001
[AudioFilePlayer] Attempting to load audio for ID: dialogue_npc_guard_001
[AudioLoad] Attempting to load: dialogue_npc_guard_001
[AudioLoad] Full path: C:\Users\Root\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\ruRU\Audio\dialogue_npc_guard_001.wav
[AudioLoad] File exists: True
[AudioLoad] Loading audio file from: C:\Users\Root\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\ruRU\Audio\dialogue_npc_guard_001.wav
[AudioFilePlayer] Successfully loaded audio clip, playing now
```

---

## Возможные проблемы и решения

### Проблема A: "File exists: False"
**Причина:** Файл находится не в правильной папке или назван неправильно.

**Решение:**
1. Посмотрите в логе путь: `[AudioLoad] Full path: ...`
2. Создайте эту папку вручную (если её нет)
3. Скопируйте `.wav` файл с ТОЧ НО таким же именем как в логе (без расширения это UUID)
4. Перезагрузите сохранение и попробуйте диалог снова

### Проблема B: "Dialogue key: " (пусто)
**Причина:** Диалоговый ключ не извлекается из игры.

**Решение:**
1. Убедитесь, что используется английская локализация
2. Проверьте, что диалог имеет текстовое описание (некоторые диалоги могут быть только голосовыми)

### Проблема C: "No audio file found for dialogue ID: ..."
**Причина:** AudioFileLoader вернул null, но файл якобы существует.

**Решение:**
1. Проверьте, что файл `.wav` открывается в плеере (не поврежден)
2. Попробуйте переконвертировать файл в WAV с параметрами:
   - Sample Rate: 44100 Hz
   - Channels: 2 (Stereo) или 1 (Mono)
   - Bit Depth: 16-bit
3. Убедитесь, что языковая папка совпадает с `CurrentLanguage` (см. в логах `language: ruRU`)

### Проблема D: Логи говорят о успехе, но звука нет
**Причина:** AudioSource не воспроизводит звук.

**Решение:**
1. Проверьте громкость в игре (не приглушена ли музыка/звуки)
2. Убедитесь, что файл `.wav` не поврежден
3. Попробуйте другой файл для тестирования

---

## Как найти правильный UUID диалога

Диалоговый ключ видно в логах как `[DialogPatch] Dialogue key: ...`

Пример логирования всех диалогов:
```csharp
// В Dialog_Patch.cs уже добавлено логирование:
Main.Logger?.Log($"[DialogPatch] Dialogue key: {key}");
```

Полный ключ формируется как: `dialogue_CATEGORY_SUBCATEGORY_NUMBER`

Пример: `dialogue_npc_guard_welcome`

---

## Быстрая проверка

1. **Запустите игру с модом**
2. **Включите UseAudioFiles в настройках**
3. **Откройте лог (Ctrl+F10)**
4. **Начните любой диалог**
5. **В логе найдите строку:**
   ```
   [DialogPatch] Dialogue key: dialogue_...
   ```
6. **Создайте папку** (если её нет):
   ```
   %userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\W40KSpeechMod\Localization\ruRU\Audio\
   ```
7. **Положите туда файл с именем:** `dialogue_...wav` (без кавычек, замените `dialogue_...` на найденный ключ)
8. **Перезагрузите диалог - должен воспроизвестись ваш аудиофайл!**

---

## Структура файла ОБЯЗАТЕЛЬНА

| Параметр | Значение |
|----------|----------|
| Формат | WAV (RIFF) |
| Кодек | PCM |
| Sample Rate | 44100 Hz |
| Channels | Mono или Stereo |
| Bit Depth | 16-bit |
| Имя файла | `{dialogue_key}.wav` |
| Папка | `W40KSpeechMod\Localization\ruRU\Audio\` |

---

## Команда для быстрого создания тестового файла (PowerShell)

Если у вас есть исходный WAV файл, конвертируйте его:

```powershell
# Требует ffmpeg: https://ffmpeg.org/download.html
ffmpeg -i "input.wav" -acodec pcm_s16le -ac 2 -ar 44100 "dialogue_test_greeting.wav"
```
