using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound Manager", menuName = "Audio/Sound Manager")]
public class SoundManagerSO : ScriptableObject
{
    private static SoundManagerSO instance;

    public static SoundManagerSO Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<SoundManagerSO>("Sound Manager");
            return instance;
        }
    }

    public AudioSource SoundObject;

    private static float _volumeChangeMultiplier = 0.15f;
    private static float _pitchChangeMultiplier = 0.1f;

    public static void PlaySoundFXClip(AudioClip clip, Vector3 soundPos, float volume)
    {
        if (clip == null || Instance == null || Instance.SoundObject == null)
            return;

        float randVolume = Random.Range(
            volume - _volumeChangeMultiplier,
            volume + _volumeChangeMultiplier
        );

        float randPitch = Random.Range(
            1 - _pitchChangeMultiplier,
            1 + _pitchChangeMultiplier
        );

        AudioSource a = Instantiate(Instance.SoundObject, soundPos, Quaternion.identity);

        a.clip = clip;
        a.volume = randVolume;
        a.pitch = randPitch;
        a.Play();
    }

    public static void PlaySoundFXClip(AudioClip[] clips, Vector3 soundPos, float volume)
    {
        if (clips == null || clips.Length == 0 || Instance == null || Instance.SoundObject == null)
            return;

        AudioClip validClip = null;

        int safety = 0;
        while (safety < 10)
        {
            int rand = Random.Range(0, clips.Length);
            if (clips[rand] != null)
            {
                validClip = clips[rand];
                break;
            }
            safety++;
        }

        if (validClip == null)
            return;

        float randVolume = Random.Range(
            volume - _volumeChangeMultiplier,
            volume + _volumeChangeMultiplier
        );

        float randPitch = Random.Range(
            1 - _pitchChangeMultiplier,
            1 + _pitchChangeMultiplier
        );

        AudioSource a = Instantiate(Instance.SoundObject, soundPos, Quaternion.identity);

        a.clip = validClip;
        a.volume = randVolume;
        a.pitch = randPitch;
        a.Play();
    }
}