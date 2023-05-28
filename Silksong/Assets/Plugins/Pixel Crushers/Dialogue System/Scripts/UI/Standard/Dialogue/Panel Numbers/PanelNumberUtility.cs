// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Utility methods for working with StandardDialogueUI panel numbers.
    /// </summary>
    public static class PanelNumberUtility
    {

        public static SubtitlePanelNumber IntToSubtitlePanelNumber(int i)
        {
            // First three enum values are special, so increment 3:
            return (0 <= i && i <= 31) ? (SubtitlePanelNumber)(i+3) : SubtitlePanelNumber.Default;
        }

        public static MenuPanelNumber IntToMenuPanelNumber(int i)
        {
            // First two enum values are special, so increment 3:
            return (0 <= i && i <= 31) ? (MenuPanelNumber)(i + 2) : MenuPanelNumber.Default;
        }

        public static int GetSubtitlePanelIndex(SubtitlePanelNumber subtitlePanelNumber)
        {
            switch (subtitlePanelNumber)
            {
                case SubtitlePanelNumber.Panel0: return 0;
                case SubtitlePanelNumber.Panel1: return 1;
                case SubtitlePanelNumber.Panel2: return 2;
                case SubtitlePanelNumber.Panel3: return 3;
                case SubtitlePanelNumber.Panel4: return 4;
                case SubtitlePanelNumber.Panel5: return 5;
                case SubtitlePanelNumber.Panel6: return 6;
                case SubtitlePanelNumber.Panel7: return 7;
                case SubtitlePanelNumber.Panel8: return 8;
                case SubtitlePanelNumber.Panel9: return 9;
                case SubtitlePanelNumber.Panel10: return 10;
                case SubtitlePanelNumber.Panel11: return 11;
                case SubtitlePanelNumber.Panel12: return 12;
                case SubtitlePanelNumber.Panel13: return 13;
                case SubtitlePanelNumber.Panel14: return 14;
                case SubtitlePanelNumber.Panel15: return 15;
                case SubtitlePanelNumber.Panel16: return 16;
                case SubtitlePanelNumber.Panel17: return 17;
                case SubtitlePanelNumber.Panel18: return 18;
                case SubtitlePanelNumber.Panel19: return 19;
                case SubtitlePanelNumber.Panel20: return 20;
                case SubtitlePanelNumber.Panel21: return 21;
                case SubtitlePanelNumber.Panel22: return 22;
                case SubtitlePanelNumber.Panel23: return 23;
                case SubtitlePanelNumber.Panel24: return 24;
                case SubtitlePanelNumber.Panel25: return 25;
                case SubtitlePanelNumber.Panel26: return 26;
                case SubtitlePanelNumber.Panel27: return 27;
                case SubtitlePanelNumber.Panel28: return 28;
                case SubtitlePanelNumber.Panel29: return 29;
                case SubtitlePanelNumber.Panel30: return 30;
                case SubtitlePanelNumber.Panel31: return 31;
                default: return -1;
            }
        }

        public static int GetMenuPanelIndex(MenuPanelNumber menuPanelNumber)
        {
            switch (menuPanelNumber)
            {
                case MenuPanelNumber.Panel0: return 0;
                case MenuPanelNumber.Panel1: return 1;
                case MenuPanelNumber.Panel2: return 2;
                case MenuPanelNumber.Panel3: return 3;
                case MenuPanelNumber.Panel4: return 4;
                case MenuPanelNumber.Panel5: return 5;
                case MenuPanelNumber.Panel6: return 6;
                case MenuPanelNumber.Panel7: return 7;
                case MenuPanelNumber.Panel8: return 8;
                case MenuPanelNumber.Panel9: return 9;
                case MenuPanelNumber.Panel10: return 10;
                case MenuPanelNumber.Panel11: return 11;
                case MenuPanelNumber.Panel12: return 12;
                case MenuPanelNumber.Panel13: return 13;
                case MenuPanelNumber.Panel14: return 14;
                case MenuPanelNumber.Panel15: return 15;
                case MenuPanelNumber.Panel16: return 16;
                case MenuPanelNumber.Panel17: return 17;
                case MenuPanelNumber.Panel18: return 18;
                case MenuPanelNumber.Panel19: return 19;
                case MenuPanelNumber.Panel20: return 20;
                case MenuPanelNumber.Panel21: return 21;
                case MenuPanelNumber.Panel22: return 22;
                case MenuPanelNumber.Panel23: return 23;
                case MenuPanelNumber.Panel24: return 24;
                case MenuPanelNumber.Panel25: return 25;
                case MenuPanelNumber.Panel26: return 26;
                case MenuPanelNumber.Panel27: return 27;
                case MenuPanelNumber.Panel28: return 28;
                case MenuPanelNumber.Panel29: return 29;
                case MenuPanelNumber.Panel30: return 30;
                case MenuPanelNumber.Panel31: return 31;
                default: return -1;
            }
        }

    }
}
