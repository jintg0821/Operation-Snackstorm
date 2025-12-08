using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;


    public void SetMasterVolume()
    {
        float value = Mathf.Clamp(masterSlider.value, 0.0001f, 1f);
        audioMixer.SetFloat("Master", Mathf.Log10(value) * 20);
    }
    public void SetBGMVolume()
    {
        float value = Mathf.Clamp(bgmSlider.value, 0.0001f, 1f);
        audioMixer.SetFloat("BGM", Mathf.Log10(value) * 20);
    }
    public void SetSfxVolume()
    {
        float value = Mathf.Clamp(sfxSlider.value, 0.0001f, 1f);
        audioMixer.SetFloat("SFX", Mathf.Log10(value) * 20);
    }
}