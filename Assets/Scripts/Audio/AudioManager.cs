using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    public AudioSource AS;
    public AudioSource Tip_AS;

    private AudioClip enterClip,clickClip;

    private void Awake()
    {
        if (Tip_AS == null)
        {
            Tip_AS = gameObject.AddComponent<AudioSource>();
            Tip_AS.loop = false;
            Tip_AS.volume = 1f;

        }
        if (AS == null)
        {
            AS = gameObject.AddComponent<AudioSource>();
            AS.loop = false;
            AS.playOnAwake = false;
            AS.volume = 1f;

        }
        DontDestroyOnLoad(this);
    }
    /// <summary>
    /// 播放UI音效
    /// </summary>
    /// <param name="clip"></param>
    public void ASPlayer(AudioClip clip,bool isloop=false)
    {
        AS.clip = clip;
        AS.Play();
        AS.loop = isloop;
    }
    /// <summary>
    /// 继续播放
    /// </summary>
    public void ASPlayerPlay()
    {
        AS.Play();
    }
    public void ASPlayer()
    {
        if (AS.isPlaying)
            ASPlayerPause();
        else
            ASPlayerPlay();
    }
    /// <summary>
    /// 结束播放
    /// </summary>
    public void ASPlayerStop()
    {
        AS.Stop();
    }
    /// <summary>
    /// 暂停播放
    /// </summary>
    public void ASPlayerPause()
    {
        AS.Pause();
    }
    public void UIEnter()
    {
        if (enterClip == null) 
            ClipInit();
        Tip_AS.PlayOneShot(enterClip);
    }
    public void UIClick()
    {
        if (clickClip == null)
            ClipInit();
        Tip_AS.PlayOneShot(clickClip);
    }
    private void ClipInit() 
    {
        //获取需要播放的音效
        if (enterClip == null) 
        {
         enterClip=   Resources.Load("Audios/EnterClip") as AudioClip;
        }
        //获取需要播放的音效
        if (clickClip == null)
        {
            clickClip = Resources.Load("Audios/ClickClip") as AudioClip;
        }
    } 
}
