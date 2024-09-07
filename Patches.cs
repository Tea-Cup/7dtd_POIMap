using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace POIMap {
	static class Patches {
		private static readonly FieldInfo uiLblCursorPos = AccessTools.Field(typeof(XUiC_MapArea), "uiLblCursorPos");

		[HarmonyPatch(typeof(XUiC_MapArea), nameof(XUiC_MapArea.Init))]
		private static class XUiC_MapAreaInitPatch {
			private static readonly MethodInfo MapAreaInit = AccessTools.Method(typeof(ModMain), nameof(ModMain.MapAreaInit));
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> original) {
				List<CodeInstruction> codes = new List<CodeInstruction>(original);
				int index = codes.FindIndex(x => x.StoresField(uiLblCursorPos));
				if (index == -1) {
					Log.Error("[POIMap] Failed to inject XUiC_MapArea.Init: stfld uiLblCursorPos not found.");
					return original;
				}

				Log.Out("[POIMap] Injecting into XUiC_MapArea.Init");
				codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldarg_0));           // this
				codes.Insert(index + 2, new CodeInstruction(OpCodes.Call, MapAreaInit)); // MapAreaInit(this)
				Log.Out("[POIMap] Injection complete.");

				return codes;
			}
		}

		[HarmonyPatch(typeof(XUiC_MapArea), nameof(XUiC_MapArea.Update))]
		private static class XUiC_MapAreaUpdatePatch {
			private static readonly FieldInfo bMouseOverMap = AccessTools.Field(typeof(XUiC_MapArea), "bMouseOverMap");
			private static readonly MethodInfo MapAreaUpdate = AccessTools.Method(typeof(ModMain), nameof(ModMain.MapAreaUpdate));
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> original) {
				List<CodeInstruction> codes = new List<CodeInstruction>(original);
				int index = codes.FindIndex(x => x.LoadsField(uiLblCursorPos));
				if (index == -1) {
					Log.Error("[POIMap] Failed to inject XUiC_MapArea.Update: ldfld uiLblCursorPos not found.");
					return original;
				}
				index -= 1;

				Log.Out("[POIMap] Injecting into XUiC_MapArea.Update");
				codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldarg_0));              // this
				codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldarg_0));              // this, this
				codes.Insert(index + 2, new CodeInstruction(OpCodes.Ldfld, bMouseOverMap)); // this, this.bMouseOverMap
				codes.Insert(index + 3, new CodeInstruction(OpCodes.Ldloc_0));              // this, this.bMouseOverMap, pos
				codes.Insert(index + 4, new CodeInstruction(OpCodes.Call, MapAreaUpdate));  // MapAreaUpdate(this, bMouseOverMap, pos)
				Log.Out("[POIMap] Injection complete.");

				return codes;
			}
		}
	}
}
