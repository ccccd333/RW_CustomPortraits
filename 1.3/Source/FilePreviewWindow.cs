using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits {
	public class FilePreviewWindow : Window {
		private readonly Texture2D tex;

		public FilePreviewWindow(Texture2D image) {
			tex = image;
			layer = WindowLayer.Super;
			closeOnClickedOutside = true;
			doWindowBackground = false;
			drawShadow = false;
			preventCameraMotion = false;
		}

		public override void DoWindowContents(Rect inRect) {
			
		}
	}
}
