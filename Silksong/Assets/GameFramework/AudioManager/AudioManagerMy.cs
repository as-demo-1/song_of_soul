using Unity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class AudioManagerMy : Singleton<AudioManagerMy>{
    private AudioSource audioSource = null;
    public float bgmVolume = 0.2f;
    public float hitVolume = 1.0f;

    private GameObject soundObj;
    private List<AudioSource> audioList = new List<AudioSource>();

    
    public AudioManagerMy(){
        MonoManager.Instance.AddUpdateEvent(CheckAudioPlaying);
    }

    // 自动检测音效播放完毕
    public void CheckAudioPlaying(){
        for(int i = audioList.Count -1; i >=0; i-- ){
            if(!audioList[i].isPlaying){
                ChangeVolume(0.2f); // 恢复bgm音量
                GameObject.Destroy(audioList[i]);
                audioList.Remove(audioList[i]);
            }
        }
    }

    public void PlayBackgroundMusic(string name){
        if(audioSource == null){
            GameObject obj = new GameObject();
            obj.name = "BackgroundMusic";
            audioSource = obj.AddComponent<AudioSource>();
        }
        ResourceManager.Instance.LoadAsyn<AudioClip>("Sound/Back/" + name,(clip)=>{
            audioSource.clip = clip;
            audioSource.volume = bgmVolume;
            audioSource.loop = true;
            audioSource.Play();
        });
    }
    public void StopBackgroundMusic(){
        if(audioSource != null) {
            audioSource.Stop();
        }
    }
    public void ChangeVolume(float v){
        bgmVolume = v;
        if(audioSource!=null){
            audioSource.volume = v;
        }
    }

    public void PauseBackgroundMusic(){
        if(audioSource != null) {
            audioSource.Pause();
        }
    }
    public void PlayAudio(string name,UnityAction<AudioSource> callback=null){
        if(soundObj == null){
            soundObj = new GameObject();
            soundObj.name = "Sound";
        }

        ResourceManager.Instance.LoadAsyn<AudioClip>("Sound/"+name,(clip)=>{
            ChangeVolume(0.05f); // 播放音效的时候调小bgm
            AudioSource source = soundObj.AddComponent<AudioSource>();
            source.clip =clip;
            source.volume = hitVolume;
            source.Play();
            audioList.Add(source);
            if(callback != null){
                callback(source);
            }
        });
    }
    public void StopAudio(AudioSource audio){
        if(audioList.Contains(audio)){
            audioList.Remove(audio);
            audio.Stop();
            GameObject.Destroy(audio);
        }
    }

}