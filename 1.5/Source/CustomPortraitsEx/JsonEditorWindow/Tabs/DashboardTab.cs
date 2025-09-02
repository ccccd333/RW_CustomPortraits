using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow.Tabs
{
    public class DashboardTab : TabBase
    {

        public static string WRITE_JSON_ALL = "write json all";
        public static string WRITE_NEW_JSON_ALL = "write new json all";
        public static string WRITE_JSON_PW = "write json pw";

        public string selected_preset_name = "";
        int stage = 0;
        string new_json_name = "";
        bool new_json_edit = false;

        public void Draw(Rect inRect, Dictionary<string, bool> end_flags)
        {
            Listing_Standard listing = Begin(inRect);

            switch (stage)
            {
                case 0:
                    DrawEditorContent(listing, end_flags);
                    break;
                case 1:
                    DrawPresetNameInput(listing);
                    break;
            }

            SetStage();

            End(listing);
        }

        public void Reset()
        {
            stage = 0;
            call_id = "";
            selected_preset_name = "";
            new_json_name = "";
            new_json_edit = false;
        }

        private void DrawEditorContent(Listing_Standard listing, Dictionary<string, bool> end_flags)
        {

            listing.Label("RCPSelectPreset".Translate());

            List<string> presets = new List<string>(PortraitCacheEx.Refs.Keys);
            listing.Label("Select an item from the list:");
            listing.GapLine();

            Rect clear_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(clear_rect.RightPart(0.55f), "入力済みをクリア"))
            {
                call_id = "clear";
            }

            listing.GapLine();

            if (listing.ButtonText("プリセットの新規作成"))
            {
                call_id = "json new";
            }

            listing.GapLine();

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
            Widgets.Label(edit_selected_json_rect.LeftPart(0.6f), "選択されたプリセット名：");
            Widgets.Label(edit_selected_json_rect.RightPart(0.15f), selected_preset_name);

            listing.GapLine();

            listing.Label("新規作成のプリセットでない且つ「優先順位の設定」のみ単体でチェックが入っていればJsonの書き込み可能です");

            listing.GapLine();

            foreach (var item in end_flags)
            {
                bool marked_for = item.Value;

                Rect row_rect = listing.GetRect(30f);

                // ラベル
                Widgets.Label(row_rect.LeftPart(0.6f), $"「{item.Key}」");
                Rect checkbox_rect = row_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect.x, checkbox_rect.y, ref marked_for);

            }

            listing.GapLine();

            Rect enter_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "JSONに書き込み"))
            {
                bool marked_for = true;
                bool marked_for_priority_weight = true;
                foreach (var item in end_flags)
                {
                    if(item.Key != "優先順位の設定" && item.Value)
                    {
                        marked_for_priority_weight = false;
                    }else if (item.Key == "優先順位の設定" && !item.Value)
                    {
                        marked_for_priority_weight = false;
                    }

                    if (!item.Value)
                    {
                        marked_for = false;
                    }
                }

                if (marked_for)
                {
                    call_id = WRITE_JSON_ALL;

                    if (new_json_edit)
                    {
                        call_id = WRITE_NEW_JSON_ALL;
                    }
                }

                if (!new_json_edit && marked_for_priority_weight)
                {
                    call_id = WRITE_JSON_PW;
                }

                marked_for = false;

                
            }
        }

        void DrawPresetNameInput(Listing_Standard listing)
        {
            listing.Label("プリセット名の入力");
            listing.Label("ここはRimWorld/CustomPortraitsに配置されている画像名の拡張子を除いたものを入力してください");
            listing.GapLine();

            if (new_json_name != "" && PortraitCacheEx.Refs.ContainsKey(new_json_name))
            {
                listing.Label("既に存在するプリセット名です。違う名前にしてください。");
            }

            listing.GapLine();

            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "json new->back";
            }

            listing.GapLine();

            Rect row = listing.GetRect(30f);

            Widgets.Label(row.LeftPart(0.6f), "プリセット名を入力してください:");

            Rect text_rect = new Rect(row.x + row.width * 0.6f, row.y, row.width * 0.25f, row.height);
            new_json_name = Widgets.TextField(text_rect, new_json_name);

            Rect button_rect = new Rect(row.x + row.width * 0.87f, row.y, row.width * 0.13f, row.height);
            if (Widgets.ButtonText(button_rect, "決定"))
            {
                new_json_name = new_json_name.Trim();

                if (!string.IsNullOrEmpty(new_json_name))
                {
                    call_id = "json new->end";
                    selected_preset_name = new_json_name;
                    new_json_edit = true;
                }
            }
        }

        private void SetStage()
        {
            if (stage == 0)
            {
                if (call_id == "json new")
                {
                    stage = 1;
                }
                else if (call_id == "clear")
                {
                    Reset();
                    stage = 0;
                }
            }
            else if (stage == 1)
            {
                if (call_id == "json new->end")
                {
                    stage = 0;
                }
                else if (call_id == "json new->back")
                {
                    Reset();
                    stage = 0;
                }
            }
        }
    }
}
