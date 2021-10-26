using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Hmx.Audio;
using UnityEngine;

namespace AuthorableModifiers
{
    internal static class Hooks
    {
        public static bool updateChainColor = false;
        public static bool hideTeles = false;

#pragma warning disable IDE0051, IDE0060

        [HarmonyPatch(typeof(Target), "InitFromSpawner", new Type[] { typeof(TargetSpawner.SpawnInfo), typeof(SongCues.Cue) })]
        private static class PatchTargetInit
        {
            private static void Prefix(Target __instance, TargetSpawner.SpawnInfo info, SongCues.Cue cue)
            {
                if (!AuthorableModifiersMod.modifiersFound) return;
                if (!updateChainColor) return;
                if (cue.behavior != Target.TargetBehavior.Chain) return;
                if (cue.handType == Target.TargetHandType.Left)
                {
                    __instance.chainLine.startColor = PlayerPreferences.I.GunColorLeft.Get() / 2;
                    __instance.chainLine.endColor = PlayerPreferences.I.GunColorLeft.Get() / 2;
                }
                else
                {
                    __instance.chainLine.startColor = PlayerPreferences.I.GunColorRight.Get() / 2;
                    __instance.chainLine.endColor = PlayerPreferences.I.GunColorRight.Get() / 2;
                }
            }
        }

        [HarmonyPatch(typeof(SongCues), "LoadCues")]
        private static class PatchLoadCues
        {
            private static void Postfix(SongCues __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiersMod.modifiersFound) return;
                if (TutorialFlow.I.mIsTutorialSong) return;
                AuthorableModifiersMod.ApplyZOffset();
                //AuthorableModifiers.LoadModifierCues();
            }
        }


        [HarmonyPatch(typeof(SongSelectItem), "OnSelect")]
        private static class PatchOnSelect
        {
            private static void Postfix(SongSelectItem __instance)
            {
                //Set filePath
                if (AuthorableModifiersMod.endless) return;
                AuthorableModifiersMod.audicaFilePath = __instance.mSongData.foundPath;
                AuthorableModifiersMod.LoadModifierCues();
                //MelonLoader.MelonLogger.Msg(__result.zipPath);
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "ShowPage", new Type[] { typeof(OptionsMenu.Page) })]
        private static class PatchShowPage
        {
            private static void Postfix(OptionsMenu __instance, OptionsMenu.Page page)
            {
                if (page == OptionsMenu.Page.Main && !AuthorableModifiersMod.modifiersFound)
                    AuthorableModifiersMod.SetUserBrightness(RenderSettings.skybox.GetFloat("_Exposure"), RenderSettings.skybox.GetFloat("_Rotation"), RenderSettings.reflectionIntensity);
            }
        }

        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State)})]
        private static class PatchMenuStateSetState
        {
            private static void Postfix(MenuState __instance, MenuState.State state)
            {
                if(state == MenuState.State.SongPage)
                {
                    
                    AuthorableModifiersMod.SetEndlessActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(LaunchPanel), "Back")]
        private static class PatchLaunchPanelBack
        {
            private static void Postfix(LaunchPanel __instance)
            {
                //Set filePath
                AuthorableModifiersMod.SetEndlessActive(false);
                AuthorableModifiersMod.Reset(true);
                //MelonLoader.MelonLogger.Msg(__result.zipPath);
            }
        }

        [HarmonyPatch(typeof(InGameUI), "Restart")]
        private static class PatchRestart
        {
            private static void Prefix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiersMod.modifiersFound) return;
                AuthorableModifiersMod.OnRestart(true);
            }
        }

        [HarmonyPatch(typeof(InGameUI), "ReturnToSongList")]
        private static class PatchReturnToSongList
        {
            private static void Postfix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiersMod.modifiersFound) return;
                AuthorableModifiersMod.SetEndlessActive(false);
                AuthorableModifiersMod.Reset();
            }
        }

        [HarmonyPatch(typeof(InGameUI), "GoToFailedPage")]
        private static class PatchGoToFailedPage
        {
            private static void Postfix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiersMod.modifiersFound) return;
                AuthorableModifiersMod.OnRestart();
            }
        }

        [HarmonyPatch(typeof(InGameUI), "GoToResultsPage")]
        private static class PatchGoToResultsPage
        {
            private static void Postfix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiersMod.modifiersFound) return;
                if (AuthorableModifiersMod.endless) return;
                AuthorableModifiersMod.OnRestart();
            }
        }

        [HarmonyPatch(typeof(Telegraph), "Init", new Type[] { typeof(SongCues.Cue), typeof(float) })]
        private static class PatchInit
        {
            private static void Postfix(Telegraph __instance, SongCues.Cue cue, float animationSpeed)
            {
                if (!hideTeles) return;
                if (cue.behavior == Target.TargetBehavior.Melee || cue.behavior == Target.TargetBehavior.Dodge) return;
                __instance.circleMesh.enabled = false;
                __instance.cloud.enabled = false;
            }
        }

        [HarmonyPatch(typeof(ScoreKeeper), "GetScoreValidity")]
        private static class PatchGetScoreValidity
        {
            private static bool Prefix(ScoreKeeper __instance, ref ScoreKeeper.ScoreValidity __result)
            {
                if (!AuthorableModifiersMod.modifiersFound) return false;
                else
                {
                    __result = ScoreKeeper.ScoreValidity.Valid;
                    __instance.mHasInvalidatedScore = false;
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(EnvironmentLoader), "SwitchEnvironment")]
        private static class PatchSwitchEnvironment
        {
            private static void Postfix(EnvironmentLoader __instance)
            {
                if (!Config.enabled) return;
                 MelonLoader.MelonCoroutines.Start(AuthorableModifiersMod.ISetDefaultArenaBrightness());
                
                   
            }
        }

        [HarmonyPatch(typeof(LaunchPanel), "Play")]
        private static class PatchPlay
        {
            private static void Postfix(LaunchPanel __instance)
            {
                if (!Config.enabled || !AuthorableModifiersMod.modifiersFound) return;
                AuthorableModifiersMod.DestroyPopup(-1,true);
            }
        }
    }
}