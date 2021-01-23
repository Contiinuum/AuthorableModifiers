using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using ArenaLoader;

namespace AuthorableModifiers
{
    public class AutoLighting : Modifier
    {
        private const float intensityNormExp = .5f;
        private const float intensityWeight = 1.5f;
        private const float threshholdIncrement = .225f;

        private bool pulseMode;

        private object faderToken;
        private object fadeToBlackToken;
        private float fadeToBlackStartTick;
        private float fadeToBlackEndTick;
        private float fadeToBlackExposure;
        private float fadeToBlackReflection;
        private object lightshowToken;
        private object psyToken;

        private float lastPsyTimer = 0f;
        private float defaultPsychadeliaPhaseSeconds = 14.28f;
        private float psychadeliaTimer = 0.0f;

        private float maxBrightness;
        private float originalMaxBrightness;
        private float fadeOutTime = 240f;
        private float mapIntensity;

        private  int startIndex = 0;
        private static List<BrightnessEvent> brightnessEvents = new List<BrightnessEvent>();
        private static List<PsyEvent> psyEvents = new List<PsyEvent>();

        public AutoLighting(ModifierType _type, float _startTick, float _endTick, float _maxBrightness, bool _pulseMode)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            pulseMode = _pulseMode;
            originalMaxBrightness = _maxBrightness;
        }

        public override void Activate()
        {
            base.Activate();
            StartLightshow();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            StopLightshow();
        }

        public void StartLightshow()
        {
            if (!Integrations.arenaLoaderFound || !Config.enabled) return;
            StopLightshow();
            maxBrightness = AuthorableModifiersMod.defaultArenaBrightness * originalMaxBrightness;
            
            List<SongCues.Cue> cues = SongCues.I.mCues.cues.ToList();
            float tick = AudioDriver.I.mCachedTick;
            mapIntensity = CalculateIntensity(cues.First().tick, cues.Last().tick, cues.ToList());
            for (int i = cues.Count - 1; i >= 0; i--)
            {
                if (cues[i].tick < tick) cues.RemoveAt(i);               
                else if (cues[i].behavior == Target.TargetBehavior.Dodge) cues.RemoveAt(i);
            }
            
            active = true;
            Task.Run(() => PrepareLightshow(cues));

            if (pulseMode)
            {
                fadeToBlackStartTick = AudioDriver.I.mCachedTick;
                fadeToBlackEndTick = fadeToBlackStartTick + (fadeOutTime / mapIntensity);
                fadeToBlackExposure = RenderSettings.skybox.GetFloat("_Exposure");
                fadeToBlackReflection = RenderSettings.reflectionIntensity;
                fadeToBlackToken = MelonCoroutines.Start(FadeToBlack());
            }

        }

        public void StopLightshow()
        {
            if (!Integrations.arenaLoaderFound || !Config.enabled) return;
            MelonCoroutines.Stop(faderToken);
            MelonCoroutines.Stop(lightshowToken);
            MelonCoroutines.Stop(fadeToBlackToken);
            MelonCoroutines.Stop(psyToken);
        }

        private float CalculateIntensity(float startTick, float endTick, List<SongCues.Cue> cues)
        {
            float intensity = 0f;
            bool indexSet = false;
            for (int i = startIndex; i < cues.Count; i++)
            {
                SongCues.Cue cue = SongCues.I.mCues.cues[i];
                if (cue.tick >= endTick) break;
                if (cue.tick >= startTick && cue.tick < endTick)
                {
                    intensity += GetTargetAmount((Hitsound)cue.velocity, cue.behavior);
                    if (!indexSet)
                    {
                        indexSet = true;
                        startIndex = i;
                    }
                }

            }

            intensity /= AudioDriver.TickSpanToMs(SongDataHolder.I.songData, startTick, endTick);

            intensity *= 1000f;
            return intensity;
        }

        private async Task PrepareFade(float targetExposure)
        {
            float oldExposure = RenderSettings.skybox.GetFloat("_Exposure");
            float oldReflection = RenderSettings.reflectionIntensity;
            ArenaLoaderMod.CurrentSkyboxExposure = oldExposure;
            float startTick = AudioDriver.I.mCachedTick;
            float _endTick = startTick + 960f;
            float percentage = 0f;
            while (percentage < 100f)
            {
                percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (_endTick - startTick);
                float currentExp = Mathf.Lerp(oldExposure, targetExposure, percentage / 100f);
                float currentRef = Mathf.Lerp(oldReflection, targetExposure, percentage / 100f);
                RenderSettings.skybox.SetFloat("_Exposure", currentExp);
                ArenaLoaderMod.CurrentSkyboxReflection = 0f;
                ArenaLoaderMod.ChangeReflectionStrength(currentRef);
                ArenaLoaderMod.CurrentSkyboxExposure = currentExp;
            }
            await Task.CompletedTask;
        }

        private async void PrepareLightshow(List<SongCues.Cue> cues)
        {
            float oldExposure = RenderSettings.skybox.GetFloat("_Exposure");
            brightnessEvents.Clear();
            psyEvents.Clear();
            ArenaLoaderMod.CurrentSkyboxExposure = oldExposure;
            if (!pulseMode)
            {
                //await PrepareFade(maxBrightness * .5f);
                await PrepareAsync(cues);
                //MelonCoroutines.Stop(faderToken);
            }
            else
            {
                await PreparePulse(cues);
            }
            brightnessEvents.Sort((cue1, cue2) => cue1.startTick.CompareTo(cue2.startTick));
            lightshowToken = MelonCoroutines.Start(BetterLightshow());
        }

        private async Task PreparePulse(List<SongCues.Cue> cues)
        {
            foreach (SongCues.Cue cue in cues)
            {
                if (cue.behavior == Target.TargetBehavior.Dodge) continue;
                float amount = GetTargetAmount((Hitsound)cue.velocity, cue.behavior) * 2f;
                //fadeToBlackStartTick = AudioDriver.I.mCachedTick;
                //fadeToBlackEndTick = fadeToBlackStartTick + (fadeOutTime / mapIntensity);
                float end = cue.tick + (fadeOutTime / mapIntensity);
                //float curr = RenderSettings.skybox.GetFloat("_Exposure") + amount;
                //float newAmount = curr > maxBrightness ? maxBrightness : curr;
                //fadeToBlackExposure = newAmount;
                //fadeToBlackReflection = newAmount;
                brightnessEvents.Add(new BrightnessEvent(amount, cue.tick, end));
                if(cue.nextCue != null)
                {
                    if(psyEvents.Count == 0) PreparePsychedelia(cue);
                    else if (psyEvents.Last().startTick != cue.tick) PreparePsychedelia(cue);
                }
               
            }
            await Task.CompletedTask;
        }

        private void PreparePsychedelia(SongCues.Cue cue)
        {
            if (Config.enablePsychedelia)
            {
                if (cue.behavior == Target.TargetBehavior.Melee || cue.behavior == Target.TargetBehavior.ChainStart || cue.behavior == Target.TargetBehavior.Hold)
                {
                    List<float> ticks = new List<float>();
                    if (cue.behavior == Target.TargetBehavior.ChainStart) LookForEndOfChain(cue, ticks);
                    else LookForLastBehavior(cue, new Target.TargetBehavior[] { Target.TargetBehavior.Melee }, ticks);

                    if (cue.behavior == Target.TargetBehavior.Hold && cue.tickLength >= 1920f)
                    {
                        //MelonCoroutines.Stop(psyToken);
                        //psyToken = MelonCoroutines.Start(DoPsychedelia(AudioDriver.I.mCachedTick + cue.tickLength - 960f));
                        psyEvents.Add(new PsyEvent(cue.tick, cue.tick + cue.tickLength - 960f));
                    }
                    else if (ticks[0] - cue.tick >= 1920f && cue.behavior == Target.TargetBehavior.Melee)
                    {
                        //MelonCoroutines.Stop(psyToken);
                        //psyToken = MelonCoroutines.Start(DoPsychedelia(ticks[0] - 960f));
                        psyEvents.Add(new PsyEvent(cue.tick, ticks[0] - 960f));
                    }
                }
            }
        }

        private async Task PrepareAsync(List<SongCues.Cue> cues)
        {
            float offset = cues[0].tick % 120;
            float span = 7680f;
            float sectionStart = cues[0].tick;
            float sectionEnd = sectionStart + span;
            float previousSectionBrightness = maxBrightness * .5f;
            ExposureState expState = ExposureState.Dark;

            List<Section> sections = new List<Section>();
            int numSections = Mathf.FloorToInt(cues.Last().tick / span) + 1;
            startIndex = 0;
            for (int i = 0; i < numSections; i++)
            {
                CalculateSections(cues, sections, sectionStart, sectionEnd, threshholdIncrement);
                sectionStart = sectionEnd;
                sectionEnd += span;
            }
            startIndex = 0;
            sections.Sort((section1, section2) => section1.start.CompareTo(section2.start));
            foreach (Section section in sections)
            {
                float sectionBrightnessSum = 0f;
                List<SongCues.Cue> sectionCues = new List<SongCues.Cue>();

                float sectionBrightness = 0f;
                float sectionTargetBrightness = GetSectionTargetBrightness(expState, section.intensity);
                foreach (SongCues.Cue cue in cues)
                {
                    if (cue.tick > section.end) break;
                    if (cue.tick >= section.start && cue.tick < section.end)
                    {
                        sectionCues.Add(cue);
                        sectionBrightness += GetTargetAmount((Hitsound)cue.velocity, cue.behavior);
                    }
                }
                if (sectionBrightness != 0f && sectionCues.Count > 0f)
                {
                    sectionCues.Sort((cue1, cue2) => cue1.tick.CompareTo(cue2.tick));
                    foreach (SongCues.Cue cue in sectionCues)
                    {
                        if (cue.nextCue is null) break;
                        if (cue.behavior == Target.TargetBehavior.Dodge) continue;
                        sectionBrightnessSum += ((GetTargetAmount((Hitsound)cue.velocity, cue.behavior) / sectionBrightness) * (sectionTargetBrightness - previousSectionBrightness));
                        float brightness = previousSectionBrightness + sectionBrightnessSum;
                        float end = cue.tick + 240f;
                        if (brightnessEvents.Count > 0)
                        {
                            if (brightnessEvents.Last().startTick != cue.tick)
                            {
                                brightnessEvents.Add(new BrightnessEvent(brightness, cue.tick, cue.nextCue.tick));
                                PreparePsychedelia(cue);
                            }
                            else
                            {
                                BrightnessEvent be = brightnessEvents.Last();
                                be.brightness = brightness;
                                if (cue.nextCue.tick > be.endTick && cue.nextCue.behavior != Target.TargetBehavior.Dodge)
                                {
                                    be.endTick = end;
                                }
                                brightnessEvents.RemoveAt(brightnessEvents.Count - 1);
                                brightnessEvents.Add(be);
                            }
                        }
                        else
                        {
                            brightnessEvents.Add(new BrightnessEvent(brightness, cue.tick, cue.nextCue.tick));
                            PreparePsychedelia(cue);
                        }

                    }
                    expState = expState == ExposureState.Light ? ExposureState.Dark : ExposureState.Light;
                    previousSectionBrightness = sectionTargetBrightness;
                }

            }
            brightnessEvents.Sort((event1, event2) => event1.startTick.CompareTo(event2.startTick));
            await Task.CompletedTask;
        }

        private void CalculateSections(List<SongCues.Cue> cues, List<Section> sections, float start, float end, float threshhold, float startIndex = 0)
        {
            float intensity = CalculateIntensity(start, end, cues);
            intensity = intensity / Mathf.Pow(mapIntensity, intensityNormExp);
            intensity = (float)Math.Tanh(intensityWeight * intensity);
            float span = end - start;
            if (span > 480f)
            {
                if (threshhold >= (float)Math.Tanh(intensityWeight * intensity))
                {
                    sections.Add(new Section(start, end, intensity));
                    return;
                }
                else
                {
                    threshhold += threshholdIncrement;
                    CalculateSections(cues, sections, start, end - span / 2, threshhold, startIndex);
                    CalculateSections(cues, sections, end - span / 2, end, threshhold, startIndex);
                }
            }
            else
            {
                sections.Add(new Section(start, end, intensity));
            }
        }

        private float GetSectionTargetBrightness(ExposureState state, float intensity)
        {
            float sign = state == ExposureState.Light ? -1 : 1;
            return (float)(.5f + sign * Math.Tanh(intensityWeight * intensity) / 2) * maxBrightness;
        }

        private IEnumerator BetterLightshow()
        {

            //List<SongCues.Cue> cues = SongCues.I.mCues.cues.ToList();
            while (active)
            {
                if (psyEvents.Count > 0)
                {
                    if (psyEvents[0].startTick <= AudioDriver.I.mCachedTick)
                    {
                        MelonCoroutines.Stop(psyToken);
                        psyToken = MelonCoroutines.Start(DoPsychedelia(psyEvents[0].startTick, psyEvents[0].endTick));
                        psyEvents.RemoveAt(0);
                    }
                }

                if (brightnessEvents.Count == 0) yield break;
                if (brightnessEvents[0].startTick <= AudioDriver.I.mCachedTick)
                {
                    if (!pulseMode)
                    {
                        MelonCoroutines.Stop(faderToken);
                        faderToken = MelonCoroutines.Start(BetterFade(brightnessEvents[0].startTick, brightnessEvents[0].endTick, brightnessEvents[0].brightness));
                    }
                    else
                    {
                        fadeToBlackStartTick = brightnessEvents[0].startTick;
                        fadeToBlackEndTick = fadeToBlackStartTick + (fadeOutTime / mapIntensity);
                        float curr = RenderSettings.skybox.GetFloat("_Exposure") + brightnessEvents[0].brightness;
                        float newAmount = curr > maxBrightness ? maxBrightness : curr;
                        float newReflection = newAmount / maxBrightness;
                        newReflection = .5f + (newAmount * newReflection);
                        fadeToBlackExposure = newAmount;
                        fadeToBlackReflection = newReflection;
                    }
                    brightnessEvents.RemoveAt(0);
                }
                yield return new WaitForSecondsRealtime(.02f);
            }
        }

        private float GetTargetAmount(Hitsound hitsound, Target.TargetBehavior behavior)
        {
            float amount = 0.1f;
            switch (hitsound)
            {
                case Hitsound.ChainNode:
                    amount = (maxBrightness / 100f) * 5f;
                    break;
                case Hitsound.ChainStart:
                    amount = (maxBrightness / 100f) * 15f;
                    break;
                case Hitsound.Kick:
                    amount = (maxBrightness / 100f) * 25f;
                    break;
                case Hitsound.Snare:
                    amount = (maxBrightness / 100f) * 40f;
                    break;
                case Hitsound.Percussion:
                    amount = (maxBrightness / 100f) * 60f;
                    break;
                case Hitsound.Melee:
                    amount = (maxBrightness / 100f) * 80f;
                    break;
                default:
                    break;
            }
            if (behavior == Target.TargetBehavior.Melee && hitsound != Hitsound.Melee) amount = (maxBrightness / 100f) * 80f;
            return amount * Config.intensity * .5f;
        }

        private void LookForEndOfChain(SongCues.Cue cue, List<float> ticks)
        {
            if (cue.nextCue is null)
            {
                ticks.Add(cue.tick);
                return;
            }

            if (cue.nextCue.behavior == Target.TargetBehavior.Chain)
            {
                LookForEndOfChain(cue.nextCue, ticks);
                return;
            }

            ticks.Add(cue.nextCue.tick);
        }

        private void LookForLastBehavior(SongCues.Cue cue, Target.TargetBehavior[] behaviors, List<float> ticks)
        {
            if (cue.nextCue is null)
            {
                ticks.Add(cue.tick);
                return;
            }

            for (int i = 0; i < behaviors.Length; i++)
            {
                if (cue.nextCue.behavior == behaviors[i])
                {
                    if (cue.behavior == Target.TargetBehavior.Hold && cue.nextCue.behavior == Target.TargetBehavior.Hold && cue.tick == cue.nextCue.tick) continue;
                    if (cue.behavior == Target.TargetBehavior.Melee && cue.nextCue.behavior == Target.TargetBehavior.Melee && cue.tick == cue.nextCue.tick) continue;
                    LookForLastBehavior(cue.nextCue, behaviors, ticks);
                    return;
                }
            }

            ticks.Add(cue.nextCue.tick);
        }

        private IEnumerator BetterFade(float startTick, float endTick, float targetExposure)
        {

            float oldExposure = RenderSettings.skybox.GetFloat("_Exposure");
            float oldReflection = RenderSettings.reflectionIntensity;
            float targetReflection = targetExposure / maxBrightness;
            targetReflection = .5f + (targetExposure * targetReflection);
            ArenaLoaderMod.CurrentSkyboxExposure = oldExposure;
            //float startTick = AudioDriver.I.mCachedTick;
            //targetExposure += oldExposure;
            float percentage = 0f;
            while (percentage < 100f)
            {
                percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (endTick - startTick);
                float currentExp = Mathf.Lerp(oldExposure, targetExposure, percentage / 100f);
                float currentRef = Mathf.Lerp(oldReflection, targetReflection, percentage / 100f);
                RenderSettings.skybox.SetFloat("_Exposure", currentExp);
                ArenaLoaderMod.CurrentSkyboxReflection = 0f;
                ArenaLoaderMod.ChangeReflectionStrength(currentRef);
                ArenaLoaderMod.CurrentSkyboxExposure = currentExp;
                yield return new WaitForSecondsRealtime(.02f);
            }

        }

        private IEnumerator FadeToBlack()
        {
            ArenaLoaderMod.CurrentSkyboxExposure = fadeToBlackExposure;
            while (true)
            {
                float percentage = ((AudioDriver.I.mCachedTick - fadeToBlackStartTick) * 100f) / (fadeToBlackEndTick - fadeToBlackStartTick);
                if (percentage >= 0)
                {
                    float currentExp = Mathf.Lerp(fadeToBlackExposure, 0f, percentage / 100f);
                    float currentRef = Mathf.Lerp(fadeToBlackReflection, .5f, percentage / 100f);
                    RenderSettings.skybox.SetFloat("_Exposure", currentExp);
                    ArenaLoaderMod.CurrentSkyboxReflection = 0f;
                    ArenaLoaderMod.ChangeReflectionStrength(currentRef);
                    ArenaLoaderMod.CurrentSkyboxExposure = currentExp;
                }
                yield return new WaitForSecondsRealtime(.01f);
            }
        }

        private IEnumerator DoPsychedelia(float start, float end)
        {
            psychadeliaTimer = lastPsyTimer;
            while (active)
            {
                float amount = SongDataHolder.I.songData.GetTempo(start) / 50f;
                float phaseTime = defaultPsychadeliaPhaseSeconds / amount;

                if (psychadeliaTimer <= phaseTime)
                {

                    psychadeliaTimer += Time.deltaTime;

                    float forcedPsychedeliaPhase = psychadeliaTimer / phaseTime;
                    GameplayModifiers.I.mPsychedeliaPhase = forcedPsychedeliaPhase;
                }
                else
                {
                    psychadeliaTimer = 0;
                }
                if (AudioDriver.I.mCachedTick > end)
                {
                    lastPsyTimer = psychadeliaTimer;
                    psychadeliaTimer = 0;
                    yield break;
                }

                yield return new WaitForSecondsRealtime(0.02f);
            }
        }

        private enum ExposureState
        {
            Dark,
            Light
        }

        private enum Hitsound
        {
            ChainStart = 1,
            ChainNode = 2,
            Melee = 3,
            Kick = 20,
            Percussion = 60,
            Snare = 127
        }

        public struct BrightnessEvent
        {
            public float brightness;
            public float startTick;
            public float endTick;

            public BrightnessEvent(float _brightness, float _startTick, float _endTick)
            {
                brightness = _brightness;
                startTick = _startTick;
                endTick = _endTick;
            }
        }

        public struct PsyEvent
        {
            public float startTick;
            public float endTick;

            public PsyEvent(float _startTick, float _endTick)
            {
                startTick = _startTick;
                endTick = _endTick;
            }
        }

        public struct Section
        {
            public float start;
            public float end;
            public float intensity;

            public Section(float _start, float _end, float _intensity)
            {
                start = _start;
                end = _end;
                intensity = _intensity;
            }
        }
    }

}

