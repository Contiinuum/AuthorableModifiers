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
        public static bool hideWarning;

        public static string lightingTitle = "[Header]Lighting Options";
        public static float intensity;


        public static void RegisterConfig()
        {
            MelonPrefs.RegisterBool(Category, nameof(enabled), true, "Enables Authorable Modifiers.");
            MelonPrefs.RegisterBool(Category, nameof(enableColorChange), true, "Allows maps to change your colors.");
            MelonPrefs.RegisterBool(Category, nameof(enableFlashingLights), true, "Allows maps to use brightness changes and flashing lights. TURN THIS OFF IF YOU HAVE EPILEPSY!");
            MelonPrefs.RegisterBool(Category, nameof(enableArenaRotation), true, "Allows maps to rotate the arena. TURN THIS OFF IF YOU EASILY SUFFER FROM MOTION SICKNESS!");
            MelonPrefs.RegisterBool(Category, nameof(enableArenaManipulation), true, "Allows maps to manipulate the arena (rotate, move, scale). TURN THIS OFF IF YOU EASILY SUFFER FROM MOTION SICKNESS!");
            MelonPrefs.RegisterBool(Category, nameof(enablePsychedelia), true, "Allows maps to make use of Psychedelia.");
            MelonPrefs.RegisterBool(Category, nameof(enableScoreDisablingModifiers), false, "Allows mods to use modifiers that disable posting your score to the leaderboards.");
            MelonPrefs.RegisterBool(Category, nameof(hideWarning), false, "Hides the warning before starting a song.");

            MelonPrefs.RegisterString(Category, nameof(lightingTitle), "", lightingTitle);
            MelonPrefs.RegisterFloat(Category, nameof(intensity), 1f, "Controls how intense lighting modifiers are. [0.1, 1, 0.1, 1]{P}");

            OnModSettingsApplied();
        }

        public static void OnModSettingsApplied()
        {
            foreach (var fieldInfo in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (fieldInfo.Name == "Category") continue;
                if (fieldInfo.FieldType == typeof(bool)) fieldInfo.SetValue(null, MelonPrefs.GetBool(Category, fieldInfo.Name));
                else if (fieldInfo.FieldType == typeof(float)) fieldInfo.SetValue(null, MelonPrefs.GetFloat(Category, fieldInfo.Name));
                else if (fieldInfo.FieldType == typeof(string)) fieldInfo.SetValue(null, MelonPrefs.GetString(Category, fieldInfo.Name));
            }

            AuthorableModifiersMod.modifiersFound = false;
        }
    }
}
