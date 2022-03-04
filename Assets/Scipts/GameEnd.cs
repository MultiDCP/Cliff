﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnd : MonoBehaviour {

    public AudioClip audioUI;
    private AudioSource theAudio;

    public void Title()
    {
        Sound();
        SceneManager.LoadScene("SampleScene");
    }

    public void Again()
    {
        Sound();
        SceneManager.LoadScene("InGame");
    }
    public void Start()
    {
        Screen.SetResolution(Screen.width, Screen.width * 16 / 9, true);
        theAudio = gameObject.AddComponent<AudioSource>();
    }

    public void Sound()
    {
        theAudio.clip = audioUI;
        theAudio.Play();
    }
}
