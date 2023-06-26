//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2020 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
	[AddComponentMenu("Koreographer/Demos/Tempo Switch")]
	public class TempoSwitch : MonoBehaviour
	{
		// These should be set up in the inspector.
		public Behaviour[] quarterNoteGroup;
		public Behaviour[] eighthNoteGroup;

		int lastQuarterNote = 0;
		int lastEighthNote = -1;
		
		void Update()
		{
			// The Demo song has a quarter note as it's beat value.  This will get us the current
			//  quarter note!
			int curQuarterNote = Mathf.FloorToInt(Koreographer.GetBeatTime());

			if (curQuarterNote != lastQuarterNote)
			{
				// Turn the group on when the beat is an even number.
				SwitchGroup(quarterNoteGroup, lastQuarterNote % 2 != 0);

				lastQuarterNote = curQuarterNote;
			}

			// The 'null' value asks Koreographer to look at the beat time of what it considers
			//  the current "Main" song.  These demos use a basic player with a single song and
			//  define that as the Main song.  Therefore there is no need to specify it.  The
			//  '2' parameter, tells Koreographer to divide each beat into 2 equal parts.  As the
			//  base beat value is 4, this will result in eighth notes.
			int curEighthNote = Mathf.FloorToInt(Koreographer.GetBeatTime(null, 2));

			if (curEighthNote != lastEighthNote)
			{
				SwitchGroup(eighthNoteGroup, lastEighthNote % 2 != 0);

				lastEighthNote = curEighthNote;
			}
		}

		void SwitchGroup(Behaviour[] behaviours, bool bGroupOn)
		{
			for (int i = 0; i < behaviours.Length; ++i)
			{
				behaviours[i].enabled = bGroupOn;
			}
		}
	}
}
