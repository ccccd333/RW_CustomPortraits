using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow.Tabs
{
    public class WriteJsonErrorWindow : TabBase
    {
        int stage = 0;
        public void Draw(Rect inRect, string preset_name, List<string> error_message1, List<string> error_message2)
        {
            Listing_Standard listing = Begin(inRect);



            //if (error_message1.Count == 0 && error_message2.Count == 0)
            //{
            //    call_id = "end";
            //}
            //else
            {
                Rect enter_rect = listing.GetRect(30f);
                if (Widgets.ButtonText(enter_rect.RightPart(0.55f).LeftPart(0.7f), "決定"))
                {
                    call_id = "end";
                }
                else
                {
                    if (error_message1.Count == 0 && error_message2.Count == 0)
                    {
                        listing.Label("エラーはないです決定ボタンを押下してください");
                        call_id = "no error";
                    }
                    else
                    {
                        listing.Label("JSON書き込み後の再読取りで問題が起きた際のメッセージです");
                        listing.Label("大体はプリセットの画像がない場合と思われます。Player.logを見てみると何処で止まっているかわかります");
                        listing.Label("テクスチャがない以外でエラーが出るとそのプリセットは読み取れないため手修正もしくはバックアップフォルダで上書き後、ゲームの再起動をお願いします。");
                        listing.Label("エラーの詳細は「RCP Reload JSON And Check Errors」で確認お願いします");
                        listing.GapLine();

                        listing.Label($"プリセット名 ==> [{preset_name}]");
                        foreach (string error in error_message1)
                        {
                            listing.Label($"ERROR MESSAGE ==> [{error}]");
                        }

                        listing.GapLine();

                        listing.Label($"プリセット名 ==> [InteractionFilter.json]");
                        foreach (string error in error_message2)
                        {
                            listing.Label($"ERROR MESSAGE ==> [{error}]");

                        }

                        call_id = "error";
                    }

                }
            }

            End(listing);
        }

        public void Reset()
        {
            call_id = "";
        }
    }
}
