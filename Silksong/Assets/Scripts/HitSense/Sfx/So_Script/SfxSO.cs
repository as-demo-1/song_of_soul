using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Sfx_SO", menuName = "Sfx/Sfx_SO")]
public class SfxSO : ScriptableObject
{
	[SerializeField] private SfxGroup[] _SfxGroups = default;

	public GameObject[] GetSfxs()//every group return a sfx by order
	{
		int numberOfSfx = _SfxGroups.Length;
		GameObject[] resultingSfxs = new GameObject[numberOfSfx];
		for (int i = 0; i < numberOfSfx; i++)
		{
			resultingSfxs[i] = _SfxGroups[i].GetNextSfx();
		}

		return resultingSfxs;
	}
}


/// <summary>
/// Represents a group of AudioClips that can be treated as one, and provides automatic randomisation or sequencing based on the <c>SequenceMode</c> value.
/// </summary>
[System.Serializable]
public class SfxGroup
{
	public SequenceMode sequenceMode = SequenceMode.RandomNoImmediateRepeat;
	public GameObject[] SfxObjs;

	private int _nextToPlay = -1;
	private int _lastPlayed = -1;


	public GameObject GetNextSfx()
	{
		// Fast out if there is only one clip to play
		if (SfxObjs.Length == 1)
			return SfxObjs[0];

		if (_nextToPlay == -1)
		{
			// Index needs to be initialised: 0 if Sequential, random if otherwise
			_nextToPlay = (sequenceMode == SequenceMode.Sequential) ? 0 : UnityEngine.Random.Range(0, SfxObjs.Length);
		}
		else
		{
			// Select next index based on the appropriate SequenceMode
			switch (sequenceMode)
			{
				case SequenceMode.Random:
					_nextToPlay = UnityEngine.Random.Range(0, SfxObjs.Length);
					break;

				case SequenceMode.RandomNoImmediateRepeat:
					do
					{
						_nextToPlay = UnityEngine.Random.Range(0, SfxObjs.Length);
					} while (_nextToPlay == _lastPlayed);
					break;

				case SequenceMode.Sequential:
					_nextToPlay = (int)Mathf.Repeat(++_nextToPlay, SfxObjs.Length);
					break;
			}
		}

		_lastPlayed = _nextToPlay;

		return SfxObjs[_nextToPlay];
	}

}
