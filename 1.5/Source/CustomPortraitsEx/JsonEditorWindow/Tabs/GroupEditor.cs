using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow.Tabs
{
    public class GroupEditor : TabBase
    {
        int stage = 0;
        string call_id = "";
        private string edit_target_group_name = "";
        Refs refs;
        private Dictionary<string, List<string>> temp_group_filter = new Dictionary<string, List<string>>();
        private List<string> temp_target_group_rows = new List<string>();
        private List<string> temp_remove_group_rows = new List<string>();

        public string result_edit_target_group_name = "";

        public void Draw(Rect inRect, List<string> selected_thoughts, List<string> selected_Interactions, List<string> result_interaction_filter, string selected_preset_name)
        {
            Listing_Standard listing = Begin(inRect);

            List<string> merged = new List<string>(selected_thoughts);
            merged.AddRange(result_interaction_filter);

            listing.Label("ここはポーンの心情名と前工程で名前を決めたインタラクト名をグループ化するところです");
            listing.Label("グループ化しないと心情名とインタラクト名の一つ一つに画像パスを書くことになり、それはとても大変です");
            listing.Label("そんなに大量の画像は作っていられないと思うので、ある程度同じような心情などは");
            listing.Label("一つのグループ名にして、そのグループ名で後工程の画像と紐づけるとためのものです");
            
            listing.GapLine();

            if (merged.Count <= 0)
            {
                listing.Label("「心情の選択」タブで一つ以上選択するか「インタラクトの振分」タブで振り分けてからこの画面を操作してください。");
            }else if(selected_Interactions.Count > 0 && result_interaction_filter.Count == 0)
            {
                listing.Label("「インタラクトの選択」タブで選択されています。");
                listing.Label("「インタラクトの振分」タブでインタラクトを送り手と受け手に振り分けてからグループ分けしてください。");
            }
            else if (selected_preset_name == "")
            {
                listing.Label("編集対象のプリセット名を一つ以上選択してください。");
            }
            else
            {
                switch (stage)
                {
                    case 0:
                        CreateOrEditGroup(listing, selected_preset_name);
                        break;
                    case 1:
                        EnterGroupName(listing);
                        break;
                    case 2:
                        EditGroup(listing, merged);
                        break;
                    case 3:
                        EndEditing(listing);
                        break;
                }

                SetStage();
            }

            End(listing);
        }

        private void CreateOrEditGroup(Listing_Standard listing, string selected_preset_name)
        {
            listing.Label("グループの新規作成");

            refs = PortraitCacheEx.Refs[selected_preset_name];

            if (listing.ButtonText("グループを新規作成する"))
            {
                call_id = "create";
            }

            temp_group_filter.Clear();
            foreach (var kv in refs.group_filter)
            {
                if (!temp_group_filter.ContainsKey(kv.Value))
                {
                    temp_group_filter[kv.Value] = new List<string>();
                }
                temp_group_filter[kv.Value].Add(kv.Key);
            }

            listing.Label("既存のグループ編集");
            foreach (var gf in temp_group_filter)
            {
                
                if (listing.ButtonText(gf.Key))
                {
                    call_id = "edit";
                    edit_target_group_name = gf.Key;
                    temp_target_group_rows = temp_group_filter[edit_target_group_name];
                }
            }
        }

        private void EnterGroupName(Listing_Standard listing)
        {
            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "create->back";
            }

            listing.GapLine();

            Rect enter_rect = listing.GetRect(30f);
            Widgets.Label(enter_rect.LeftPart(0.6f), "グループ名を入力してください:");

            listing.GapLine();

            Rect input_rect = listing.GetRect(30f);
            edit_target_group_name = Widgets.TextField(input_rect, edit_target_group_name);

            listing.GapLine();

            if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "決定"))
            {
                call_id = "create->enter name";
            }

        }

        private void EditGroup(Listing_Standard listing, List<string> merged)
        {
            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "edit->back";
            }

            listing.GapLine();

            listing.Label("どの心情名、インタラクト名をこのグループに含めるか選択してください");
            listing.Label("後ほどこのグループ名でどのテクスチャを表示するかを決定します");
            Rect selected_rect = listing.GetRect(30f);
            Widgets.Label(selected_rect.LeftPart(0.6f), "選択しているグループ名:");
            Widgets.Label(selected_rect.RightPart(0.15f), edit_target_group_name);

            listing.Label("追加済み");
            for (int i = temp_target_group_rows.Count - 1; i >= 0; i--)
            {
                var row = temp_target_group_rows[i];
                Rect row_rect = listing.GetRect(30f);
                Widgets.Label(row_rect.LeftPart(0.6f), "     ==>" + row);

                if (Widgets.ButtonText(row_rect.RightPart(0.55f).LeftPart(0.7f), "はずす"))
                {
                    temp_target_group_rows.RemoveAt(i);
                    if (!temp_remove_group_rows.Contains(row))
                    {
                        temp_remove_group_rows.Add(row);
                    }
                }
            }

            listing.GapLine();

            Rect push_rect = listing.GetRect(30f);
            Widgets.Label(push_rect.LeftPart(0.6f), "選択した心情とインタラクト");
            if(Widgets.ButtonText(push_rect.RightPart(0.55f).LeftPart(0.7f), "全部追加する"))
            {
                foreach (var row in merged)
                {

                    if (!temp_target_group_rows.Contains(row))
                    {
                        temp_target_group_rows.Add(row);
                    }
                }
            }

            listing.Gap();

            foreach (var row in merged)
            {
                

                Rect row_rect = listing.GetRect(30f);
                // ラベル
                Widgets.Label(row_rect.LeftPart(0.6f), "     ==>" + row);

                // SELECT ボタン
                if (Widgets.ButtonText(row_rect.RightPart(0.55f).LeftPart(0.7f), "追加する"))
                {
                    if (!temp_target_group_rows.Contains(row))
                    {
                        temp_target_group_rows.Add(row);
                    }
                }
            }

            listing.GapLine();

            listing.Label("一度グループから外したもの(ゴミ箱)");
            foreach (var row in temp_remove_group_rows)
            {

                Rect row_rect = listing.GetRect(30f);
                // ラベル
                Widgets.Label(row_rect.LeftPart(0.6f), "     ==>" + row);

                // SELECT ボタン
                if (Widgets.ButtonText(row_rect.RightPart(0.55f).LeftPart(0.7f), "追加する"))
                {
                    if (!temp_target_group_rows.Contains(row))
                    {
                        temp_target_group_rows.Add(row);
                    }
                }
            }
            listing.GapLine();
            Rect enter_rect = listing.GetRect(30f);
            
            if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "決定"))
            {
                if (temp_target_group_rows.Count > 0)
                {
                    result_edit_target_group_name = edit_target_group_name;
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

            listing.Label("ここの設定は終わりです。この画面の状態で「ポートレートの選択」タブを入力してください");
            listing.Label("もし入力内容に問題がある場合、お手数ですが「保存しないで戻る」を選択してください");

            listing.GapLine();
            listing.Label($"編集したグループの名前：{edit_target_group_name}");
            listing.Label($"このグループに含まれる心情やインタラクトの振り分けした名前：");
            foreach (var kv in temp_target_group_rows)
            {
                listing.Label($"    ==>{kv}");
            }

        }


        private void SetStage()
        {
            if(stage == 0)
            {
                if(call_id == "create")
                {
                    stage = 1;
                }
                else if (call_id == "edit")
                {
                    stage = 2;
                }
            }
            else if(stage == 1)
            {
                if(call_id == "create->enter name")
                {
                    stage = 2;
                    call_id = "edit";
                }
                else if(call_id == "create->back")
                {
                    Reset();
                    stage = 0;
                }
            }
            else if(stage == 2)
            {
                if(call_id == "edit->back")
                {
                    Reset();
                    stage = 0;
                }
                else if(call_id == "edit->end editing")
                {
                    stage = 3;
                }
            }else if(stage == 3)
            {
                if (call_id == "edit->end editing->back")
                {
                    Reset();
                    stage = 0;
                }
            }
        }

        private void Reset()
        {
            stage = 0;
            call_id = "";
            edit_target_group_name = "";
            result_edit_target_group_name = "";
            refs = null;
            temp_group_filter.Clear();
            temp_target_group_rows.Clear();
            temp_remove_group_rows.Clear();
        }
    }
}
