using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using Harmony;
using System.Linq;
using System.Collections;
using ArenaLoader;
using AutoLightshow;

namespace AuthorableModifiers
{
    public class AuthorableModifiersMod : MelonMod
    {
        public static List<Modifier> awaitEnableModifiers = new List<Modifier>();
        public static List<Modifier> awaitDisableModifiers = new List<Modifier>();
        public static List<Modifier> preloadModifiers = new List<Modifier>();
        public static List<Modifier> zOffsetList = new List<Modifier>();
        public static List<ModifierQueueItem> modifierQueue = new List<ModifierQueueItem>();
        public static List<Modifier> singleUseModifiers = new List<Modifier>();
        public static Dictionary<int, float> oldOffsetDict = new Dictionary<int, float>();
        public static string audicaFilePath = "";
        public static bool modifiersFound = false;

        public static Vector3 debugTextPosition = new Vector3(0f, 8f, 8f);

        public static Psychedelia activePsychedelia = null;
        public static ColorChange activeColorChange = null;
        public static string oldArena = "";
        public static Color oldLeftHandColor;
        public static Color oldRightHandColor;
        public static bool oldColorsSet = false;
        public static bool oldArenaSet = false;
        public static float defaultArenaBrightness = .5f;
        public static float defaultArenaReflection = 1f;
        public static float userArenaBrightness = .5f;
        public static float userArenaReflection = 1f;
        public static float userArenaRotation = 0f;

        public static DebugTextPopup popupText = null;
        public static bool lightshowWasEnabled = false;
        public static class BuildInfo
        {
            public const string Name = "AuthorableModifiers";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Continuum"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "1.2.2"; // Version of the Mod.  (MUST BE SET)
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
            while (EnvironmentLoader.I.IsSwitching())
            {
                yield return new WaitForSecondsRealtime(.2f);
            }
            defaultArenaBrightness = RenderSettings.skybox.GetFloat("_Exposure");
            defaultArenaReflection = RenderSettings.reflectionIntensity;
        }

        public static void ApplyZOffset()
        {
            if (!Config.enabled) return;
            SetOldOffsets();
            zOffsetList.Sort((z1, z2) => z1.startTick.CompareTo(z2.startTick));
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
            if (audicaFilePath.Length == 0)
            {
                MelonLogger.Log("Audica file path not found.");
                modifiersFound = false;
                return;
            }
            awaitEnableModifiers = Decoder.GetModifierCues(audicaFilePath);
            if (awaitEnableModifiers is null || (awaitEnableModifiers.Count == 0 && preloadModifiers.Count == 0 && zOffsetList.Count == 0))
            {
                //MelonLogger.Log("Couldn't load data in modifierCues.json. Please check if there are any typos present.");
                modifiersFound = false;
                return;
            }
            if (Integrations.autoLightshowFound)
            {
                EnableAutoLightshow(false);           
            }
            foreach (Modifier m in preloadModifiers)
            {
                 m.Activate();                                         
            }
            foreach(Modifier m in awaitEnableModifiers)
            {
                modifierQueue.Add(new ModifierQueueItem(m, m.startTick, Action.Activate));
                if (!m.isSingleUseModule) modifierQueue.Add(new ModifierQueueItem(m, m.endTick, Action.Deactivate));
            }
            MelonLogger.Log("Modifiers loaded.");
            //awaitEnableModifiers.Sort((mod1, mod2) => mod1.startTick.CompareTo(mod2.startTick));
            modifierQueue.Sort((mod1, mod2) => (mod1.tick - (int)mod1.action).CompareTo((mod2.tick - (int)mod2.action)));
            modifiersFound = true;
            if((Config.enableFlashingLights || Config.enableArenaRotation) && !fromRestart && !Config.hideWarning)
                MelonCoroutines.Start(IWaitForArenaLoad("<color=\"red\">WARNING</color>\nMay contain flashing lights and rotating arenas. \nThis can be disabled in Mod Settings.", .001f));
        }

        private static void EnableAutoLightshow(bool enable)
        {
            if (Integrations.autoLightshowFound)
            {
                if (AutoLightshowMod.isEnabled)
                {
                    if (!enable) lightshowWasEnabled = true;
                }
                
                if(!enable || (enable && lightshowWasEnabled))
                    AutoLightshowMod.EnableMod(enable);
            }
        }

        public static void SetOldArena(string arena)
        {
            if (oldArenaSet) return;
            oldArena = arena;
            oldArenaSet = true;
        }

        private static IEnumerator IWaitForArenaLoad(string text, float speed)
        {
            while (EnvironmentLoader.I.IsSwitching())
            {
                yield return new WaitForSecondsRealtime(.2f);
            }
            DebugWarningPopup(text, speed);
        }

        public static void DebugWarningPopup(string text, float speed)
        {
            popupText = KataConfig.I.CreateDebugText(text, debugTextPosition, 4f, null, false, speed);
        }

        public static void DebugTextPopup(string text, float size, Vector3 position, bool glow)
        {
            popupText = KataConfig.I.CreateDebugText(text, position, size, null, glow, .001f);
        }

        public static void DestroyPopup()
        {
            if(popupText != null)
            {
                GameObject.Destroy(popupText.transform.root.gameObject);
            }
        }

        public static void SetOldColors(Color l, Color r)
        {
            if (oldColorsSet) return;
            oldLeftHandColor = l;
            oldRightHandColor = r;
            oldColorsSet = true;
        }

        public static void SetUserBrightness(float brightness, float rotation, float reflection)
        {
            userArenaBrightness = brightness;
            userArenaRotation = rotation;
            userArenaReflection = reflection;
        }

        public static void OnRestart()
        {
            if (!Config.enabled) return;
            if (!modifiersFound) return;          
            ResetValues();
            LoadModifierCues(true);
        }

        public static void Reset(bool fromBack = false)
        {
            if (!Config.enabled) return;
            if (!modifiersFound) return;
            modifiersFound = false;
            ResetValues(fromBack);
            DestroyPopup();
            oldColorsSet = false;
            oldArenaSet = false;
            zOffsetList.Clear();
            oldOffsetDict.Clear();
            EnableAutoLightshow(true);
            lightshowWasEnabled = false;
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

        private static void ResetValues(bool fromBack = false)
        {
            //foreach (Modifier mod in awaitDisableModifiers) mod.Deactivate();
            // foreach (Modifier mod in preloadModifiers) mod.Deactivate();
            //foreach (Modifier mod in singleUseModifiers) mod.Deactivate();
            if (!fromBack)
            {
                foreach (ModifierQueueItem item in modifierQueue) item.modifier.Deactivate();
            }
            
            modifierQueue.Clear();
            awaitDisableModifiers.Clear();
            awaitEnableModifiers.Clear();
            preloadModifiers.Clear();
            singleUseModifiers.Clear();
            activePsychedelia = null;
            activeColorChange = null;
            if (oldColorsSet)
            {
                new ColorChange(ModifierType.ColorChange, 0, 0, new float[] { 0f, 0f, 0f }, new float[] { 0f, 0f, 0f }).UpdateColors(oldLeftHandColor, oldRightHandColor);
            }
                
            if (Integrations.arenaLoaderFound)
            {
                /*RenderSettings.skybox.SetFloat("_Exposure", userArenaBrightness);
                RenderSettings.skybox.SetFloat("_Rotation", userArenaRotation);
                ArenaLoaderMod.CurrentSkyboxExposure = userArenaBrightness;
                ArenaLoaderMod.currentSkyboxRotation = userArenaRotation;
                ArenaLoaderMod.CurrentSkyboxReflection = userArenaReflection;
                */
                if (oldArena.Length > 0 && oldArenaSet)
                {
                    if(oldArena != PlayerPreferences.I.Environment.Get())
                    {
                        PlayerPreferences.I.Environment.Set(oldArena);
                        EnvironmentLoader.I.SwitchEnvironment();                    
                    }                   
                }
                MelonCoroutines.Start(IResetArenaValues(true));
            }
        }

        private static IEnumerator IResetArenaValues(bool resetToUserValues)
        {
            while (EnvironmentLoader.I.IsSwitching())
            {
                yield return new WaitForSecondsRealtime(.2f);
            }
            float brightness = resetToUserValues ? userArenaBrightness : defaultArenaBrightness;
            float rotation = resetToUserValues ? userArenaRotation : 0f;
            float reflection = resetToUserValues ? userArenaReflection : defaultArenaReflection;
            ArenaLoaderMod.CurrentSkyboxExposure = brightness;
            ArenaLoaderMod.currentSkyboxRotation = rotation;
            RenderSettings.skybox.SetFloat("_Exposure", brightness);
            RenderSettings.skybox.SetFloat("_Rotation", rotation);
            ArenaLoaderMod.CurrentSkyboxReflection = 0f;
            ArenaLoaderMod.ChangeReflectionStrength(reflection);
        }

        public override void OnUpdate()
        {
            if (!Config.enabled) return;
            if (MenuState.sState != MenuState.State.Launched) return;
            if (!modifiersFound) return;
            if (AudioDriver.I is null) return;
            if (!AudioDriver.I.IsPlaying()) return;
            float currentTick = AudioDriver.I.mCachedTick;

            if(modifierQueue.Count > 0)
            {
                if(modifierQueue[0].tick <= currentTick)
                {
                    if (modifierQueue[0].action == Action.Activate) modifierQueue[0].modifier.Activate();
                    else modifierQueue[0].modifier.Deactivate();

                    modifierQueue.RemoveAt(0);
                }
            }
            /*
            if (awaitDisableModifiers.Count > 0)
            {
                for(int i = 0; i < awaitDisableModifiers.Count; i++)
                {
                    if (awaitDisableModifiers[0].endTick <= currentTick)
                    {
                        awaitDisableModifiers[0].Deactivate();
                        awaitDisableModifiers.RemoveAt(0);
                    }
                    else
                    {
                        break;
                    }
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
            */
        }

        public struct ModifierQueueItem
        {
            public Modifier modifier;
            public float tick;
            public Action action;

            public ModifierQueueItem(Modifier _modifier, float _tick, Action _action)
            {
                modifier = _modifier;
                tick = _tick;
                action = _action;
            }
        }

        public enum Action
        {
            Activate = 0,
            Deactivate = 1
        }



    }
}














































































