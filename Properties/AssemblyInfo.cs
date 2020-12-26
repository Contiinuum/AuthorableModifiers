using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using AudicaModding;

[assembly: AssemblyTitle(AuthorableModifiers.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(AuthorableModifiers.BuildInfo.Company)]
[assembly: AssemblyProduct(AuthorableModifiers.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + AuthorableModifiers.BuildInfo.Author)]
[assembly: AssemblyTrademark(AuthorableModifiers.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(AuthorableModifiers.BuildInfo.Version)]
[assembly: AssemblyFileVersion(AuthorableModifiers.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(AuthorableModifiers), AuthorableModifiers.BuildInfo.Name, AuthorableModifiers.BuildInfo.Version, AuthorableModifiers.BuildInfo.Author, AuthorableModifiers.BuildInfo.DownloadLink)]
[assembly: MelonOptionalDependencies("ArenaLoader")]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]