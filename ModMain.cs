using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace POIMap {
	public class ModMain : IModApi {
		private static readonly Color inactiveColor = Color.grey;
		private static readonly Color prefabActiveColor = Color.red;
		private static readonly Color biomeActiveColor = new Color(1f, 0.5f, 0f);
		private static readonly FastTags<TagGroup.Poi> excludeTags = FastTags<TagGroup.Poi>.Parse("part,streettile,navonly,hideui");
		private static XUiV_Label uiLblCursorPOI;
		private static XUiV_Sprite uiSkull1;
		private static XUiV_Sprite uiSkull2;
		private static XUiV_Sprite uiSkull3;
		private static XUiV_Sprite uiSkull4;
		private static XUiV_Sprite uiSkull5;
		private static XUiV_Sprite uiSkull6;
		private static XUiV_Sprite uiSkull7;
		private static XUiV_Rect uiSkullHalf;
		private static float lastTimePrefabChecked = 0;
		private static Vector3 lastPos;

		public static PrefabInstance GetPOIAtWorld(World world, int x, int z) {
			DynamicPrefabDecorator dpd = world.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
			List<PrefabInstance> list = new List<PrefabInstance>();
			dpd.GetPrefabsAtXZ(x, x + 1, z, z + 1, list);
			foreach (PrefabInstance inst in list) {
				if (!inst.prefab.Tags.Test_AnySet(excludeTags)) return inst;
			}
			return null;
		}
		public static Color GetSkullColor(int prefabDiff, int biomeDiff, int skullDiff) {
			if (prefabDiff >= skullDiff) return prefabActiveColor;
			if (prefabDiff + biomeDiff >= skullDiff) return biomeActiveColor;
			return inactiveColor;
		}
		private static void SetValues(string name, int prefab, int biome, bool half) {
			if(name == null) {
				uiLblCursorPOI.IsVisible = false;
				uiSkull1.IsVisible = false;
				uiSkull2.IsVisible = false;
				uiSkull3.IsVisible = false;
				uiSkull4.IsVisible = false;
				uiSkull5.IsVisible = false;
				uiSkull6.IsVisible = false;
				uiSkull7.IsVisible = false;
				uiSkullHalf.IsVisible = false;
				return;
			}

			int sumDiff = prefab + biome;
			uiLblCursorPOI.Text = name;
			uiLblCursorPOI.IsVisible = true;
			uiSkull1.Color = GetSkullColor(prefab, biome, 1);
			uiSkull2.Color = GetSkullColor(prefab, biome, 2);
			uiSkull3.Color = GetSkullColor(prefab, biome, 3);
			uiSkull4.Color = GetSkullColor(prefab, biome, 4);
			uiSkull5.Color = GetSkullColor(prefab, biome, 5);
			uiSkull6.Color = GetSkullColor(prefab, biome, 6);
			uiSkull7.Color = GetSkullColor(prefab, biome, 7);
			uiSkull1.IsVisible = sumDiff >= 1;
			uiSkull2.IsVisible = sumDiff >= 2;
			uiSkull3.IsVisible = sumDiff >= 3;
			uiSkull4.IsVisible = sumDiff >= 4;
			uiSkull5.IsVisible = sumDiff >= 5;
			uiSkull6.IsVisible = sumDiff >= 6;
			uiSkull7.IsVisible = sumDiff >= 7;
			uiSkullHalf.IsVisible = half;
		}

		public static void MapAreaInit(XUiC_MapArea xui) {
			uiLblCursorPOI = (XUiV_Label)xui.GetChildById("cursorPOI").ViewComponent;
			uiSkull1 = (XUiV_Sprite)xui.GetChildById("skull1").ViewComponent;
			uiSkull2 = (XUiV_Sprite)xui.GetChildById("skull2").ViewComponent;
			uiSkull3 = (XUiV_Sprite)xui.GetChildById("skull3").ViewComponent;
			uiSkull4 = (XUiV_Sprite)xui.GetChildById("skull4").ViewComponent;
			uiSkull5 = (XUiV_Sprite)xui.GetChildById("skull5").ViewComponent;
			uiSkull6 = (XUiV_Sprite)xui.GetChildById("skull6").ViewComponent;
			uiSkull7 = (XUiV_Sprite)xui.GetChildById("skull7").ViewComponent;
			uiSkullHalf = (XUiV_Rect)xui.GetChildById("skullHalf").ViewComponent;
		}
		public static void MapAreaUpdate(XUiC_MapArea xui, bool bMouseOverMap, Vector3 pos) {
			if (!bMouseOverMap) {
				SetValues(null, 0, 0, false);
				return;
			}
			if (Vector3.Distance(pos, lastPos) < 0.5f) return;
			lastPos = pos;
			lastTimePrefabChecked = Time.time;

			int x = Mathf.FloorToInt(pos.x), z = Mathf.FloorToInt(pos.z);
			World world = GameManager.Instance.World;
			Prefab prefab = GetPOIAtWorld(world, x, z)?.prefab;
			BiomeDefinition biome = (world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw)?.GetBiomeProvider().GetBiomeAt(x, z);

			string name = "";
			int prefabDiff = prefab?.DifficultyTier ?? 0;
			int biomeDiff = 0;
			bool showHalf = false;

			if (prefab != null) {
				name = prefab.LocalizedName;
				prefabDiff = prefab.DifficultyTier;
			} else if (biome != null) {
				name = biome.LocalizedName;
			}
			if (biome != null) {
				float n = (biome.Difficulty - 1) * 0.5f;
				biomeDiff = Mathf.FloorToInt(n);
				showHalf = n - biomeDiff == 0.5f;
			}
			SetValues(name, prefabDiff, biomeDiff, showHalf);
		}

		public void InitMod(Mod modInstance) {
			new Harmony("Foxy.POIMap").PatchAll();
		}
	}
}
