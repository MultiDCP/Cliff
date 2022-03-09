using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_M : MonoBehaviour {

    public AudioClip audioUI;
    private AudioSource theAudio;

    public void Title()
    {
        Sound();
        SceneManager.LoadScene("SampleScene");
    }

    public void Quit()
    {
        Sound();
        Application.Quit();
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
