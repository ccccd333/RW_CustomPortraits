using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits {
	public static class Helper {
		private static readonly MethodInfo helperDraw = AccessTools.Method(typeof(Helper), nameof(Helper.DrawPortrait));
		private static readonly MethodInfo helperCond = AccessTools.Method(typeof(Helper), nameof(Helper.ShouldInterceptDraw));

		public static MethodInfo DrawColonistBarMethod => helperDraw;
		public static MethodInfo ShouldDrawColonistBarMethod => helperCond;

		public static Pawn GetSelectedPawn() {
			if (Find.UIRoot is UIRoot_Play root) return root.mapUI.selector.SingleSelectedThing as Pawn;
			return null;
		}

		public static string Label(string key) {
			return $"Foxy.CustomPortraits.{key}".Translate().Trim();
		}
		public static string Label(string key, NamedArgument arg) {
			return $"Foxy.CustomPortraits.{key}".Translate(arg).Trim();
		}

		private static void DrawPortrait(Rect rect, Pawn pawn) {
			if (pawn is null) return;
			Texture2D tex = pawn.GetPortraitTexture(PortraitPosition.ColonistBar);
			if (tex == null) return;
			PortraitDrawer.DrawColonistBar(rect, tex);
		}
		private static bool ShouldInterceptDraw(Pawn pawn) {
			if (!StaticSettings.IsColonistBar) return false;
			return pawn != null && pawn.GetPortraitTexture(PortraitPosition.ColonistBar) != null;
		}
	}
}
