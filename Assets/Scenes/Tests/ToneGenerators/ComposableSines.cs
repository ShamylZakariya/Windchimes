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
        internal double duration;
        internal bool started;
    }

    [SerializeField]
    SineWaveSource[] sines = null;

    [SerializeField]
    float frequencyMultiplier = 1f;

    [SerializeField]
    float envelopeTimeScale = 1f;

    //
    //  Private statue
    //

    private AudioSource _source;
    private float _sampleRate = 0;
    const float TWO_PI = Mathf.PI * 2;

    void Start()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.Stop();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayTone();
        }
    }

    void PlayTone()
    {
        for (int i = 0; i < sines.Length; i++)
        {
            SineWaveSource swc = sines[i];
            swc.phase = 0;
            swc.phaseOffset = i * TWO_PI / sines.Length;
            swc.duration = swc.envelope[swc.envelope.length - 1].time - swc.envelope[0].time;
            swc.started = false;
            sines[i] = swc;
        }

        if (!_source.isPlaying)
        {
            _source.Play();
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double now = AudioSettings.dspTime;
        double dspTimeIncrement = 1 / ((double)_sampleRate * envelopeTimeScale);
        int expiredCount = 0;

        for (int s = 0, sEnd = sines.Length; s < sEnd; s++)
        {
            SineWaveSource swc = sines[s];

            if (!swc.started)
            {
                swc.startTime = now;
                swc.started = true;
            }

            double age = (now - swc.startTime) / envelopeTimeScale;
            float increment = frequencyMultiplier * swc.frequency * TWO_PI / _sampleRate;

            for (int i = 0, iEnd = data.Length; i < iEnd; i += channels)
            {
                swc.phase += increment;
                float envelope = Mathf.Max(swc.envelope.Evaluate((float)age), 0);
                float value = (envelope * swc.gain * Mathf.Sin(swc.phase + swc.phaseOffset));

                age += dspTimeIncrement;
                data[i] += value;

                if (channels == 2)
                {
                    data[i + 1] += value;
                }
            }

            sines[s] = swc;

            if (!swc.loop && age > swc.duration)
            {
                expiredCount++;
            }
        }
    }
}
