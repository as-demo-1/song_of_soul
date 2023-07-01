using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Sfx_SO", menuName = "Sfx/Sfx_SO")]
public class SfxSO : ScriptableObject
{
	[SerializeField] private SfxGroup[] _SfxGroups = default;

	public GameObject[] GetSfxs()//every group return a sfx by order
		//批量化读取对应特效单例，估计为了部分重复或者随机特效准备的
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
/// 果然，包含了自动的随机或者序列性的特效及对应音频
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
			//这里的参数0代表序列，其余代表随机
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
					//稍微有点不一样，这里0确实是随机，但是这里还留了个随机但是下一个不与上一个重复的分支
					do
					{
						_nextToPlay = UnityEngine.Random.Range(0, SfxObjs.Length);
					} while (_nextToPlay == _lastPlayed);
					break;

				case SequenceMode.Sequential:
					//序列播放
					_nextToPlay = (int)Mathf.Repeat(++_nextToPlay, SfxObjs.Length);
					break;
			}
		}

		_lastPlayed = _nextToPlay;

		return SfxObjs[_nextToPlay];
	}

}
