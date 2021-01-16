using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using AuthorableModifiers;

[assembly: AssemblyTitle(AuthorableModifiersMod.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(AuthorableModifiersMod.BuildInfo.Company)]
[assembly: AssemblyProduct(AuthorableModifiersMod.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + AuthorableModifiersMod.BuildInfo.Author)]
[assembly: AssemblyTrademark(AuthorableModifiersMod.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(AuthorableModifiersMod.BuildInfo.Version)]
[assembly: AssemblyFileVersion(AuthorableModifiersMod.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(AuthorableModifiersMod), AuthorableModifiersMod.BuildInfo.Name, AuthorableModifiersMod.BuildInfo.Version, AuthorableModifiersMod.BuildInfo.Author, AuthorableModifiersMod.BuildInfo.DownloadLink)]
[assembly: MelonOptionalDependencies("ArenaLoader", "ScoreOverlay", "AutoLightshow")]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]