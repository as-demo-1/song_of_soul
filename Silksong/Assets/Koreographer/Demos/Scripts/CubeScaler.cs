//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2020 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
	[AddComponentMenu("Koreographer/Demos/Cube Scaler")]
	public class CubeScaler : MonoBehaviour
	{
		[EventID]
		public string eventID;
		public float minScale = 0.5f;
		public float maxScale = 1.5f;
		
		void Start()
		{
			// Register for Koreography Events.  This sets up the callback.
			Koreographer.Instance.RegisterForEventsWithTime(eventID, AdjustScale);
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
		
		void AdjustScale(KoreographyEvent evt, int sampleTime, int sampleDelta, DeltaSlice deltaSlice)
		{
			if (evt.HasCurvePayload())
			{
				// Get the value of the curve at the current audio position.  This will be a
				//  value between [0, 1] and will be used, below, to interpolate between
				//  minScale and maxScale.
				float curveValue = evt.GetValueOfCurveAtTime(sampleTime);

				transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, curveValue);
			}
		}
	}
}
