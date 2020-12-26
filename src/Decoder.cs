using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using SimpleJSON;
using MelonLoader;
namespace AudicaModding
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
                                MelonLogger.Log("Couldn't read file.");
                                return null;
                            }
                            for (int i = 0; i < modifiersJSON["modifiers"].Count; i++)
                            {
                                ModifierType type = (ModifierType) Enum.Parse(typeof(ModifierType), modifiersJSON["modifiers"][i]["type"]);
                                Modifier modifierCue = null;
                                float p = 100;
                                bool preload = false;
                                switch (type)
                                {
                                    case ModifierType.AimAssist:
                                        modifierCue = new AimAssistChange(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p
                                            );
                                        break;
                                    case ModifierType.ColorChange:
                                        if (!Config.enableColorChange) continue;
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
                                            );
                                        break;
                                    case ModifierType.ColorUpdate:
                                        if (!Config.enableColorChange) continue;
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
                                            );
                                        break;
                                    case ModifierType.ColorSwap:
                                        if (!Config.enableColorChange) continue;
                                        modifierCue = new ColorSwap(type,
                                           modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"]
                                           );
                                        break;
                                    case ModifierType.HiddenTelegraphs:
                                        modifierCue = new HiddenTelegraphs(type,
                                           modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"]
                                           );
                                        break;
                                    case ModifierType.InvisibleGuns:
                                        modifierCue = new InvisibleGuns(type,
                                           modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"]
                                           );
                                        break;
                                    case ModifierType.Particles:
                                        modifierCue = new Particles(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                           modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p
                                            );
                                        break;
                                    case ModifierType.Psychedelia:                                       
                                        modifierCue = new Psychedelia(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"]
                                            );
                                        if (!Config.enableFlashingLights && modifierCue.amount > 2000f) continue;
                                            break;
                                    case ModifierType.PsychedeliaUpdate:
                                        modifierCue = new PsychedeliaUpdate(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["amount"]
                                            );
                                        if (!Config.enableFlashingLights && modifierCue.amount > 2000f) continue;
                                        break;
                                    case ModifierType.Speed:
                                        modifierCue = new SpeedChange(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p
                                            );
                                        break;
                                    case ModifierType.zOffset:
                                        float transitionAmount = 0f;
                                        float.TryParse(modifiersJSON["modifiers"][i]["value1"], out transitionAmount);
                                        modifierCue = new ZOffset(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p,
                                            transitionAmount
                                            );
                                        modifierCue.isSingleUseModule = true;
                                        AuthorableModifiers.zOffsetList.Add(modifierCue);
                                        continue;
                                      case ModifierType.ArenaRotation:
                                        if (!Integrations.arenaLoaderFound) continue;
                                        if (!Config.enableArenaRotation) continue;
                                        modifierCue = new ArenaRotation(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"],
                                            modifiersJSON["modifiers"][i]["option1"]
                                            );
                                        if (!modifiersJSON["modifiers"][i]["option1"]) modifierCue.isSingleUseModule = true;
                                        else modifierCue.amount /= p;
                                        break;
                                    case ModifierType.ArenaBrightness:
                                        if (!Integrations.arenaLoaderFound) continue;
                                        modifierCue = new ArenaBrightness(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"],
                                            modifiersJSON["modifiers"][i]["option1"],
                                            modifiersJSON["modifiers"][i]["option2"]
                                            );
                                        if (!modifiersJSON["modifiers"][i]["option1"] && !modifiersJSON["modifiers"][i]["option2"]) modifierCue.isSingleUseModule = true;
                                        if (!Config.enableFlashingLights && modifiersJSON["modifiers"][i]["option2"]) continue;
                                        break;
                                    case ModifierType.Fader:
                                        if (!Integrations.arenaLoaderFound) continue;
                                        modifierCue = new Fader(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["amount"] / p
                                            );
                                        break;
                                    case ModifierType.ArenaChange:
                                        if (!Integrations.arenaLoaderFound) continue;
                                        modifierCue = new ArenaChange(type,
                                             modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["value1"],
                                             modifiersJSON["modifiers"][i]["value2"],
                                             modifiersJSON["modifiers"][i]["option1"]
                                            );
                                        modifierCue.isSingleUseModule = true;
                                        preload = modifiersJSON["modifiers"][i]["option1"];
                                        break;
                                    case ModifierType.OverlaySetter:
                                        if (!Integrations.scoreOverlayFound) continue;
                                        modifierCue = new OverlaySetter(type,
                                            modifiersJSON["modifiers"][i]["startTick"],
                                            modifiersJSON["modifiers"][i]["endTick"],
                                            modifiersJSON["modifiers"][i]["value1"],
                                            modifiersJSON["modifiers"][i]["value2"]
                                            );
                                        modifierCue.isSingleUseModule = true;
                                        break;
                                    default:
                                        break;
                                }
                                if (modifierCue.endTick == 0) modifierCue.isSingleUseModule = true;
                                //if (modifierCue.endTick == 0) modifierCue.endTick = _endTick;
                                if (preload) AuthorableModifiers.preloadModifiers.Add(modifierCue);
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
