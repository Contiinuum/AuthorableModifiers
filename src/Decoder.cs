using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using SimpleJSON;
using MelonLoader;
using UnityEngine;

namespace AuthorableModifiers
{
    public static class Decoder
    {
        public static List<Modifier> GetModifierCues(string audicaFilePath)
        {
            List<Modifier> modifiers = new List<Modifier>();
            //float _endTick = SongCues.I.GetLastCueEndTick();
            using (FileStream audicaFile = new FileStream(audicaFilePath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(audicaFile, ZipArchiveMode.Read))
                {
                    bool modifierCuesFound = false;
                    foreach(ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name == "modifiers.json")
                        {
                            modifierCuesFound = true;
                            //MelonLogger.Msg("Loading Modifiers...");
                            break;
                        }
                    }
                    if (!modifierCuesFound) return null;
                    using (Stream stream = archive.GetEntry("modifiers.json").Open())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string data = reader.ReadToEnd();
                            var modifiersJSON = JSON.Parse(data);
                            if (modifiersJSON["modifiers"].Count == 0)
                            {
                                MelonLogger.Msg("Couldn't read file.");
                                return null;
                            }
                            for (int i = 0; i < modifiersJSON["modifiers"].Count; i++)
                            {
                                float p = 100;

                                ModifierType type = (ModifierType) Enum.Parse(typeof(ModifierType), modifiersJSON["modifiers"][i]["type"]);
                                float startTick = modifiersJSON["modifiers"][i]["startTick"];
                                float endTick = modifiersJSON["modifiers"][i]["endTick"];
                                float amount = modifiersJSON["modifiers"][i]["amount"];
                                float amountPercentage = amount / p;
                                Color leftHandColor = new Color(modifiersJSON["modifiers"][i]["leftHandColor"][0], modifiersJSON["modifiers"][i]["leftHandColor"][1], modifiersJSON["modifiers"][i]["leftHandColor"][2]);
                                Color rightHandColor = new Color(modifiersJSON["modifiers"][i]["rightHandColor"][0], modifiersJSON["modifiers"][i]["rightHandColor"][1], modifiersJSON["modifiers"][i]["rightHandColor"][2]);
                                string value1 = modifiersJSON["modifiers"][i]["value1"];
                                string value2 = modifiersJSON["modifiers"][i]["value2"];
                                bool option1 = modifiersJSON["modifiers"][i]["option1"];
                                bool option2 = modifiersJSON["modifiers"][i]["option2"];
                                bool independantBool = modifiersJSON["modifiers"][i]["independantBool"];
                                string xOffset = modifiersJSON["modifiers"][i]["xoffset"];
                                string yOffset = modifiersJSON["modifiers"][i]["yoffset"];
                                string zOffset = modifiersJSON["modifiers"][i]["zoffset"];

                                Modifier modifierCue = null;
                                bool preload = false;
                                switch (type)
                                {
                                    case ModifierType.AimAssist:
                                        modifierCue = new AimAssistChange()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            Amount = amountPercentage
                                        };
                                        /*modifierCue = new AimAssistChange(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p
                                            );*/
                                        break;
                                    case ModifierType.ColorChange:
                                        if (!Config.enableColorChange) continue;
                                        modifierCue = new ColorChange()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            LeftHandColor = leftHandColor,
                                            RightHandColor = rightHandColor
                                        };
                                        /*
                                        modifierCue = new ColorChange(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            new float[] 
                                            {
                                                modifiersJSON["modifiers"][i]["leftHandColor"][0],
                                                modifiersJSON["modifiers"][i]["leftHandColor"][1],
                                                modifiersJSON["modifiers"][i]["leftHandColor"][2]
                                            },
                                            new float[]
                                            {
                                                modifiersJSON["modifiers"][i]["rightHandColor"][0],
                                                modifiersJSON["modifiers"][i]["rightHandColor"][1],
                                                modifiersJSON["modifiers"][i]["rightHandColor"][2]
                                            }
                                            );*/
                                        break;
                                    case ModifierType.ColorUpdate:
                                        if (!Config.enableColorChange) continue;
                                        modifierCue = new ColorUpdate()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            LeftHandColor = leftHandColor,
                                            RightHandColor = rightHandColor,
                                            IsSingleUse = true,
                                        };
                                        /*
                                        modifierCue = new ColorUpdate(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            new float[]
                                            {
                                                modifiersJSON["modifiers"][i]["leftHandColor"][0],
                                                modifiersJSON["modifiers"][i]["leftHandColor"][1],
                                                modifiersJSON["modifiers"][i]["leftHandColor"][2]
                                            },
                                            new float[]
                                            {
                                                modifiersJSON["modifiers"][i]["rightHandColor"][0],
                                                modifiersJSON["modifiers"][i]["rightHandColor"][1],
                                                modifiersJSON["modifiers"][i]["rightHandColor"][2]
                                            }
                                            );*/
                                        break;
                                    case ModifierType.ColorSwap:
                                        if (!Config.enableColorChange) continue;
                                        modifierCue = new ColorSwap()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick
                                        };
                                        /*
                                        modifierCue = new ColorSwap(type,
                                           modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"]
                                           );*/
                                        break;
                                    case ModifierType.HiddenTelegraphs:
                                        modifierCue = new HiddenTelegraphs()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick
                                        };
                                        /*
                                        modifierCue = new HiddenTelegraphs(type,
                                           modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"]
                                           );*/
                                        break;
                                    case ModifierType.InvisibleGuns:
                                        modifierCue = new InvisibleGuns()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick
                                        };
                                        /*
                                        modifierCue = new InvisibleGuns(type,
                                           modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"]
                                           );*/
                                        break;
                                    case ModifierType.Particles:
                                        modifierCue = new Particles()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            Amount = amountPercentage
                                        };
                                        /*
                                        modifierCue = new Particles(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p
                                            );*/
                                        break;
                                    case ModifierType.Psychedelia:
                                        if (!Config.enablePsychedelia || (!Config.enableFlashingLights && amount > 500f)) continue;
                                        modifierCue = new Psychedelia()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            Amount = amount
                                        };
                                        /*
                                        modifierCue = new Psychedelia(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"]
                                            );*/
                                        //if (!Config.enableFlashingLights && modifierCue.Amount > 500f) continue;
                                            break;
                                    case ModifierType.PsychedeliaUpdate:
                                        if (!Config.enablePsychedelia || (!Config.enableFlashingLights && amount > 500f)) continue;
                                        modifierCue = new PsychedeliaUpdate()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            Amount = amount,
                                            IsSingleUse = true
                                        };
                                       /* modifierCue = new PsychedeliaUpdate(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["amount"]
                                            );*/
                                        //if (!Config.enableFlashingLights && modifierCue.Amount > 500f) continue;
                                        break;
                                    case ModifierType.Speed:
                                        if (amount < 1f && !Config.enableScoreDisablingModifiers) continue;
                                        modifierCue = new SpeedChange()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            Amount = amountPercentage
                                        };
                                        /*
                                        modifierCue = new SpeedChange(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p
                                            );*/
                                        //if (modifierCue.Amount < 1f && !Config.enableScoreDisablingModifiers) continue;
                                        break;
                                    case ModifierType.zOffset:
                                        if (modifiersJSON["modifiers"][i]["option1"]) continue;
                                        float transitionAmount = 0f;
                                        float.TryParse(modifiersJSON["modifiers"][i]["value1"], out transitionAmount);
                                        modifierCue = new ZOffset()
                                        {
                                            Type = type,
                                            StartTick = modifiersJSON["modifiers"][i]["startTick"],
                                            EndTick = modifiersJSON["modifiers"][i]["endTick"],
                                            Amount = modifiersJSON["modifiers"][i]["amount"] / p,
                                            transitionNumberOfTargets = transitionAmount,
                                            IsSingleUse = true

                                        };
                                        /*modifierCue = new ZOffset(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p,
                                            transitionAmount
                                            );
                                        modifierCue.isSingleUseModule = true;*/
                                        AuthorableModifiersMod.zOffsetList.Add(modifierCue);
                                        continue;
                                      case ModifierType.ArenaRotation:
                                        if (!Integrations.arenaLoaderFound || !Config.enableArenaRotation) continue;
                                        //if (!Config.enableArenaRotation) continue;
                                        modifierCue = new ArenaRotation()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            Amount = amount,
                                            Continuous = option1,
                                            Incremental = option2,
                                            IsSingleUse = !option1 && !option2
                                        };
                                        /*modifierCue = new ArenaRotation(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"],
                                            modifiersJSON["modifiers"][i]["option1"],
                                            modifiersJSON["modifiers"][i]["option2"]
                                            );*/
                                        //if (!modifiersJSON["modifiers"][i]["option1"] && !modifiersJSON["modifiers"][i]["option2"]) modifierCue.IsSingleUse = true;
                                        //if (!option1 && !option2) modifierCue.IsSingleUse = true;
                                        //else modifierCue.Amount /= p;
                                        break;
                                    case ModifierType.ArenaBrightness:
                                        if (!Integrations.arenaLoaderFound || !Config.enableFlashingLights) continue;
                                        //if (!Config.enableFlashingLights) continue;
                                        modifierCue = new ArenaBrightness()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            Amount = amount,
                                            Continuous = option1,
                                            Strobo = option2,
                                            IsSingleUse = !option1 && !option2
                                        };
                                        /*modifierCue = new ArenaBrightness(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"],
                                            modifiersJSON["modifiers"][i]["option1"],
                                            modifiersJSON["modifiers"][i]["option2"]
                                            );*/
                                        //if (!modifiersJSON["modifiers"][i]["option1"] && !modifiersJSON["modifiers"][i]["option2"]) modifierCue.IsSingleUse = true;
                                        //if (!Config.enableFlashingLights && modifiersJSON["modifiers"][i]["option2"]) continue;
                                        break;
                                    case ModifierType.Fader:
                                        if (!Integrations.arenaLoaderFound || !Config.enableFlashingLights) continue;
                                        //if (!Config.enableFlashingLights) continue;
                                        modifierCue = new Fader()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            Amount = amountPercentage
                                        };

                                        /*modifierCue = new Fader(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p
                                            );*/
                                        break;
                                    case ModifierType.ArenaChange:
                                        if (!Integrations.arenaLoaderFound) continue;
                                        List<string> arenaOptions = new List<string>();
                                        if (value1 != "") arenaOptions.Add(value1);
                                        if (value2 != "") arenaOptions.Add(value2);
                                        modifierCue = new ArenaChange()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            ArenaOptions = arenaOptions,
                                            Preload = option1,
                                            IsSingleUse = true
                                        };

                                        /*modifierCue = new ArenaChange(type,
                                             modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["value1"],
                                             modifiersJSON["modifiers"][i]["value2"],
                                             modifiersJSON["modifiers"][i]["option1"]
                                            );*/
                                        //modifierCue.IsSingleUse = true;
                                        //preload = modifiersJSON["modifiers"][i]["option1"];
                                        preload = option1;
                                        break;
                                    case ModifierType.OverlaySetter:
                                        if (!Integrations.scoreOverlayFound) continue;
                                        modifierCue = new OverlaySetter()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            SongInfo = value1,
                                            Mapper = value2,
                                            IsSingleUse = true
                                        };
                                        /*
                                        modifierCue = new OverlaySetter(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["value1"],
                                            modifiersJSON["modifiers"][i]["value2"]
                                            );*/
                                        //modifierCue.IsSingleUse = true;
                                        break;
                                    case ModifierType.TextPopup:
                                        modifierCue = new TextPopup(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["value1"], //text
                                            modifiersJSON["modifiers"][i]["value2"], //size
                                            modifiersJSON["modifiers"][i]["xoffset"], // x offset
                                            modifiersJSON["modifiers"][i]["yoffset"], // y offset
                                            modifiersJSON["modifiers"][i]["zoffset"], // z offset
                                            modifiersJSON["modifiers"][i]["option1"], // glow
                                            modifiersJSON["modifiers"][i]["independantBool"]);// face forward
                                        break;
                                    case ModifierType.AutoLighting:
                                        if (!Integrations.arenaLoaderFound || !Config.enableFlashingLights) continue;
                                        //if (!Config.enableFlashingLights) continue;

                                        modifierCue = new AutoLighting()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            Amount = amountPercentage,
                                            PulseMode = option1,
                                            OriginalMaxBrightness = amountPercentage
                                        };

                                        /*modifierCue = new AutoLighting(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p,
                                            modifiersJSON["modifiers"][i]["option1"]);*/
                                        AutoLighting al = modifierCue as AutoLighting;
                                        AuthorableModifiersMod.autoLightings.Add(al);
                                        break;
                                    case ModifierType.ArenaScale:
                                    case ModifierType.ArenaPosition:
                                    case ModifierType.ArenaSpin:
                                        if (!Config.enableArenaManipulation) continue;
                                        modifierCue = new ArenaManipulation()
                                        {
                                            Type = type,
                                            StartTick = startTick,
                                            EndTick = endTick,
                                            AmountX = xOffset,
                                            AmountY = yOffset,
                                            AmountZ = zOffset,
                                            Reset = option1,
                                            Preload = option2,
                                            ShortestRoute = independantBool
                                        };
                                        /*modifierCue = new ArenaScale(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["xoffset"],
                                            modifiersJSON["modifiers"][i]["yoffset"],
                                            modifiersJSON["modifiers"][i]["zoffset"],
                                            modifiersJSON["modifiers"][i]["option1"],
                                            modifiersJSON["modifiers"][i]["option2"]);
                                        preload = modifiersJSON["modifiers"][i]["option2"];*/
                                        preload = option2;
                                        break;
                                    case ModifierType.SkyboxColor:
                                        if (!Config.enableSkyboxColorChange || !Integrations.arenaLoaderFound) continue;
                                        modifierCue = new SkyboxColor(type,
                                            amount,
                                            startTick,
                                            endTick,
                                            leftHandColor.r,
                                            leftHandColor.g,
                                            leftHandColor.b,
                                            option2);
                                        if(startTick == endTick || endTick == 0)
                                        {
                                            modifierCue.IsSingleUse = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (modifierCue.EndTick == 0)
                                {
                                    modifierCue.IsSingleUse = true;
                                    AuthorableModifiersMod.singleUseModifiers.Add(modifierCue);
                                }
                                    
                                //if (modifierCue.endTick == 0) modifierCue.endTick = _endTick;
                                if (preload) AuthorableModifiersMod.preloadModifiers.Add(modifierCue);
                                else modifiers.Add(modifierCue);

                            }
                            return modifiers;
                        }
                    }                      
                }
            }
        }
    }
}
