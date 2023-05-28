#if USE_AURORA
// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Aurora
{

    /// <summary>
    /// This static class defines the predefined Aurora Toolset tokens and their Variable[] table counterparts.
    /// </summary>
    public static class AuroraPredefinedTokens
    {

        public static Dictionary<string, string> all { get { return m_all; } }

        /// @cond FOR_V1_COMPATIBILITY
        public static Dictionary<string, string> All { get { return all; } }
        /// @endcond
        
        private static Dictionary<string, string> m_all = new Dictionary<string, string>() {
            { "<FirstName>", "FirstName" }, // The player characacter's first name.
			{ "<LastName>", "LastName" }, // The PC's Last name.
			{ "<FullName>", "FullName" }, // The PC's First and Last Name
			{ "<Subrace>", "Subrace" }, // gives the PC's subrace (if he/she entered one)
			{ "<Diety>", "Deity" }, // names the PC's chosen Diety (if he/she entered one)
			{ "<Alignment>", "Alignment" }, // Names the PC's Alignment.
			{ "<Lawful/Chaotic>", "Lawful_Chaotic" }, // Gives whether the PC is Neutral, lawful or chaotic.
			{ "<Law/Chaos>", "Law_Chaos" }, // gives if the PC is to Law, Neutrality, or Chaos.
			{ "<Good/Evil>", "Good_Evil" }, // gives the PC's Good/Neutral/Evil.
			{ "<Class>", "Class" }, // Names the PC's class (Wizard, Fighter, Monk etc)
			{ "<Race>", "Race" }, // Names the PC's Race (Elven, Gnome, Human, Half-Elven)
			{ "<Man/Woman>", "Man_Woman" },
            { "<Male/Female>", "Male_Female" },
            { "<Boy/Girl>", "Boy_Girl" },
            { "<Mister/Missus>", "Mister_Missus" },
            { "<Master/Mistress>", "Master_Mistress" },
            { "<Lord/Lady>", "Lord_Lady" },
            { "<Lad/Lass>", "Lad_Lass" },
            { "<Sir/Madam>", "Sir_Madam" },
            { "<Him/Her>", "Him_Her" },
            { "<His/Her>", "His_Her" },
            { "<His/Hers>", "His_Hers" },
            { "<He/She>", "He_She" },
            { "<Brother/Sister>", "Brother_Sister" },
            { "<GameTime>", "GameTime" }, // gives the time in-game.
			{ "<GameYear>", "GameYear" }, // gives the year in the game.
			{ "<GameMonth>", "GameMonth" }, // gives the month in the game.
			{ "<QuarterDay>", "QuarterDay" }, // gives Morning, Evening, Afternoon, Night.
			{ "<Level>", "Level" }
        };

    }
}
#endif
