using CustomPortraits;
using Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow.Tabs;
using Newtonsoft.Json.Linq;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Foxy.CustomPortraits.CustomPortraitsEx.JsonEditorWindow
{
    [StaticConstructorOnStartup]
    public class JsonEditorWindow : Mod
    {


        private static int tab_int = 0;
        private ThoughtBrowser ThoughtBrowser = new ThoughtBrowser();
        private DashboardTab DashboardTab = new DashboardTab();
        private InteractionBrowser InteractionBrowser = new InteractionBrowser();
        private InteractionFilterEditor InteractionFilterEditor = new InteractionFilterEditor();
        private GroupEditor GroupEditor = new GroupEditor();
        private PortraitGroupEditor PortraitGroupEditor = new PortraitGroupEditor();
        private List<TabRecord> Tabs = new List<TabRecord>();

        public JsonEditorWindow(ModContentPack content) : base(content) { }

        public override string SettingsCategory()
        {
            return "RCPEditor".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            float height = inRect.height * 0.04f;
            Rect tabsRect = new Rect(inRect.x + inRect.width * 0.06f, inRect.y + height, inRect.width * 0.94f, height);
            if (Tabs == null)
            {
                Tabs = new List<TabRecord>();
            }
            if (Tabs.Empty())
            {
                Tabs.Add(new TabRecord("ダッシュボード", () =>
                {
                    SetTabInt(0);
                }, tab_int == 0));
                Tabs.Add(new TabRecord("心情の選択", () =>
                {
                    SetTabInt(1);
                }, tab_int == 1));
                Tabs.Add(new TabRecord("インタラクトの選択", () =>
                {
                    SetTabInt(2);
                }, tab_int == 2));
                Tabs.Add(new TabRecord("インタラクトの振分", () =>
                {
                    SetTabInt(3);
                }, tab_int == 3));
                Tabs.Add(new TabRecord("グループ化", () =>
                {
                    SetTabInt(4);
                }, tab_int == 4));
                Tabs.Add(new TabRecord("ポートレートの選択", () =>
                {
                    SetTabInt(5);
                }, tab_int == 5));
                Tabs.Add(new TabRecord("優先順位の設定", () =>
                {
                    SetTabInt(6);
                }, tab_int == 6));
            }
            TabDrawer.DrawTabs(tabsRect, Tabs);

            
            //Rect viewRect = new Rect(0, 0, inRect.width - 17f, scrollHeight);
            //Widgets.BeginScrollView(inRect, ref scroll, viewRect);
            //Listing_Standard listing = new Listing_Standard(inRect.AtZero(), () => scroll)
            //{
            //    maxOneColumn = true,
            //    ColumnWidth = viewRect.width
            //};
            //listing.Begin(viewRect);

            //----------------------Begin

            if (!PortraitCacheEx.IsAvailable)
            {
                //listing.Label("RCPError".Translate());
            }
            else
            {
                //listing.Label("RCPSelectPreset".Translate());

                //List<string> presets = new List<string>(PortraitCacheEx.Refs.Keys);
                //listing.Label("Select an item from the list:");

                //// リスト表示
                //foreach (var item in presets)
                //{
                //    if (listing.ButtonText(item))
                //    {
                //        // クリックされたら TextField にコピー
                //        input_text = item;
                //    }
                //}

                //listing.GapLine();


                //listing.Label("Edit selected JSON:");
                //listing.Label(input_text);

                switch (tab_int)
                {
                    case 0:
                        Tabs[0].selected = true;
                        DashboardTab.DrawEditorContent(inRect, ThoughtBrowser.selected_thoughts, InteractionBrowser.selected_Interactions);
                        break;
                    case 1:
                        Tabs[1].selected = true;
                        ThoughtBrowser.Draw(inRect);
                        break;
                    case 2:
                        Tabs[2].selected = true;
                        InteractionBrowser.Draw(inRect);
                        break;
                    case 3:
                        Tabs[3].selected = true;
                        InteractionFilterEditor.Draw(inRect, InteractionBrowser.selected_Interactions);
                        break;
                    case 4:
                        Tabs[4].selected = true;
                        GroupEditor.Draw(inRect, ThoughtBrowser.selected_thoughts, InteractionBrowser.selected_Interactions, InteractionFilterEditor.result_interaction_filter, DashboardTab.selected_preset_name);
                        break;
                    case 5:
                        Tabs[5].selected = true;
                        PortraitGroupEditor.Draw(inRect, GroupEditor.result_edit_target_group_name, DashboardTab.selected_preset_name);
                        break;
                }
            }

            //----------------------End
            //if (Event.current.type == EventType.Layout)
            //{
            //    scrollHeight = listing.CurHeight;
            //}

            //listing.End();
            //Widgets.EndScrollView();
        }

        private void SetTabInt(int i)
        {
            tab_int = i;
            if (i > Tabs.Count - 1)
            {
                return;
            }
            for (int a = 0; a < Tabs.Count; a++)
            {
                if (a == i)
                {
                    Tabs[a].selected = true;
                }
                else
                {
                    Tabs[a].selected = false;
                }
            }
        }
    }
}
