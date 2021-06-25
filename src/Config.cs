using MelonLoader;
using System.Reflection;

namespace AuthorableModifiers
{
    public static class Config
    {
        public const string Category = "AuthorableModifiers";

        public static bool enabled;
        public static bool enableColorChange;
        public static bool enablePsychedelia;
        public static bool enableFlashingLights;
        public static bool enableArenaRotation;
        public static bool enableArenaManipulation;
        public static bool enableScoreDisablingModifiers;
        public static bool enableSkyboxColorChange;
        public static bool hideWarning;

        public static string lightingTitle = "[Header]Lighting Options";
        public static float intensity;

        public static bool postToApi;
        public static string apiUrl;

        public static void RegisterConfig()
        {
            MelonPreferences.CreateEntry(Category, nameof(enabled), true, "Enables Authorable Modifiers.");
            MelonPreferences.CreateEntry(Category, nameof(enableColorChange), true, "Allows maps to change your colors.");
            MelonPreferences.CreateEntry(Category, nameof(enableFlashingLights), true, "Allows maps to use brightness changes and flashing lights. TURN THIS OFF IF YOU HAVE EPILEPSY!");
            MelonPreferences.CreateEntry(Category, nameof(enableArenaRotation), true, "Allows maps to rotate the arena. TURN THIS OFF IF YOU EASILY SUFFER FROM MOTION SICKNESS!");
            MelonPreferences.CreateEntry(Category, nameof(enableArenaManipulation), true, "Allows maps to manipulate the arena (rotate, move, scale). TURN THIS OFF IF YOU EASILY SUFFER FROM MOTION SICKNESS!");
            MelonPreferences.CreateEntry(Category, nameof(enablePsychedelia), true, "Allows maps to make use of Psychedelia.");
            MelonPreferences.CreateEntry(Category, nameof(enableScoreDisablingModifiers), false, "Allows mods to use modifiers that disable posting your score to the leaderboards.");
            MelonPreferences.CreateEntry(Category, nameof(enableSkyboxColorChange), true, "Allows maps to use Skybox Color Changes.");
            MelonPreferences.CreateEntry(Category, nameof(hideWarning), false, "Hides the warning before starting a song.");

            MelonPreferences.CreateEntry(Category, nameof(lightingTitle), "", "[Header]Lighting Options");
            MelonPreferences.CreateEntry(Category, nameof(intensity), 1f, "Controls how intense lighting modifiers are. [0.1, 1, 0.1, 1]{P}");

            MelonPreferences.CreateEntry(Category, nameof(postToApi), false, "Posts Brightness and Color data to specified URL in apiUrl");
            MelonPreferences.CreateEntry(Category, nameof(apiUrl), "", apiUrl);

            OnPreferencesSaved();
        }

        public static void OnPreferencesSaved()
        {
            foreach (var fieldInfo in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (fieldInfo.Name == "Category") continue;
                if (fieldInfo.FieldType == typeof(bool)) fieldInfo.SetValue(null, MelonPreferences.GetEntryValue<bool>(Category, fieldInfo.Name));
                else if (fieldInfo.FieldType == typeof(float)) fieldInfo.SetValue(null, MelonPreferences.GetEntryValue<float>(Category, fieldInfo.Name));
                else if (fieldInfo.FieldType == typeof(string)) fieldInfo.SetValue(null, MelonPreferences.GetEntryValue<string>(Category, fieldInfo.Name));
            }

            AuthorableModifiersMod.modifiersFound = false;
        }
    }
}
