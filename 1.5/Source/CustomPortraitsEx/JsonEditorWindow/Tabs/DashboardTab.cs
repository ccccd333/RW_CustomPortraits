using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow.Tabs
{
    public class DashboardTab : TabBase
    {
        public string selected_preset_name = "";

        public void DrawEditorContent(Rect inRect, List<string> selected_thoughts, List<string> selected_Interactions)
        {

            Listing_Standard listing = Begin(inRect);
            //----------------------------

            listing.Label("RCPSelectPreset".Translate());

            List<string> presets = new List<string>(PortraitCacheEx.Refs.Keys);
            listing.Label("Select an item from the list:");

            // リスト表示
            foreach (var item in presets)
            {
                if (item == "InteractionFilter") continue;

                if (listing.ButtonText(item))
                {
                    // クリックされたら TextField にコピー
                    selected_preset_name = item;
                }
            }

            listing.GapLine();

            Rect edit_selected_json_rect = listing.GetRect(30f);
            Widgets.Label(edit_selected_json_rect.LeftPart(0.6f), "Edit selected JSON:");
            Widgets.Label(edit_selected_json_rect.RightPart(0.15f), selected_preset_name);

            bool marked_for_thought = false;
            if (selected_thoughts.Count > 0)
            {
                Rect row_rect = listing.GetRect(30f);

                // ラベル
                Widgets.Label(row_rect.LeftPart(0.6f), $"Selected Thoughts {selected_thoughts.Count}");
                marked_for_thought = true;
                Rect checkbox_rect = row_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect.x, checkbox_rect.y, ref marked_for_thought);
            }
            else
            {
                Rect row_rect = listing.GetRect(30f);

                // ラベル
                Widgets.Label(row_rect.LeftPart(0.6f), $"Selected Thoughts {selected_thoughts.Count}");
                marked_for_thought = false;
                Rect checkbox_rect = row_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect.x, checkbox_rect.y, ref marked_for_thought);
            }

            bool marked_for_interaction = false;
            if (selected_Interactions.Count > 0)
            {
                Rect row_rect = listing.GetRect(30f);

                // ラベル
                Widgets.Label(row_rect.LeftPart(0.6f), $"Selected Thoughts {selected_thoughts.Count}");
                marked_for_interaction = true;
                Rect checkbox_rect = row_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect.x, checkbox_rect.y, ref marked_for_interaction);
            }
            else
            {
                Rect row_rect = listing.GetRect(30f);

                // ラベル
                Widgets.Label(row_rect.LeftPart(0.6f), $"Selected Thoughts {selected_thoughts.Count}");
                marked_for_interaction = false;
                Rect checkbox_rect = row_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect.x, checkbox_rect.y, ref marked_for_interaction);
            }


            //----------------------------
            End(listing);
        }
    }
}
