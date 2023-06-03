//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2020 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
	[AddComponentMenu("Koreographer/Demos/UI Message Setter")]
	public class UIMessageSetter : MonoBehaviour
	{
		[EventID]
		public string eventID;
		public GUIStyle style;

		KoreographyEvent curTextEvent;
		
		void Start()
		{
			// Register for Koreography Events.  This sets up the callback.
			Koreographer.Instance.RegisterForEventsWithTime(eventID, UpdateText);
		}
		
		void OnDestroy()
		{
			// Sometimes the Koreographer Instance gets cleaned up before hand.
			//  No need to worry in that case.
			if (Koreographer.Instance != null)
			{
				Koreographer.Instance.UnregisterForAllEvents(this);
			}
		}

		void OnGUI()
		{
			if (curTextEvent != null)
			{
				// Use the entire screen size as the draw surface. Draw location is determined by
				//	the GUIStyle.
				GUI.Box(new Rect(0,0, Screen.width, Screen.height), curTextEvent.GetTextValue(), style);
			}
		}
		
		void UpdateText(KoreographyEvent evt, int sampleTime, int sampleDelta, DeltaSlice deltaSlice)
		{
			// Verify that we have Text in the Payload.
			if (evt.HasTextPayload())
			{
				// Set the text if we have a text event!
				// We can get multiple events called at the same time (if they overlap in the track).
				//  In this case, we prefer the event with the most recent start sample.
				if (curTextEvent == null ||
				    (evt != curTextEvent && evt.StartSample > curTextEvent.StartSample))
				{
					// Store for later use and comparison.
					curTextEvent = evt;
				}

				// Clear out the text if our event ended this musical frame.
				if (curTextEvent.EndSample < sampleTime)
				{
					// Remove so that the above timing logic works when the audio loops/jumps.
					curTextEvent = null;
				}
			}
		}
	}
}
