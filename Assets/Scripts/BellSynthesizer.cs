using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BellSynthesizer : MonoBehaviour
{
    [Serializable]
    public struct Tone
    {
        public float frequency;
        [Range(0, 1)]
        public float gain;
        [Range(0, Mathf.PI * 2)]
        public float phaseOffset;
        public AnimationCurve envelope;

        //
        //  Private state
        //

        internal float phase;
        internal double startTime;
        internal double duration;
        internal bool started;
        internal bool completed;
    }

    [Serializable]
    public class BellPrototype
    {
        public Tone[] tones;

        [SerializeField]
        public float frequencyMultiplier = 1f;

        [SerializeField]
        public float envelopeTimeScale = 1f;

        public BellPrototype Clone()
        {
            return new BellPrototype()
            {
                tones = tones.Select((i) => i).ToArray(),
                frequencyMultiplier = this.frequencyMultiplier,
                envelopeTimeScale = this.envelopeTimeScale
            };
        }

        internal bool completed = false;
    }

    [SerializeField] BellPrototype bellPrototype = null;

    //
    //  Private statue
    //

    private List<BellPrototype> _activeBells = new List<BellPrototype>();

    private AudioSource _source;
    private float _sampleRate = 0;
    private float _secondsUntilNextCleanup = 0;
    private const float CleanupPerdiod = 1f;
    private const float TwoPi = Mathf.PI * 2;


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
            Play(bellPrototype);
        }

        _secondsUntilNextCleanup -= Time.deltaTime;
        if (_secondsUntilNextCleanup <= 0)
        {
            _secondsUntilNextCleanup += CleanupPerdiod;
            int c = _activeBells.Count;
            _activeBells = _activeBells.Where((i) => !i.completed).ToList();
            if (_activeBells.Count < c)
            {
                Debug.LogFormat("Pruned {0} tone bundles", (c - _activeBells.Count));
            }
        }
    }

    public void Play(BellPrototype bp)
    {
        bp = bp.Clone();
        bp.completed = false;
        for (int i = 0; i < bp.tones.Length; i++)
        {
            Tone swc = bp.tones[i];
            swc.phase = 0;
            swc.phaseOffset = i * TwoPi / bp.tones.Length;
            swc.duration = swc.envelope[swc.envelope.length - 1].time - swc.envelope[0].time;
            swc.started = false;
            swc.completed = false;
            bp.tones[i] = swc;
        }

        _activeBells.Add(bp);

        if (!_source.isPlaying)
        {
            _source.Play();
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double now = AudioSettings.dspTime;
        foreach(BellPrototype toneBundle in _activeBells)
        {
            if (toneBundle.completed) continue;

            double dspTimeIncrement = 1 / ((double)_sampleRate * toneBundle.envelopeTimeScale);
            int expiredCount = 0;

            for (int t = 0, tEnd = toneBundle.tones.Length; t < tEnd; t++)
            {
                Tone tone = toneBundle.tones[t];

                if (!tone.started)
                {
                    tone.startTime = now;
                    tone.started = true;
                }

                double age = (now - tone.startTime) / toneBundle.envelopeTimeScale;
                float increment = toneBundle.frequencyMultiplier * tone.frequency * TwoPi / _sampleRate;

                for (int i = 0, iEnd = data.Length; i < iEnd; i += channels)
                {
                    tone.phase += increment;
                    float envelope = Mathf.Max(tone.envelope.Evaluate((float)age), 0);
                    float value = (envelope * tone.gain * Mathf.Sin(tone.phase + tone.phaseOffset));

                    age += dspTimeIncrement;
                    data[i] += value;

                    if (channels == 2)
                    {
                        data[i + 1] += value;
                    }
                }

                toneBundle.tones[t] = tone;

                if (age > tone.duration)
                {
                    expiredCount++;
                }
            }

            if (expiredCount == toneBundle.tones.Length)
            {
                toneBundle.completed = true;
            }
        }

    }
}
