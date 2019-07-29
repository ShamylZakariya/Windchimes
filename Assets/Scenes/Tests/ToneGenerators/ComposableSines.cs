using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ComposableSines : MonoBehaviour
{
    [Serializable]
    public struct SineWaveSource
    {
        public float frequency;
        [Range(0, 1)]
        public float gain;
        [Range(0, Mathf.PI * 2)]
        public float phaseOffset;
        public AnimationCurve envelope;
        public bool loop;

        //
        //  Private statue
        //

        internal float phase;
        internal double startTime;
        internal double endTime;
    }

    [SerializeField]
    SineWaveSource[] sines = null;

    [SerializeField]
    float frequencyMultiplier = 1f;

    //
    //  Private statue
    //

    private AudioSource _source;
    private float _sampleRate = 0;
    const float TWO_PI = Mathf.PI * 2;
    bool _toneCompleted;


    void Start()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.Stop();
    }

    void Update()
    {
        if (_toneCompleted && _source.isPlaying)
        {
            _source.Stop();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_source.isPlaying)
            {
                PlayTone();
            }
            else
            {
                _source.Stop();
            }
        }
    }

    void PlayTone()
    {
        for (int i = 0; i < sines.Length; i++)
        {
            SineWaveSource swc = sines[i];
            swc.phase = 0;
            swc.startTime = AudioSettings.dspTime;
            swc.phaseOffset = i * TWO_PI / sines.Length;

            float duration = swc.envelope[swc.envelope.length - 1].time;
            swc.endTime = swc.startTime + (double)duration * 1.5; // some fudge

            sines[i] = swc;
        }
        _source.Play();
        _toneCompleted = false;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double now = AudioSettings.dspTime;
        double dspTimeIncrement = 1.0 / (double)_sampleRate;
        bool didGenerateAudio = false;

        for (int s = 0, sEnd = sines.Length; s < sEnd; s++)
        {
            SineWaveSource swc = sines[s];
            if (!swc.loop && now >= swc.endTime) continue;

            float increment = frequencyMultiplier * swc.frequency * 2f * Mathf.PI / _sampleRate;

            double age = AudioSettings.dspTime - swc.startTime;

            for (int i = 0, iEnd = data.Length; i < iEnd; i += channels)
            {
                swc.phase = swc.phase + increment;
                if (swc.phase > TWO_PI) { swc.phase = 0; }

                float envelope = swc.envelope.Evaluate((float)age);
                float value = (envelope * swc.gain * Mathf.Sin(swc.phase + swc.phaseOffset));

                data[i] += value;

                if (channels == 2)
                {
                    data[i + 1] += value;
                }

                age += dspTimeIncrement;
            }

            sines[s] = swc;
            didGenerateAudio = true;
        }

        if (!didGenerateAudio)
        {
            Debug.LogFormat("[OnAudioFilterRead] - done generating tones; stopping audio source...");
            _toneCompleted = true;
        }
    }
}
