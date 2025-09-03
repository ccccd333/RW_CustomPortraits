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
    public class ReloadJsonWithErrorsWindow : Mod
    {
        private static int tab_int = 0;
        private List<TabRecord> Tabs = new List<TabRecord>();

        public ReloadJsonWithErrorsWindow(ModContentPack content) : base(content) { }

        public override string SettingsCategory()
        {
            return "RCP Reload JSON And Check Errors";
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
                Tabs.Add(new TabRecord("プリセット一覧", () =>
                {
                    SetTabInt(0);
                }, tab_int == 0));
                Tabs.Add(new TabRecord("JSONの再読み込み", () =>
                {
                    SetTabInt(1);
                }, tab_int == 1));
                Tabs.Add(new TabRecord("エラー一覧", () =>
                {
                    SetTabInt(2);
                }, tab_int == 2));
            }
            TabDrawer.DrawTabs(tabsRect, Tabs);
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
