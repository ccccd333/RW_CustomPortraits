using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow.Tabs
{
    public class PortraitGroupEditor : TabBase
    {
        int stage = 0;
        string call_id = "";
        Refs refs;
        bool edited_cache_flag = false;
        bool temp_is_animation_mode = false;
        List<string> temp_files = new List<string>();
        TextureMeta temp_texture_meta = new TextureMeta();
        string temp_st_display_duration = "";

        TextureMeta result_texture_meta = new TextureMeta();

        public void Draw(Rect inRect, string edit_target_group_name, string selected_preset_name)
        {
            Listing_Standard listing = Begin(inRect);
            listing.Label("ここは「ポートレートの選択」タブです");
            listing.Label("心情などを前工程でグループ化されているため、そのグループ名とここに定義するポートレートを紐づけます");
            listing.Label("例えば世間話ならこのポートレート、侮辱されたならこのポートレートみたいな設定です");
            listing.Label("アニメーションの場合は複数のDDS(DXT1)画像のみとなります");
            listing.Label("アニメーション以外の場合はPNGもしくはDDS(DXT1)画像となります");
            listing.GapLine();

            if (edit_target_group_name == "")
            {
                listing.Label("「グループ化」タブが完了後にこのタブで編集してください");
            }
            else
            {
                switch (stage)
                {
                    case 0:
                        EditPortraitGroup(listing, edit_target_group_name, selected_preset_name);
                        break;
                    case 1:
                        EndEditing(listing);
                        break;
                }

                SetStage();
            }

            End(listing);
        }

        private void EditPortraitGroup(Listing_Standard listing, string edit_target_group_name, string selected_preset_name)
        {
            listing.Label("ポートレートとグループ名の紐づけ");
            listing.GapLine();

            refs = PortraitCacheEx.Refs[selected_preset_name];
            if (refs.txs.ContainsKey(edit_target_group_name))
            {
                
                listing.Label($"既に紐づいているグループが存在するため初期値はこちらを使用します：{edit_target_group_name}");
                if (!edited_cache_flag)
                {
                    var tx = refs.txs[edit_target_group_name];
                    temp_texture_meta = new TextureMeta(tx);
                    //tx.file_path_first;

                    Log.Message($"{temp_texture_meta.d} {temp_texture_meta.file_base_path} {temp_texture_meta.file_path_first} {temp_texture_meta.file_path_second} {temp_texture_meta.IsAnimation}");
                }

                edited_cache_flag = true;

                listing.GapLine();
            }

            listing.Label("ポートレートの基準となるパスです");
            listing.Label("フォーマットは「{Preset名}/{グループ名}/」となっております");
            listing.Label("この後Json再度読み込み時はここに画像を置いていく形になります");

            Rect base_path_rect = listing.GetRect(30f);
            temp_texture_meta.file_base_path = $"{selected_preset_name}/{edit_target_group_name}/";
            listing.Label(temp_texture_meta.file_base_path);
            //temp_texture_meta.file_base_path = Widgets.TextField(base_path_rect, temp_texture_meta.file_base_path);

            listing.GapLine();

            listing.Label("アニメーションポートレートの場合選択を押してください");

            Rect is_anim_rect = listing.GetRect(30f);

            // ラベル
            Widgets.Label(is_anim_rect.LeftPart(0.6f), "アニメーションにするか");

            // SELECT ボタン
            if (Widgets.ButtonText(is_anim_rect.RightPart(0.55f).LeftPart(0.7f), "SELECT"))
            {
                temp_texture_meta.IsAnimation = !temp_texture_meta.IsAnimation;
            }

            // チェックボックス
            bool is_anim = temp_texture_meta.IsAnimation;
            Rect checkbox_rect = is_anim_rect.RightPart(0.15f);
            Widgets.Checkbox(checkbox_rect.x, checkbox_rect.y, ref is_anim);

            listing.GapLine();
            listing.Label("現在表示中のポートレートが心情などによって別のポートレートに切り替わる時間を設定してください");

            Rect input_f_rect = listing.GetRect(30f);
            float rv = temp_texture_meta.display_duration;
            Widgets.TextFieldNumeric<float>(input_f_rect, ref rv, ref temp_st_display_duration);
            temp_texture_meta.display_duration = rv;

            listing.GapLine();
            if (temp_texture_meta.IsAnimation)
            {
                listing.Label("アニメーション画像の場合from-toの番号を入れます");
                listing.Label("fromは1固定でtoはアニメーション画像の終端の画像名(1.dds~79.ddsの場合79)を入れます");

                temp_texture_meta.file_path_first = "1";
                listing.Label($"From==>{temp_texture_meta.file_path_first}");


                listing.Label($"To==>{temp_texture_meta.file_path_second}");

                Rect input_to_rect = listing.GetRect(30f);
                int to_tex = int.Parse(temp_texture_meta.file_path_second);
                Widgets.TextFieldNumeric<int>(input_to_rect, ref to_tex, ref temp_texture_meta.file_path_second);
                temp_texture_meta.file_path_second = to_tex.ToString();

                listing.GapLine();

                listing.Label("アニメーション画像の場合DDS(DXT1)固定です");
                temp_texture_meta.d = ".dds";
                listing.Label($"Extension==>{temp_texture_meta.d}");

                temp_texture_meta.file_path = temp_texture_meta.file_base_path + temp_texture_meta.file_path_first + temp_texture_meta.d + "~" + temp_texture_meta.file_path_second + temp_texture_meta.d;
            }
            else
            {
                listing.Label("状態変化時1枚の画像を表示する場合");
                listing.Label("fromは1固定でtoは何も設定しません。ですので「{Preset名}/{グループ名}/」に1.ddsもしくは1.pngを配置してください");

                temp_texture_meta.file_path_first = "1";
                listing.Label($"From==>{temp_texture_meta.file_path_first}");

                //Rect input_to_rect = listing.GetRect(30f);
                //int to_tex = int.Parse(temp_texture_meta.file_path_second);
                //Widgets.TextFieldNumeric<int>(input_to_rect, ref to_tex, ref temp_texture_meta.file_path_second);
                //temp_texture_meta.file_path_second = to_tex.ToString();
                //listing.Label($"To==>{temp_texture_meta.file_path_second}");
                //listing.GapLine();

                listing.Label("1枚の画像だけを表示する場合の画像の拡張子を選んでください");
                listing.Label($"Extension==>{temp_texture_meta.d}");

                Rect is_ext_dds_rect = listing.GetRect(30f);
                Widgets.Label(is_ext_dds_rect.LeftPart(0.6f), ".dds");

                // SELECT ボタン
                if (Widgets.ButtonText(is_ext_dds_rect.RightPart(0.55f).LeftPart(0.7f), "SELECT"))
                {
                    temp_texture_meta.d = ".dds";
                }

                bool is_dds = temp_texture_meta.d == ".dds" ? true : false;
                // チェックボックス
                Rect checkbox_rect1 = is_ext_dds_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect1.x, checkbox_rect1.y, ref is_dds);

                Rect is_ext_png_rect = listing.GetRect(30f);
                Widgets.Label(is_ext_png_rect.LeftPart(0.6f), ".png");

                // SELECT ボタン
                if (Widgets.ButtonText(is_ext_png_rect.RightPart(0.55f).LeftPart(0.7f), "SELECT"))
                {
                    temp_texture_meta.d = ".png";
                }

                bool is_png = temp_texture_meta.d == ".png" ? true : false;
                // チェックボックス
                Rect checkbox_rect2 = is_ext_png_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect2.x, checkbox_rect2.y, ref is_png);

                temp_texture_meta.file_path = temp_texture_meta.file_base_path + temp_texture_meta.file_path_first + temp_texture_meta.d;
            }

            Rect enter_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "決定"))
            {
                bool check = true;

                if (temp_texture_meta.IsAnimation)
                {
                    if (temp_texture_meta.d != ".dds") check = false;

                    if (temp_texture_meta.file_base_path == "") check = false;

                    if(temp_texture_meta.file_path_first != "1") check = false;
                    int outint = -1;
                    if (int.TryParse(temp_texture_meta.file_path_second, out outint))
                    {
                        if (outint < 1) check = false;
                    }

                    if(temp_texture_meta.display_duration < 0.01f) check = false;
                }
                else
                {
                    
                    if (temp_texture_meta.d != ".dds" || temp_texture_meta.d != ".png") check = false;

                    if (temp_texture_meta.file_base_path == "") check = false;

                    if (temp_texture_meta.file_path_first != "1") check = false;

                    if (temp_texture_meta.display_duration < 0.01f) check = false;
                }

                if (check)
                {
                    call_id = "edit end";
                    result_texture_meta = new TextureMeta(temp_texture_meta);
                }
            }
        }

        private void EndEditing(Listing_Standard listing)
        {
            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "edit end->back";
            }

            listing.GapLine();

            listing.Label("ここの設定は終わりです。この画面の状態で「ポートレートの表示する優先順位を設定」タブを入力してください");
            listing.Label("もし入力内容に問題がある場合、お手数ですが「保存しないで戻る」を選択してください");

            listing.GapLine();
            listing.Label($"ポートレートのパス：{result_texture_meta.file_path}");
            listing.Label($"アニメーション画像か：{result_texture_meta.IsAnimation}");
            listing.Label($"画面表示時間(秒)：{result_texture_meta.display_duration}");
        }

        private void SetStage()
        {
            if (stage == 0)
            {
                if (call_id == "edit end")
                {
                    stage = 1;
                }
            }
            else if (stage == 1)
            {
                if (call_id == "edit end->back")
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
            refs = null;
            edited_cache_flag = false;

            temp_is_animation_mode = false;
            temp_files.Clear();
            temp_texture_meta = new TextureMeta();
            result_texture_meta = new TextureMeta();
            temp_st_display_duration = "";
        }
    }
}
