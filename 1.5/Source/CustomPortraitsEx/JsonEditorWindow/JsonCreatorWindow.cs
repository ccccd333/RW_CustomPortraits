using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow
{
    [StaticConstructorOnStartup]
    public class JsonCreatorWindow : Mod
    {
        private Vector2 scroll;
        private float scrollHeight = 200f;
        private string input_text = "";


        public JsonCreatorWindow(ModContentPack content) : base(content) { }

        public override string SettingsCategory()
        {
            return "RCPCreator".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0, 0, inRect.width - 17f, scrollHeight);
            Widgets.BeginScrollView(inRect, ref scroll, viewRect);
            Listing_Standard listing = new Listing_Standard(inRect.AtZero(), () => scroll)
            {
                maxOneColumn = true,
                ColumnWidth = viewRect.width
            };
            listing.Begin(viewRect);

            //----------------------Begin

            if (!PortraitCacheEx.IsAvailable)
            {
                listing.Label("RCPError".Translate());
            }
            else
            {
                listing.Label("New JSON Name:");
                Rect textRect = listing.GetRect(30f);
                input_text = Widgets.TextField(textRect, input_text);

                if (listing.ButtonText("Create"))
                {
                    Log.Message("Creating JSON: " + input_text);
                    // JSON作成処理
                }

                listing.GapLine();
            }

            //----------------------End
            if (Event.current.type == EventType.Layout)
            {
                scrollHeight = listing.CurHeight;
            }

            listing.End();
            Widgets.EndScrollView();
        }
    }
}
