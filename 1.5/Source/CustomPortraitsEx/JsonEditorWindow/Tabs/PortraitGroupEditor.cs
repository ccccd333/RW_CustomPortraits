using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow.Tabs
{
    public class PortraitGroupEditor : TabBase
    {
        int stage = 0;
        Refs refs;
        bool edited_cache_flag = false;
        bool edited_idle_cache_flag = false;
        bool edited_dead_cache_flag = false;
        List<string> temp_files = new List<string>();
        TextureMeta temp_texture_meta = new TextureMeta();
        string temp_st_display_duration = "6.0";
        string temp_st_idle_display_duration = "6.0";
        string temp_st_dead_display_duration = "6.0";
        List<string> error_list = new List<string>();

        TextureMeta temp_texture_idle_meta = new TextureMeta();
        TextureMeta temp_texture_dead_meta = new TextureMeta();

        // グループ名はGroupEditor側のresult_edit_target_group_name
        // 今回のグループ名に対するテクスチャ群
        public TextureMeta result_texture_meta = new TextureMeta();
        // アイドル用のテクスチャ群
        public TextureMeta result_texture_idle_meta = new TextureMeta();
        // 死亡用のテクスチャ群
        public TextureMeta result_texture_dead_meta = new TextureMeta();

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
                        EditIdlePortrait(listing, selected_preset_name);
                        break;
                    case 2:
                        EditDeadPortrait(listing, selected_preset_name);
                        break;
                    case 3:
                        EndEditing(listing);
                        break;
                }

                SetStage();
            }

            End(listing);
        }

        public void Reset()
        {
            stage = 0;
            call_id = "";
            refs = null;
            edited_cache_flag = false;
            edited_idle_cache_flag = false;
            edited_dead_cache_flag = false;

            temp_files.Clear();
            temp_texture_meta = new TextureMeta();

            temp_texture_idle_meta = new TextureMeta();
            temp_texture_dead_meta = new TextureMeta();

            result_texture_meta = new TextureMeta();
            result_texture_idle_meta = new TextureMeta();
            result_texture_dead_meta = new TextureMeta();
            temp_st_display_duration = "6.0";
            temp_st_idle_display_duration = "6.0";
            temp_st_dead_display_duration = "6.0";

            error_list.Clear();
        }

        private void EditPortraitGroup(Listing_Standard listing, string edit_target_group_name, string selected_preset_name)
        {
            listing.Label("ポートレートとグループ名の紐づけ");
            listing.GapLine();

            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "back";
            }

            listing.GapLine();

            if (PortraitCacheEx.Refs.ContainsKey(selected_preset_name))
            {
                refs = PortraitCacheEx.Refs[selected_preset_name];
                if (refs.txs.ContainsKey(edit_target_group_name))
                {

                    listing.Label($"既に紐づいているグループが存在するため初期値はこちらを使用します：{edit_target_group_name}");
                    if (!edited_cache_flag)
                    {
                        var tx = refs.txs[edit_target_group_name];
                        temp_texture_meta = new TextureMeta(tx);
                        //tx.file_path_first;
                        temp_st_display_duration = temp_texture_meta.display_duration.ToString();
                        //Log.Message($"{temp_texture_meta.d} {temp_texture_meta.file_base_path} {temp_texture_meta.file_path_first} {temp_texture_meta.file_path_second} {temp_texture_meta.IsAnimation}");
                    }

                    edited_cache_flag = true;

                    listing.GapLine();
                }
            }

            PortraitEditTemplate(listing, temp_texture_meta, ref result_texture_meta, selected_preset_name, edit_target_group_name, ref temp_st_display_duration, "edit portrait group");

        }

        private void EditIdlePortrait(Listing_Standard listing, string selected_preset_name)
        {
            listing.Label("アイドル状態のポートレートの設定");
            listing.Label("重み設定で一つも状態が選ばれなかった場合に設定するアイドル用の画像です");
            listing.Label("毎回同じ心情の画像を表示ではなく、確率の場合例えば99%でも処理が抜けてきます");
            listing.Label("そのため何も選ばれなかった場合の画像の設定です");
            listing.GapLine();

            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "edit portrait group->back";
            }

            listing.GapLine();
            if (PortraitCacheEx.Refs.ContainsKey(selected_preset_name))
            {
                if (refs.txs.ContainsKey("Idle"))
                {

                    listing.Label($"既にIdleが存在するため初期値はこちらを使用します：Idle");
                    if (!edited_idle_cache_flag)
                    {
                        var tx = refs.txs["Idle"];
                        temp_texture_idle_meta = new TextureMeta(tx);
                        //tx.file_path_first;
                        temp_st_idle_display_duration = temp_texture_idle_meta.display_duration.ToString();
                        //Log.Message($"{temp_texture_meta.d} {temp_texture_meta.file_base_path} {temp_texture_meta.file_path_first} {temp_texture_meta.file_path_second} {temp_texture_meta.IsAnimation}");
                    }

                    edited_idle_cache_flag = true;

                    listing.GapLine();
                }
            }

            PortraitEditTemplate(listing, temp_texture_idle_meta, ref result_texture_idle_meta, selected_preset_name, "Idle", ref temp_st_idle_display_duration, "edit portrait group->edit idle portrait");
        }

        private void EditDeadPortrait(Listing_Standard listing, string selected_preset_name)
        {
            listing.Label("死亡状態のポートレートの設定");
            listing.Label("ポーンは死亡した段階で心情がなくなります");
            listing.Label("この状態では何もポートレートが表示できなくなるので死亡状態のポートレート設定を行います");
            listing.GapLine();

            Rect back_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(back_rect.RightPart(0.55f), "保存しないで戻る"))
            {
                call_id = "edit portrait group->edit dead->back";
            }

            listing.GapLine();
            if (PortraitCacheEx.Refs.ContainsKey(selected_preset_name))
            {
                if (refs.txs.ContainsKey("Dead"))
                {

                    listing.Label($"既にDeadが存在するため初期値はこちらを使用します：Dead");
                    if (!edited_dead_cache_flag)
                    {
                        var tx = refs.txs["Dead"];
                        temp_texture_dead_meta = new TextureMeta(tx);
                        //tx.file_path_first;
                        temp_st_dead_display_duration = temp_texture_dead_meta.display_duration.ToString();
                        //Log.Message($"{temp_texture_meta.d} {temp_texture_meta.file_base_path} {temp_texture_meta.file_path_first} {temp_texture_meta.file_path_second} {temp_texture_meta.IsAnimation}");
                    }

                    edited_dead_cache_flag = true;

                    listing.GapLine();
                }
            }

            PortraitEditTemplate(listing, temp_texture_dead_meta, ref result_texture_dead_meta, selected_preset_name, "Dead", ref temp_st_dead_display_duration, "edit end");
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

        private void PortraitEditTemplate(Listing_Standard listing, TextureMeta meta, ref TextureMeta result_meta, string selected_preset_name, string edit_target_group_name, ref string work_display_duration, string id)
        {
            listing.Label("ポートレートの基準となるパスです");
            listing.Label("フォーマットは「{Preset名}/{グループ名}/」となっております");
            listing.Label("この後Json再度読み込み時はここに画像を置いていく形になります");

            Rect base_path_rect = listing.GetRect(30f);
            meta.file_base_path = $"{selected_preset_name}/{edit_target_group_name}/";
            listing.Label(meta.file_base_path);
            //temp_texture_meta.file_base_path = Widgets.TextField(base_path_rect, temp_texture_meta.file_base_path);

            listing.GapLine();

            listing.Label("アニメーションポートレートの場合選択を押してください");

            Rect is_anim_rect = listing.GetRect(30f);

            // ラベル
            Widgets.Label(is_anim_rect.LeftPart(0.6f), "アニメーションにするか");

            // SELECT ボタン
            if (Widgets.ButtonText(is_anim_rect.RightPart(0.55f).LeftPart(0.7f), "SELECT"))
            {
                meta.IsAnimation = !meta.IsAnimation;
            }

            // チェックボックス
            bool is_anim = meta.IsAnimation;
            Rect checkbox_rect = is_anim_rect.RightPart(0.15f);
            Widgets.Checkbox(checkbox_rect.x, checkbox_rect.y, ref is_anim);

            listing.GapLine();
            listing.Label("現在表示中のポートレートが心情などによって別のポートレートに切り替わる時間を設定してください");

            Rect input_f_rect = listing.GetRect(30f);
            float rv = meta.display_duration;
            string display_dur_buff = work_display_duration;
            Widgets.TextFieldNumeric<float>(input_f_rect, ref rv, ref display_dur_buff);
            meta.display_duration = rv;
            work_display_duration = display_dur_buff;

            listing.GapLine();
            if (meta.IsAnimation)
            {
                listing.Label("アニメーション画像の場合from-toの番号を入れます");
                listing.Label("fromは1固定でtoはアニメーション画像の終端の画像名(1.dds~79.ddsの場合79)を入れます");

                meta.file_path_first = "1";
                listing.Label($"From==>{meta.file_path_first}");


                listing.Label($"To==>{meta.file_path_second}");

                Rect input_to_rect = listing.GetRect(30f);
                int to_tex = 2;
                if (!int.TryParse(meta.file_path_second, out to_tex)) { to_tex = 2; }

                Widgets.TextFieldNumeric<int>(input_to_rect, ref to_tex, ref meta.file_path_second);
                meta.file_path_second = to_tex.ToString();

                listing.GapLine();

                listing.Label("アニメーション画像の場合DDS(DXT1)固定です");
                meta.d = ".dds";
                listing.Label($"Extension==>{meta.d}");

                meta.file_path = meta.file_base_path + meta.file_path_first + meta.d + "~" + meta.file_path_second + meta.d;
            }
            else
            {
                listing.Label("状態変化時1枚の画像を表示する場合");
                listing.Label("fromは1固定でtoは何も設定しません。ですので「{Preset名}/{グループ名}/」に1.ddsもしくは1.pngを配置してください");

                meta.file_path_first = "1";
                listing.Label($"From==>{meta.file_path_first}");

                listing.Label("1枚の画像だけを表示する場合の画像の拡張子を選んでください");
                listing.Label($"Extension==>{meta.d}");

                Rect is_ext_dds_rect = listing.GetRect(30f);
                Widgets.Label(is_ext_dds_rect.LeftPart(0.6f), ".dds");

                // SELECT ボタン
                if (Widgets.ButtonText(is_ext_dds_rect.RightPart(0.55f).LeftPart(0.7f), "SELECT"))
                {
                    meta.d = ".dds";
                }

                bool is_dds = meta.d == ".dds" ? true : false;
                // チェックボックス
                Rect checkbox_rect1 = is_ext_dds_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect1.x, checkbox_rect1.y, ref is_dds);

                Rect is_ext_png_rect = listing.GetRect(30f);
                Widgets.Label(is_ext_png_rect.LeftPart(0.6f), ".png");

                // SELECT ボタン
                if (Widgets.ButtonText(is_ext_png_rect.RightPart(0.55f).LeftPart(0.7f), "SELECT"))
                {
                    meta.d = ".png";
                }

                bool is_png = meta.d == ".png" ? true : false;
                // チェックボックス
                Rect checkbox_rect2 = is_ext_png_rect.RightPart(0.15f);
                Widgets.Checkbox(checkbox_rect2.x, checkbox_rect2.y, ref is_png);

                meta.file_path = meta.file_base_path + meta.file_path_first + meta.d;
            }
            listing.GapLine();

            if (error_list.Count > 0)
            {
                foreach (var error in error_list)
                {
                    listing.Label(error);
                }
            }

            Rect enter_rect = listing.GetRect(30f);
            if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "決定"))
            {
                bool check = true;
                error_list.Clear();
                if (meta.IsAnimation)
                {
                    if (meta.d != ".dds")
                    {
                        error_list.Add("[ERROR] animationなのにDDSじゃない[これ見かけたら報告ください]");
                        check = false;
                    }

                    if (meta.file_base_path == "")
                    {
                        error_list.Add("[ERROR] file_base_pathがない[これ見かけたら報告ください]");
                        check = false;
                    }

                    if (meta.file_path_first != "1")
                    {
                        error_list.Add("[ERROR] file_path_firstが1以外になってる[これ見かけたら報告ください]");
                        check = false;

                    }
                    int outint = -1;
                    if (int.TryParse(meta.file_path_second, out outint))
                    {
                        if (outint < 1)
                        {
                            error_list.Add("[ERROR] from-toのto部分がfrom部分より小さい値になってます");
                            check = false;
                        }
                    }

                    if (meta.display_duration < 0.01f)
                    {
                        error_list.Add("[ERROR] 切り替わり時間は1秒以上に設定してください");
                        check = false;
                    }
                }
                else
                {

                    if (meta.d != ".dds" && meta.d != ".png")
                    {
                        error_list.Add("[ERROR] 拡張子がDDSとPNG以外[これ見かけたら報告ください]");
                        check = false;

                    }

                    if (meta.file_base_path == "")
                    {
                        error_list.Add("[ERROR] file_base_pathがない[これ見かけたら報告ください]");
                        check = false;
                    }

                    if (meta.file_path_first != "1")
                    {
                        error_list.Add("[ERROR] file_path_firstが1以外になってる[これ見かけたら報告ください]");
                        check = false;
                    }

                    if (meta.display_duration < 0.01f)
                    {
                        error_list.Add("[ERROR] 切り替わり時間は1秒以上に設定してください");
                        check = false;
                    }
                }

                if (check)
                {
                    call_id = id;
                    result_meta = new TextureMeta(meta);
                }
            }
        }

        private void SetStage()
        {
            if (stage == 0)
            {
                if (call_id == "edit portrait group")
                {
                    stage = 1;
                }
                else if (call_id == "back")
                {
                    Reset();
                    stage = 0;
                }
            }
            else if (stage == 1)
            {
                if (call_id == "edit portrait group->edit idle portrait")
                {
                    stage = 2;
                }
                else if (call_id == "edit portrait group->back")
                {
                    Reset();
                    stage = 0;
                }
            }
            else if (stage == 2)
            {
                if (call_id == "edit end")
                {
                    stage = 3;
                }
                else if (call_id == "edit portrait group->edit dead->back")
                {
                    Reset();
                    stage = 0;
                }
            }
            else if (stage == 3)
            {
                if (call_id == "edit end->back")
                {
                    Reset();
                    stage = 0;
                }
            }
        }


    }
}
