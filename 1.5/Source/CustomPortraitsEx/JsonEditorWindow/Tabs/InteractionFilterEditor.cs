using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
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
    public class InteractionFilterEditor : TabBase
    {
        int stage = 0;
        string call_id = "";
        InteractionSelectionMap ism;

        private string edit_target_intf = "";
        private string edit_target_intf_initiator = "";
        private string edit_target_intf_recipient = "";
        private int edit_target_cache_duration_seconds = 12;
        private string filter_text = "";
        string cache_duration_seconds_input_buffer = "12";

        private Dictionary<string, List<InteractionFilter>> temp_reverse_interaction_filter = new Dictionary<string, List<InteractionFilter>>();
        //private List<InteractionFilter> temp_interaction_filter_row = new List<InteractionFilter>();
        private List<string> temp_remove_interaction_filter_rows = new List<string>();

        private List<string> temp_edited_Interactions = new List<string>();

        private List<string> temp_selected_Interactions = new List<string>();

        private InteractionFilter temp_interaction_filter = new InteractionFilter();
        private List<InteractionFilter> temp_confirm_cache = new List<InteractionFilter>();


        public List<string> result_interaction_filter = new List<string>();


        public void Draw(Rect inRect, List<string> selected_Interactions)
        {
            Listing_Standard listing = Begin(inRect);

            listing.Label("ここはポーンが世間話や侮辱などを行ったもしくは受けた際");
            listing.Label("どういう名前で管理するかをまとめるものです");
            listing.Label("「侮辱を受けた側はこの名前」と決めることで、後続のグループ編集でここで決めた名前が使えるようになります");
            listing.Label("グループ編集でなぜ行わないかというと、インタラクトは受け手と送り手が存在するためです");
            listing.Label("心情と違い1人称ではなく二者関係ですが、Modによっては受け手も送り手も一緒の人間になる場合があります");
            listing.GapLine();

            if (selected_Interactions.Count <= 0)
            {
                listing.Label("「インタラクトの選択」タブを一つ以上選択してください。");
            }
            else
            {
                switch (stage)
                {
                    case 0:
                        CreateOrEditInteractionFilter(listing);
                        break;
                    case 1:
                        EnterInitiatorAndRecipientName(listing);
                        break;
                    case 2:
                        EditInteractionFilter(listing, selected_Interactions);
                        break;
                    case 3:
                        EndEditing(listing);
                        break;
                    case 99:
                        ConfirmCacheDurationOverride(listing);
                        break;
                }

                SetStage();
            }

            End(listing);
        }

        private void CreateOrEditInteractionFilter(Listing_Standard listing)
        {
            listing.Label("インタラクト名を新規作成");

            ism = PortraitCacheEx.InteractionSelectionMap;

            if (listing.ButtonText("新規作成する"))
            {
                call_id = "create";
            }

            temp_reverse_interaction_filter.Clear();
            foreach (var kv in ism.InteractionFilter)
            {
                string matched_key = $"[送り手]{kv.Value.matched_initiator_key},[受け手]{kv.Value.matched_recipient_key}";
                if (!temp_reverse_interaction_filter.ContainsKey(matched_key))
                {
                    temp_reverse_interaction_filter[matched_key] = new List<InteractionFilter>();
                }
                // 構造体にすればよかった。影響調査が面倒なのでこのまま
                temp_reverse_interaction_filter[matched_key].Add(new InteractionFilter(kv.Value));
            }

            bool is_cddifferent = false;
            listing.Label("既存のインタラクト定義を編集する");
            foreach (var gf in temp_reverse_interaction_filter)
            {
                if (listing.ButtonText(gf.Key))
                {
                    call_id = "edit";
                    edit_target_intf = gf.Key;
                    temp_edited_Interactions.Clear();
                    temp_confirm_cache.Clear();
                    foreach (var intf in gf.Value)
                    {
                        if (temp_interaction_filter.IsCacheDurationDifferent(intf))
                        {
                            temp_confirm_cache.Add(new InteractionFilter(intf));
                            is_cddifferent = true;
                        }
                        temp_interaction_filter = new InteractionFilter(intf);
                        if (!temp_edited_Interactions.Contains(intf.interaction_name))
                        {
                            temp_edited_Interactions.Add(intf.interaction_name);

                        }

                        if (!temp_selected_Interactions.Contains(intf.interaction_name))
                        {
                            temp_selected_Interactions.Add(intf.interaction_name);
                        }
                    }

                    temp_edited_Interactions.Sort();
                }
            }

            if (call_id == "edit" && is_cddifferent)
            {
                call_id = "edit->confirm cache duration override";
            }
        }

        private void EnterInitiatorAndRecipientName(Listing_Standard listing)
        {
            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "create->back";
            }

            listing.GapLine();

            listing.Label("送り手としてインタラクトした際の名前を入力してください:");
            Rect input_rect = listing.GetRect(30f);
            edit_target_intf_initiator = Widgets.TextField(input_rect, edit_target_intf_initiator);

            listing.GapLine();

            listing.Label("送り手としてインタラクトした際の名前を入力してください:");
            Rect input_rect2 = listing.GetRect(30f);
            edit_target_intf_recipient = Widgets.TextField(input_rect2, edit_target_intf_recipient);

            listing.GapLine();

            //listing.Label("この受け手と送り手(どちらかのみでも可能)をまとめる要約名を入力してください:");
            //Rect input_rect3 = listing.GetRect(30f);
            //edit_target_intf_short_label = Widgets.TextField(input_rect3, edit_target_intf_short_label);

            //listing.GapLine();

            listing.Label("これはキャッシュ時間です。ポーンが世間話をしたが終了したことまでは通知されません");
            listing.Label("そのため始まってから終了するまでの時間を決定します");
            listing.Label("キャッシュ時間の設定：");
            Rect input_rect4 = listing.GetRect(30f);
            int rv = edit_target_cache_duration_seconds;
            Widgets.TextFieldNumeric<int>(input_rect4, ref rv, ref cache_duration_seconds_input_buffer);
            edit_target_cache_duration_seconds = rv;
            listing.GapLine();

            Rect enter_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "決定"))
            {
                if (edit_target_intf_initiator != "" || edit_target_intf_recipient != "")
                {
                    call_id = "create->enter name";
                    temp_interaction_filter = new InteractionFilter();
                    if (edit_target_intf_initiator != "")
                    {
                        temp_interaction_filter.is_initiator = true;
                        temp_interaction_filter.matched_initiator_key = edit_target_intf_initiator;
                    }

                    if (edit_target_intf_recipient != "")
                    {
                        temp_interaction_filter.is_recipient = true;
                        temp_interaction_filter.matched_recipient_key = edit_target_intf_recipient;
                    }

                    if (edit_target_cache_duration_seconds < 0)
                    {
                        edit_target_cache_duration_seconds = 12;
                    }

                    temp_interaction_filter.cache_duration_seconds = edit_target_cache_duration_seconds;

                    edit_target_intf = $"[送り手]{edit_target_intf_initiator},[受け手]{edit_target_intf_recipient}";

                    //temp_interaction_filter_row.Clear();
                    //temp_interaction_filter_row.Add(interactionFilter);

                    if (temp_reverse_interaction_filter.ContainsKey(edit_target_intf))
                    {
                        temp_edited_Interactions.Clear();

                        foreach (var intf in temp_reverse_interaction_filter[edit_target_intf])
                        {
                            if (!temp_edited_Interactions.Contains(intf.interaction_name))
                            {
                                temp_edited_Interactions.Add(intf.interaction_name);

                            }
                        }
                    }
                }

            }
        }

        private void EditInteractionFilter(Listing_Standard listing, List<string> selected_Interactions)
        {
            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "edit->back";
            }

            listing.GapLine();

            listing.Label("ここはインタラクト選択一覧画面で選択したものを送り手/受け手の名前と紐づける場所です");
            listing.Label("１つ以上選択して決定ボタンを押してください");
            listing.Label("この受け手と送り手(どちらかのみでも可能)をまとめる要約名を入力してください:");

            listing.GapLine();

            listing.Label($"送り手/受け手の名前：{edit_target_intf}");


            listing.GapLine();

            Rect reset_rect = listing.GetRect(30f);
            Widgets.Label(reset_rect.LeftPart(0.6f), "Clear Selected");
            if (Widgets.ButtonText(reset_rect.RightPart(0.55f).LeftPart(0.7f), "CLEAR"))
            {
                temp_selected_Interactions.Clear();
            }


            listing.Label("インタラクト名をフィルターする:");
            Rect filter_rect = listing.GetRect(30f);
            filter_text = Widgets.TextField(filter_rect, filter_text);

            listing.GapLine();

            List<string> merged = selected_Interactions
                .Concat(temp_edited_Interactions)
                .Distinct()
                .ToList();
            merged.Sort();

            List<string> filtered;
            if (string.IsNullOrEmpty(filter_text))
            {
                filtered = merged;
            }
            else
            {
                try
                {
                    Regex regex = new Regex(filter_text, RegexOptions.IgnoreCase);
                    filtered = merged.Where(t => regex.IsMatch(t)).ToList();
                }
                catch (ArgumentException)
                {
                    // 正規表現エラー
                    listing.Label($"正規表現が不正です: {filter_text}");
                    filtered = new List<string>();
                }
            }

            foreach (var intr in filtered)
            {
                Rect row_rect = listing.GetRect(30f);

                // ラベル
                Widgets.Label(row_rect.LeftPart(0.6f), intr);

                // SELECT ボタン
                if (Widgets.ButtonText(row_rect.RightPart(0.55f).LeftPart(0.7f), "SELECT"))
                {
                    if (!temp_selected_Interactions.Contains(intr))
                    {
                        temp_selected_Interactions.Add(intr);
                    }
                    else
                    {
                        temp_selected_Interactions.Remove(intr);
                    }

                }

                // チェックボックス
                bool check_on = temp_selected_Interactions.Contains(intr);
                Rect checkbox_rect = row_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect.x, checkbox_rect.y, ref check_on);

            }

            Rect enter_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "決定"))
            {
                if (temp_selected_Interactions.Count > 0)
                {
                    result_interaction_filter.Clear();
                    if (temp_interaction_filter.matched_initiator_key != temp_interaction_filter.matched_recipient_key)
                    {
                        result_interaction_filter.Add(temp_interaction_filter.matched_initiator_key);
                        result_interaction_filter.Add(temp_interaction_filter.matched_recipient_key);
                    }
                    else
                    {
                        result_interaction_filter.Add(temp_interaction_filter.matched_initiator_key);
                    }
                    call_id = "edit->end editing";
                }
            }
        }

        private void EndEditing(Listing_Standard listing)
        {
            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "edit->end editing->back";
            }

            listing.GapLine();

            listing.Label("ここの設定は終わりです。この画面の状態で他のタブを入力してください");
            listing.Label("もし入力内容に問題がある場合、お手数ですが「保存しないで戻る」を選択してください");

            listing.GapLine();
            listing.Label($"送り手/受け手の名前：{edit_target_intf}");
            listing.Label($"インタラクト名：");
            foreach (var kv in temp_selected_Interactions)
            {
                listing.Label($"    ==>{kv}");
            }

            listing.GapLine();
            listing.Label($"キャッシュ時間：{temp_interaction_filter.cache_duration_seconds}");

        }

        private void ConfirmCacheDurationOverride(Listing_Standard listing)
        {
            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "edit->confirm cache duration override->back";
            }

            listing.GapLine();

            listing.Label("キャッシュ時間が異なるが、同じインタラクト名を指しています");
            listing.Label("手動でJsonを記述したもののため、この画面では後の値で上書きしてしまいます");
            listing.Label("一旦InteractionFilter.jsonをバックアップを取ることをお勧めします");
            listing.Label("よろしいですか？");

            listing.GapLine();
            listing.Label($"上書きされる対象と値の一覧：上書きされるキャッシュ時間 ==> {temp_interaction_filter.cache_duration_seconds}");
            foreach(var tg in temp_confirm_cache)
            {
                listing.Label($"    ==>インタラクト名 {tg.interaction_name} 受け手 {tg.matched_recipient_key} 送り手 {tg.matched_initiator_key}");
            }
            listing.GapLine();
            Rect enter_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "決定"))
            {
                call_id = "edit";
            }

        }

        private void SetStage()
        {
            if (stage == 0)
            {
                if (call_id == "create")
                {
                    stage = 1;
                }
                else if (edit_target_intf != "" && call_id == "edit")
                {
                    //temp_interaction_filter_row = temp_interaction_filter[edit_target_intf];
                    stage = 2;
                }
                else if (call_id == "edit->confirm cache duration override")
                {
                    stage = 99;
                }
            }
            else if (stage == 1)
            {
                if (call_id == "create->enter name")
                {
                    stage = 2;
                    call_id = "edit";
                }
                else if (call_id == "create->back")
                {
                    Reset();
                    stage = 0;
                }
            }
            else if (stage == 2)
            {
                if (call_id == "edit->back")
                {
                    Reset();
                    stage = 0;
                }
                else if (call_id == "edit->end editing")
                {
                    stage = 3;
                }
            }
            else if (stage == 3)
            {
                if (call_id == "edit->end editing->back")
                {
                    Reset();
                    stage = 0;
                }
            }
            else if (stage == 99)
            {
                if (call_id == "edit->confirm cache duration override->back")
                {
                    Reset();
                    stage = 0;
                }
                else if (edit_target_intf != "" && call_id == "edit")
                {
                    //temp_interaction_filter_row = temp_interaction_filter[edit_target_intf];
                    stage = 2;
                }
            }
        }

        private void Reset()
        {
            stage = 0;
            call_id = "";
            edit_target_intf = "";
            edit_target_intf_initiator = "";
            edit_target_intf_recipient = "";
            filter_text = "";
            cache_duration_seconds_input_buffer = "12";
            temp_reverse_interaction_filter.Clear();
            //temp_interaction_filter_row.Clear();
            temp_remove_interaction_filter_rows.Clear();
            temp_selected_Interactions.Clear();
            temp_edited_Interactions.Clear();
            temp_interaction_filter = new InteractionFilter();
            result_interaction_filter.Clear();
        }
    }
}
