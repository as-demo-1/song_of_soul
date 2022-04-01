using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private string tagToDetect = "Player";
    [SerializeField] private string title;

	[SerializeField] private TutorialPanelSO TutorialPanelSO;

	private void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log("Trigger");
		if (other.CompareTag(tagToDetect))
		{
			TutorialPanelSO.RasieEvent(title);
			Debug.Log("Player Trigger");
		}

	}
}
