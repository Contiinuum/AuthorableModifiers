using Harmony;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Hmx.Audio;
using UnityEngine;

namespace AudicaModding
{
    internal static class Hooks
    {
        public static bool updateChainColor = false;
        public static bool hideTeles = false;


        [HarmonyPatch(typeof(Target), "InitFromSpawner", new Type[] { typeof(TargetSpawner.SpawnInfo), typeof(SongCues.Cue) })]
        private static class PatchTargetInit
        {
            private static void Prefix(Target __instance, TargetSpawner.SpawnInfo info, SongCues.Cue cue)
            {
                if (!AuthorableModifiers.modifiersFound) return;
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
                AuthorableModifiers.ApplyZOffset();
                //AuthorableModifiers.LoadModifierCues();
            }
        }


        [HarmonyPatch(typeof(SongSelectItem), "OnSelect")]
        private static class PatchOnSelect
        {
            private static void Postfix(SongSelectItem __instance)
            {
                //Set filePath
                AuthorableModifiers.audicaFilePath = __instance.mSongData.foundPath;
                AuthorableModifiers.SetUserBrightness(RenderSettings.skybox.GetFloat("_Exposure"), RenderSettings.skybox.GetFloat("_Rotation"));
                AuthorableModifiers.LoadModifierCues();
                //MelonLoader.MelonLogger.Log(__result.zipPath);
            }
        }

        [HarmonyPatch(typeof(LaunchPanel), "Back")]
        private static class PatchLaunchPanelBack
        {
            private static void Postfix(LaunchPanel __instance)
            {
                //Set filePath
                AuthorableModifiers.Reset();
                //MelonLoader.MelonLogger.Log(__result.zipPath);
            }
        }

        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class PatchSetMenuState
        {
            private static void Prefix(MenuState __instance, ref MenuState.State state)
            {
                //if (state != MenuState.State.Launched && MenuState.sLastState == MenuState.State.Launched) AuthorableModifiers.Reset();
            }
        }

        [HarmonyPatch(typeof(InGameUI), "Restart")]
        private static class PatchRestart
        {
            private static void Prefix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiers.modifiersFound) return;
                AuthorableModifiers.OnRestart();
            }
        }

        [HarmonyPatch(typeof(InGameUI), "ReturnToSongList")]
        private static class PatchReturnToSongList
        {
            private static void Postfix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiers.modifiersFound) return;
                AuthorableModifiers.Reset();
            }
        }

        [HarmonyPatch(typeof(InGameUI), "GoToFailedPage")]
        private static class PatchGoToFailedPage
        {
            private static void Postfix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiers.modifiersFound) return;
                AuthorableModifiers.OnRestart();
            }
        }

        [HarmonyPatch(typeof(InGameUI), "GoToResultsPage")]
        private static class PatchGoToResultsPage
        {
            private static void Postfix(InGameUI __instance)
            {
                if (KataConfig.I.practiceMode) return;
                if (!AuthorableModifiers.modifiersFound) return;
                AuthorableModifiers.OnRestart();
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
                if (!AuthorableModifiers.modifiersFound) return false;
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
                if(MenuState.sState == MenuState.State.SettingsPage)
                    MelonLoader.MelonCoroutines.Start(AuthorableModifiers.ISetDefaultArenaBrightness());
            }
        }
    }
}