/**
 * AddonManager
 * Author: Denarii Games
 * Version: 1.0-rc2
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace NightBlade.AddonManager
{
	public static class Constants
	{
		public static IReadOnlyList<string> Categories { get; } = new List<string>
		{
			"Demo",
			"Characters",
			"Monsters",
			"NPCs",
			"Combat",
			"Economy",
			"Items",
			"Gameplay",
			"UI",
			"MMO",
			"Integration",
			"Tools"
		}.AsReadOnly();

		public static bool IsCategoryAllowed(string category) => Categories.Contains(category);
	}

	public enum PackageStatus { Unknown, NotInstalled, UpToDate, Outdated, Fetching }

	public class PackageInfo
	{
		public string guid; //required in package.json, must match file in packaged unitypackage
		public string name; //required in package.json
		public string packageUrl; //required in package.json
		public string latestVersion; //required in package.json
		public string updateDate; //required in package.json, format: "2025-12-25"
		public string author; //required in package.json

		public string description; //optional in package.json
		public string category;  //optional in package.json
		public bool isCore; //reserved for official addons

		public string gitUrl; //set from manifest to package.json (avoid master vs main discovery)
		public string screenshotUrl; //computed from giturl and screenshot in package.json
		public PackageStatus status = PackageStatus.Unknown; //set in GetInstalledAddons
		public string installedVersion = null; //set in GetInstalledAddons
		public Texture2D screenshot = null; //set in FetchScreenshot

		public bool corePatch; //computed from patchFile in package.json
		public string patchUrl; //computed from giturl and patchFile in package.json

		public string[] dependencies;
	}

    public partial class AddonManagerWindow : EditorWindow
    {
		private const string PACKAGE_MANIFEST_URL = "https://raw.githubusercontent.com/denariigames/nightblade-addon-manager/refs/heads/master/manifest.json";
		private const string ADDON_FOLDER = "Assets/NightBlade_addons/";

		private List<PackageInfo> packages = new List<PackageInfo>();
		private PackageInfo selectedPackage = null;

		[MenuItem("NightBlade/Addon Manager")]
		public static void ShowWindow()
		{
			GetWindow<AddonManagerWindow>("Addon Manager");
		}

		private Texture2D logoIcon;
		private Texture2D checkIcon;
		private Texture2D updateIcon;
		private Texture2D reloadIcon;
		private Texture2D coreBadge;
		private Texture2D newBadge;
		private Texture2D hotBadge;
		private Texture2D patchBadge;
		private string lastCategoryFilter = "";
		private string lastStatusFilter = "";
		private string lastUpdatedFilter = "";
		private string lastCoreFilter = "";
		private void OnEnable()
		{
			//Load textures
			logoIcon = Resources.Load<Texture2D>("LogoIcon");
			checkIcon = Resources.Load<Texture2D>("CheckIcon");
			updateIcon = Resources.Load<Texture2D>("UpdateIcon");
			reloadIcon = Resources.Load<Texture2D>("ReloadIcon");
			coreBadge = Resources.Load<Texture2D>("CoreBadge");
			newBadge = Resources.Load<Texture2D>("NewBadge");
			hotBadge = Resources.Load<Texture2D>("HotBadge");
			patchBadge = Resources.Load<Texture2D>("PatchBadge");

			LoadAddonsManifest();

			if (AddonInstallState.HasPending)
			{
				CompleteInstall(AddonInstallState.PackageGuid, ADDON_FOLDER + AddonInstallState.TargetFolder);
				AddonInstallState.Clear();
			}

			//clear filters
			lastCategoryFilter = "";
			lastStatusFilter = "";
			lastUpdatedFilter = "";
			lastCoreFilter = "";
		}
	}
}