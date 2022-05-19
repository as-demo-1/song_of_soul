using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AudioPlay : MonoBehaviour
{
    private static AudioPlay _instance;
    public static AudioPlay Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AudioPlay();
            }

            return _instance;
        }
    }

    readonly string datapath = "Assets/Resources/Audio";
    public static List<string> filename = new List<string>();
    public static Dictionary<string, string> ends = new Dictionary<string, string>();
    AudioSource voicePlayer;


    // Start is called before the first frame update
    void Start()
    {
        //voicePlayer = (AudioSource)GameObject.Find("UI").GetComponent<AudioSource>();
        if (Directory.Exists(datapath))
        {
            DirectoryInfo direction = new DirectoryInfo(datapath);
            FileInfo[] files = direction.GetFiles("*");
            for (int i = 0; i < files.Length; i++)
            {
                //去除Unity内部.meta文件
                if (files[i].Name.EndsWith(".meta"))
                    continue;
                else
                {
                    filename.Add(files[i].Name.Substring(0, files[i].Name.Length - 4));
                    ends.Add(files[i].Name.Substring(0, files[i].Name.Length - 4), files[i].Name.Substring(files[i].Name.Length - 4));
                }
            }
        }
    }




    public void PlayAudio(string id)
    {
        
        float voiceVolumeValue = 0.5f;
        voicePlayer = (AudioSource)GameObject.Find("UI").GetComponent<AudioSource>();
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + id);
        voicePlayer.clip = clip;
        voicePlayer.Play();
    }
}
