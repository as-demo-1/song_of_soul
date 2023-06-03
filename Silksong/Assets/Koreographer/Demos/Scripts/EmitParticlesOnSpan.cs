//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2020 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
	[RequireComponent(typeof(ParticleSystem))]
	[AddComponentMenu("Koreographer/Demos/Emit Particles On Span")]
	public class EmitParticlesOnSpan : MonoBehaviour
	{
		[EventID]
		public string eventID;
		public float particlesPerBeat = 100;

		ParticleSystem particleCom;
		int lastEmitFrame = -1;

		void Start()
		{
			particleCom = GetComponent<ParticleSystem>();

			// Register for Koreography Events.  This sets up the callback.
			Koreographer.Instance.RegisterForEvents(eventID, OnParticleControlEvent);
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

		void OnParticleControlEvent(KoreographyEvent evt)
		{
			// If two Koreography span events overlap, this can be called twice in the same frame.
			//  This check ensures that we only ask the particle system to emit once for any frame.
			if (Time.frameCount != lastEmitFrame)
			{
				// Spans get called over a specified amount of music time.  Use Koreographer's beat delta
				//  to calculate the number of particles to emit this frame based on the "particlesPerBeat"
				//  rate configured in the Inspector.
				int particleCount = (int)(particlesPerBeat * Koreographer.GetBeatTimeDelta());

				// Emit the calculated number of particles!
				particleCom.Emit(particleCount);

				lastEmitFrame = Time.frameCount;
			}
		}
	}
}
