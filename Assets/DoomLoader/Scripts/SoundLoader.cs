﻿using UnityEngine;
using System.Collections.Generic;

public class SoundLoader : MonoBehaviour 
{
    public static SoundLoader Instance;
    public Dictionary<string, AudioClip> Sounds = new Dictionary<string, AudioClip>();

    public SoundOverride[] OverrideSounds = new SoundOverride[0];

    [System.Serializable]
    public struct SoundOverride
    {
        public string SoundName;
        public AudioClip Sound;
    }

    void Awake()
    {
        Instance = this;

        foreach (SoundOverride so in OverrideSounds)
            Sounds.Add(so.SoundName, so.Sound);
    }

    public AudioClip LoadSound(string soundName)
    {
        if (Sounds.ContainsKey(soundName))
            return Sounds[soundName];

        foreach (Lump l in WadLoader.lumps)
        {
            if (l.lumpName != soundName)
                continue;

            int p = 0;
            int format = l.data[p++] | ((int)l.data[p++]) << 8;
            if (format != 3)
            {
                Debug.Log("SoundLoader: LoadSound: \"" + soundName + "\" format != 3");
                return null;
            }

            int samplerate = l.data[p++] | ((int)l.data[p++]) << 8;
            int count = (int)(l.data[p++] | (int)l.data[p++] << 8 | (int)l.data[p++] << 16 | (int)l.data[p++] << 24);
            count -= 32; //sound lumps have 16 bytes before and after samples as padding

            p += 16; //padding
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
                samples[i] = (float)l.data[p++] / 128f - 1f;

            AudioClip clip = AudioClip.Create(soundName, count, 1, samplerate, false);
            clip.SetData(samples, 0);

            Sounds.Add(l.lumpName, clip);
            return clip;
        }

        Debug.Log("SoundLoader: LoadSound: Could not find sound \"" + soundName + "\"");
        return null;
    }
}
