using MelonLoader;
using System.Reflection;

namespace AudicaModding
{
    public static class Config
    {
        public const string Category = "AuthorableModifiers";

        public static bool enabled;
        public static bool enableColorChange;
        public static bool enableFlashingLights;
        public static bool enableArenaRotation;



        public static void RegisterConfig()
        {
            MelonPrefs.RegisterBool(Category, nameof(enabled), true, "Enables Authorable Modifiers.");
            MelonPrefs.RegisterBool(Category, nameof(enableColorChange), true, "Allows maps to change your colors.");
            MelonPrefs.RegisterBool(Category, nameof(enableFlashingLights), true, "Allows maps to use brightness changes and flashing lights. TURN THIS OFF IF YOU HAVE EPILEPSY!");
            MelonPrefs.RegisterBool(Category, nameof(enableArenaRotation), true, "Allows maps to rotate the arena. TURN THIS OFF IF YOU EASILY SUFFER FROM MOTION SICKNESS!");

            OnModSettingsApplied();
        }

        public static void OnModSettingsApplied()
        {
            foreach (var fieldInfo in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (fieldInfo.FieldType == typeof(bool)) fieldInfo.SetValue(null, MelonPrefs.GetBool(Category, fieldInfo.Name));
            }

            AuthorableModifiers.modifiersFound = false;
        }
    }
}
