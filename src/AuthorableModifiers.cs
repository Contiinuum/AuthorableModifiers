using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using Harmony;
using System.Linq;
using System.Collections;

namespace AudicaModding
{
    public class AuthorableModifiers : MelonMod
    {
        public static List<Modifier> awaitEnableModifiers = new List<Modifier>();
        public static List<Modifier> awaitDisableModifiers = new List<Modifier>();
        public static List<Modifier> preloadModifiers = new List<Modifier>();
        public static List<Modifier> zOffsetList = new List<Modifier>();
        public static Dictionary<int, float> oldOffsetDict = new Dictionary<int, float>();
        public static string audicaFilePath = "";
        public static bool modifiersFound = false;

        public static Psychedelia activePsychedelia = null;
        public static ColorChange activeColorChange = null;
        public static string oldArena = "";
        public static Color oldLeftHandColor;
        public static Color oldRightHandColor;
        public static bool oldColorsSet = false;
        public static bool oldArenaSet = false;
        public static bool arenaBrightnessSet = false;
        public static float defaultArenaBrightness = .5f;
        public static float userArenaBrightness = .5f;
        public static float userArenaRotation = 0f;

        public static class BuildInfo
        {
            public const string Name = "AuthorableModifiers";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Continuum"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "1.1.4"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }

        public override void OnApplicationStart()
        {
            HarmonyInstance instance = HarmonyInstance.Create("AuthorableModifiers");
            Config.RegisterConfig();
            Integrations.LookForIntegrations();
        }

        public override void OnModSettingsApplied()
        {
            Config.OnModSettingsApplied();
        }

        public static IEnumerator ISetDefaultArenaBrightness()
        {
            if (!Config.enabled) yield break;
            if (KataConfig.I.practiceMode) yield break;
            if (arenaBrightnessSet) yield break;
            while (EnvironmentLoader.I.IsSwitching())
            {
                yield return new WaitForSecondsRealtime(.2f);
            }
            defaultArenaBrightness = RenderSettings.skybox.GetFloat("_Exposure");
            arenaBrightnessSet = true;
        }

        public static void ApplyZOffset()
        {
            SetOldOffsets();
            foreach(Modifier m in zOffsetList)
            {
                m.Activate();
            }
        }

        public static void LoadModifierCues(bool fromRestart = false)
        {
            if (!Config.enabled) return;
            if (KataConfig.I.practiceMode) return;
            Reset();
            MelonLogger.Log("Loading modifierCues.json...");
            if (audicaFilePath.Length == 0)
            {
                MelonLogger.Log("Audica file path not found.");
                modifiersFound = false;
                return;
            }
            awaitEnableModifiers = Decoder.GetModifierCues(audicaFilePath);
            if (awaitEnableModifiers is null || (awaitEnableModifiers.Count == 0 && preloadModifiers.Count == 0))
            {
                MelonLogger.Log("Couldn't load data in modifierCues.json. Please check if there are any typos present.");
                modifiersFound = false;
                return;
            }
            foreach(Modifier m in preloadModifiers)
            {
                m.Activate();                
            }
            MelonLogger.Log("Modifiers loaded.");
            awaitEnableModifiers.Sort((mod1, mod2) => mod1.startTick.CompareTo(mod2.startTick));
            modifiersFound = true;
        }

        public static void SetOldArena(string arena)
        {
            if (oldArenaSet) return;
            oldArena = arena;
            oldArenaSet = true;
        }

        public static void SetOldColors(Color l, Color r)
        {
            if (oldColorsSet) return;
            oldLeftHandColor = l;
            oldRightHandColor = r;
            oldColorsSet = true;
        }

        public static void SetUserBrightness(float brightness, float rotation)
        {
            userArenaBrightness = brightness;
            userArenaRotation = rotation;
        }

        public static void OnRestart()
        {
            if (!Config.enabled) return;
            if (!modifiersFound) return;          
            ResetValues();
            LoadModifierCues();
        }

        public static void Reset()
        {
            if (!Config.enabled) return;
            if (!modifiersFound) return;
            modifiersFound = false;
            ResetValues();
            oldColorsSet = false;
            oldArenaSet = false;
            zOffsetList.Clear();
            oldOffsetDict.Clear();
        }

        public static void SetOldOffsets()
        {
            SongCues.Cue[] songCues = SongCues.I.mCues.cues;
            foreach (SongCues.Cue c in songCues)
            {
                if(c.behavior != Target.TargetBehavior.Dodge && c.behavior != Target.TargetBehavior.Melee)
                {
                    int key = c.tick + (int)c.handType;
                    if(!oldOffsetDict.ContainsKey(key)) oldOffsetDict.Add(key, c.zOffset);
                }
               
            }
               
        }

        private static void ResetValues()
        {
            foreach (Modifier mod in awaitDisableModifiers) mod.Deactivate();
            foreach (Modifier mod in preloadModifiers) mod.Deactivate();
            awaitDisableModifiers.Clear();
            awaitEnableModifiers.Clear();
            preloadModifiers.Clear();
            activePsychedelia = null;
            activeColorChange = null;
            if (oldColorsSet) new ColorChange(ModifierType.ColorChange, 0, 0, new float[] { 0f, 0f, 0f}, new float[] { 0f, 0f, 0f}).UpdateColors(oldLeftHandColor, oldRightHandColor);
            if (Integrations.arenaLoaderFound)
            {
                //RenderSettings.skybox.SetFloat("_Exposure", userArenaBrightness);
                //RenderSettings.skybox.SetFloat("_Rotation", userArenaRotation);
                AudicaMod.currentSkyboxExposure = userArenaBrightness;
                AudicaMod.currentSkyboxRotation = userArenaRotation;
                if (oldArena.Length > 0)
                {
                    PlayerPreferences.I.Environment.Set(oldArena);
                    EnvironmentLoader.I.SwitchEnvironment();
                }
                MelonCoroutines.Start(IResetArenaValues());
            }
        }

        private static IEnumerator IResetArenaValues()
        {
            while (EnvironmentLoader.I.IsSwitching())
            {
                yield return new WaitForSecondsRealtime(.2f);
            }
            RenderSettings.skybox.SetFloat("_Exposure", userArenaBrightness);
            RenderSettings.skybox.SetFloat("_Rotation", userArenaRotation);
        }

        public override void OnUpdate()
        {
            if (!Config.enabled) return;
            if (MenuState.sState != MenuState.State.Launched) return;
            if (!modifiersFound) return;
            if (AudioDriver.I is null) return;
            if (!AudioDriver.I.IsPlaying()) return;
            float currentTick = AudioDriver.I.mCachedTick;
            if (awaitDisableModifiers.Count > 0)
            {
                if (awaitDisableModifiers[0].endTick <= currentTick)
                {
                    awaitDisableModifiers[0].Deactivate();
                    awaitDisableModifiers.RemoveAt(0);
                }
            }

            if (awaitEnableModifiers.Count > 0)
            {
                if(awaitEnableModifiers[0].startTick <= currentTick)
                {
                    if (!awaitEnableModifiers[0].isSingleUseModule)
                    {
                        awaitDisableModifiers.Add(awaitEnableModifiers[0]);
                        awaitDisableModifiers.Sort((mod1, mod2) => mod1.endTick.CompareTo(mod2.endTick));
                    }
                    
                    awaitEnableModifiers[0].Activate();
                    awaitEnableModifiers.RemoveAt(0);
                   
                }
            }
        }



    }
}














































































