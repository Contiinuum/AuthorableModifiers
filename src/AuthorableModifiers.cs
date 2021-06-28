using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using System.Linq;
using System.Collections;
using ArenaLoader;
using AutoLightshow;
using AudicaModding;

namespace AuthorableModifiers
{
    public class AuthorableModifiersMod : MelonMod
    {
        public static List<Modifier> awaitEnableModifiers = new List<Modifier>();
        public static List<Modifier> awaitDisableModifiers = new List<Modifier>();
        public static List<Modifier> preloadModifiers = new List<Modifier>();
        public static List<Modifier> zOffsetList = new List<Modifier>();
        public static List<AutoLighting> autoLightings = new List<AutoLighting>();
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
        public static bool endless = false;

        public static Color defaultSkyboxColor = new Color();

        public static Dictionary<float, DebugTextPopup> popupTextDictionary = new Dictionary<float, DebugTextPopup>();
        public static bool lightshowWasEnabled = false;
        public static class BuildInfo
        {
            public const string Name = "AuthorableModifiers";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Continuum"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "1.2.6"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }

        public static void SetEndlessActive(bool active)
        {
            endless = active;
        }

        public override void OnApplicationStart()
        {
            Config.RegisterConfig();
            Integrations.LookForIntegrations();
        }

        public override void OnPreferencesSaved()
        {
            Config.OnPreferencesSaved();
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
            zOffsetList.Sort((z1, z2) => z1.StartTick.CompareTo(z2.StartTick));
            foreach(Modifier m in zOffsetList)
            {
                m.Activate();
            }
            autoLightings.Sort((a1, a2) => a1.StartTick.CompareTo(a2.StartTick));
            foreach (AutoLighting al in autoLightings)
            {
                al.Preload();
                //break;
            }
        }
        public static bool modifiersLoaded = false;
        public static void LoadModifierCues(bool fromRestart = false)
        {
            if (!Config.enabled || KataConfig.I.practiceMode)
            {
                modifiersLoaded = true;
                return;
            }
            modifiersLoaded = false;
            Reset();
            if (audicaFilePath.Length == 0)
            {
                MelonLogger.Msg("Audica file path not found.");
                modifiersFound = false;
                modifiersLoaded = true;
                return;
            }
            awaitEnableModifiers = Decoder.GetModifierCues(audicaFilePath);

            if (awaitEnableModifiers is null || (awaitEnableModifiers.Count == 0 && preloadModifiers.Count == 0 && zOffsetList.Count == 0))
            {
                //MelonLogger.Msg("Couldn't load data in modifierCues.json. Please check if there are any typos present.");
                modifiersFound = false;
                modifiersLoaded = true;
                if (endless)
                {
                    ResetArena(true);
                }
                return;
            }
            SetOldColors(KataConfig.I.leftHandColor, KataConfig.I.rightHandColor);
            if (Integrations.autoLightshowFound)
            {
                EnableAutoLightshow(false);
            }
            bool arenaChanged = false;
            foreach (Modifier m in preloadModifiers)
            {
                if (endless && !arenaChanged)
                {
                    if (m.Type == ModifierType.ArenaChange) arenaChanged = true;
                }
                m.Activate();
            }
            if(endless && !arenaChanged)
            {
                ResetArena(true);
            }
            foreach(Modifier m in awaitEnableModifiers)
            {
                modifierQueue.Add(new ModifierQueueItem(m, m.StartTick, Action.Activate));
                if (!m.IsSingleUse) modifierQueue.Add(new ModifierQueueItem(m, m.EndTick, Action.Deactivate));
            }
            MelonLogger.Msg("Modifiers loaded.");
            //awaitEnableModifiers.Sort((mod1, mod2) => mod1.startTick.CompareTo(mod2.startTick));
            modifierQueue.Sort((mod1, mod2) => (mod1.tick - (int)mod1.action).CompareTo((mod2.tick - (int)mod2.action)));
            modifiersFound = true;
            if((Config.enableFlashingLights || Config.enableArenaRotation) && !fromRestart && !Config.hideWarning && !endless)
                MelonCoroutines.Start(IWaitForArenaLoad("<color=\"red\">WARNING</color>\nMay contain flashing lights and rotating arenas. \nThis can be disabled in Mod Settings.", .001f));
            //if (endless) MelonCoroutines.Start(StartTimer());
            //else modifiersLoaded = true;
            MelonCoroutines.Start(WaitForArenaSwitch());
            //modifiersLoaded = true;
        }

        private static IEnumerator WaitForArenaSwitch()
        {
            while (EnvironmentLoader.I.IsSwitching())
            {
                yield return new WaitForSecondsRealtime(.2f);
            }
            modifiersLoaded = true;
            defaultSkyboxColor = RenderSettings.skybox.GetColor("_Tint");
        }

        private static void EnableAutoLightshow(bool enable)
        {
            if (Integrations.autoLightshowFound)
            {
                if (AutoLightshowMod.IsEnabled)
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
            DebugWarningPopup(text, speed, -1);
        }

        public static void DebugWarningPopup(string text, float speed, int tick)
        {
            popupTextDictionary.Add(tick, KataConfig.I.CreateDebugText(text, debugTextPosition, 4f, null, false, speed));
        }

        public static void DebugTextPopup(string text, float size, Vector3 position, bool glow, bool faceForward, float tick)
        {
            DebugTextPopup textpopup = KataConfig.I.CreateDebugText(text, position, size, null, glow, .001f);

            if(faceForward)
                textpopup.transform.forward = Vector3.forward;

            popupTextDictionary.Add(tick, textpopup);
        }

        public static void DestroyPopup(float tick, bool destroyAll = false)
        {
            if (destroyAll)
            {
                foreach(KeyValuePair<float, DebugTextPopup> entry in popupTextDictionary)
                {
                    GameObject.Destroy(entry.Value.gameObject);
                }
                popupTextDictionary.Clear();
                return;
            }
            if(popupTextDictionary != null)
            {
                if (popupTextDictionary.ContainsKey(tick))
                {
                    GameObject.Destroy(popupTextDictionary[tick].gameObject);
                    popupTextDictionary.Remove(tick);
                }
                   
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

        public static void OnRestart(bool ingameRestart = false)
        {
            if (endless) return;
            if (!Config.enabled) return;
            if (!modifiersFound) return;
            
            if (ingameRestart)
            {
                Reset();
            }
            else
            {
                ResetValues();
            }
            LoadModifierCues(true);
        }

        public static void Reset(bool fromBack = false)
        {
            if (!Config.enabled) return;
            if (!modifiersFound) return;
            modifiersFound = false;
            ResetValues(fromBack);
            DestroyPopup(0, true);
            if (!endless)
            {
                oldColorsSet = false;
                oldArenaSet = false;
            }
            zOffsetList.Clear();
            oldOffsetDict.Clear();
            if(Integrations.autoLightshowFound) EnableAutoLightshow(true);
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
            if (!fromBack)
            {
                foreach (Modifier mod in awaitDisableModifiers)
                {
                    if(mod.Type != ModifierType.ColorChange && mod.Type != ModifierType.ColorUpdate)
                        mod.Deactivate();
                }
                   
                foreach (Modifier mod in preloadModifiers) mod.Deactivate();
                foreach (Modifier mod in singleUseModifiers)
                {
                    if (mod.Type != ModifierType.ColorChange && mod.Type != ModifierType.ColorUpdate)
                        mod.Deactivate();
                }
                foreach (ModifierQueueItem item in modifierQueue)
                {
                    if (item.modifier.Type != ModifierType.ColorChange && item.modifier.Type != ModifierType.ColorUpdate)
                        item.modifier.Deactivate();
                }

                if (oldColorsSet)
                {
                    //new ColorChange(ModifierType.ColorChange, 0, 0, new float[] { 0f, 0f, 0f }, new float[] { 0f, 0f, 0f }).UpdateColors(oldLeftHandColor, oldRightHandColor);
                    new ColorChange() { }.UpdateColors(oldLeftHandColor, oldRightHandColor);
                }
            }
            
            modifierQueue.Clear();
            awaitDisableModifiers.Clear();
            awaitEnableModifiers.Clear();
            preloadModifiers.Clear();
            singleUseModifiers.Clear();
            autoLightings.Clear();
            activePsychedelia = null;
            activeColorChange = null;
            GameObject go = GameObject.Find("World");
            if(go != null)
            {
                Transform world = go.transform;
                world.transform.position = Vector3.zero;
                world.transform.eulerAngles = Vector3.zero;
                world.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            ResetArena();
            
        }

        private static void ResetArena(bool forceReset = false)
        {
            RenderSettings.skybox.SetColor("_Tint", defaultSkyboxColor);
            ApiController.TurnOff();
            SkyboxControl skyboxControl = GameObject.FindObjectOfType<SkyboxControl>();
            if (skyboxControl != null) skyboxControl.enabled = true;
            if (Integrations.arenaLoaderFound)
            {
                if (oldArena.Length > 0 && oldArenaSet)
                {
                    if(!endless || forceReset)
                    {
                        if (oldArena != PlayerPreferences.I.Environment.Get())
                        {
                            PlayerPreferences.I.Environment.Set(oldArena);
                            EnvironmentLoader.I.SwitchEnvironment();
                        }
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
            /*if (Input.GetKeyDown(KeyCode.L))
            {
                ApiController.isRunning = true;
                MelonCoroutines.Start(ApiController.StartPost());
                //ApiController.PostAsync(255, 255, 255, 255);
            }
           if (Input.GetKeyDown(KeyCode.L))
            {
                AutoPlayer.EnableAutoplayer(!AutoPlayer.I.IsAutoPlayerEnabled);
                MelonLogger.Msg("Auto player is " + (AutoPlayer.I.IsAutoPlayerEnabled ? "enabled" : "disabled"));                
                
            }
            if (Input.GetKeyDown(KeyCode.P) && MenuState.sState == MenuState.State.Launched)
            {
                InGameUI.I.GoToPausePage(true);
            }*/

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
                    if (modifierQueue[0].action == Action.Activate)
                    {
                        modifierQueue[0].modifier.Activate();
                    }
                    else
                    {
                        modifierQueue[0].modifier.Deactivate();
                    }

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














































































