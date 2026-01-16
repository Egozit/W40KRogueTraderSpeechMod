# open visual studio build tools 2026
# Navigate to project directory
cd "c:\Users\Root\Desktop\W40KRogueTraderSpeechMod"

# Restore NuGet packages
dotnet restore

# Build in Release mode
dotnet build -c Release

# DLL will be at: SpeechMod\bin\Release\W40KRTSpeechMod.dll