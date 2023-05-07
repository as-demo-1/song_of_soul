//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2020 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
	[RequireComponent(typeof(Rigidbody))]
	[AddComponentMenu("Koreographer/Demos/Musical Impulse")]
	public class MusicalImpulse : MonoBehaviour
	{
		[EventID]
		public string eventID;
		public float jumpSpeed = 3f;

		Rigidbody rigidbodyCom;

		void Start()
		{
			// Register for Koreography Events.  This sets up the callback.
			Koreographer.Instance.RegisterForEvents(eventID, AddImpulse);

			rigidbodyCom = GetComponent<Rigidbody>();
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
		
		void AddImpulse(KoreographyEvent evt)
		{
			// Add impulse by overriding the Vertical component of the Velocity.
			Vector3 vel = rigidbodyCom.velocity;
			vel.y = jumpSpeed;

			rigidbodyCom.velocity = vel;
		}
	}
}
