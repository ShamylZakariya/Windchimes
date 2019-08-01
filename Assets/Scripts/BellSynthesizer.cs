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

        public float duration;

        //
        //  Private state
        //

        internal float phase;
        internal double startTime;
        internal bool started;
        internal bool completed;
    }

    [Serializable]
    public class BellPrototype
    {
        [SerializeField]
        public Tone[] tones;

        [SerializeField]
        public float frequencyMultiplier = 1f;

        [SerializeField]
        public float envelopeTimeScale = 1f;

        [SerializeField]
        public float gain = 1f;

        public BellPrototype Clone()
        {
            return new BellPrototype()
            {
                tones = tones.Select((i) => i).ToArray(),
                frequencyMultiplier = this.frequencyMultiplier,
                envelopeTimeScale = this.envelopeTimeScale,
                gain = this.gain
            };
        }

        internal bool completed = false;
    }

    [SerializeField] BellPrototype bellPrototype = null;

    //
    //  Private statue
    //

    private List<BellPrototype> _activeBells = new List<BellPrototype>();
    private List<BellPrototype> _queuedBells = new List<BellPrototype>();

    private AudioSource _source;
    private float _sampleRate = 0;
    private float _secondsUntilNextCleanup = 0;
    private bool _generatingTone;
    private object _generatingToneLock = new object();
    private const float CleanupPeriod = 1f;
    private const float TwoPi = Mathf.PI * 2;

    public BellPrototype Prototype { get { return bellPrototype.Clone(); } }

    void Start()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.Stop();
    }

    void Update()
    {
        _secondsUntilNextCleanup -= Time.deltaTime;
        if (_secondsUntilNextCleanup <= 0)
        {
            _secondsUntilNextCleanup += CleanupPeriod;
            _activeBells = _activeBells.Where((i) => !i.completed).ToList();
        }

        if (_queuedBells.Any())
        {
            lock (_generatingToneLock)
            {
                _activeBells.AddRange(_queuedBells);
                _queuedBells.Clear();
            }
        }
    }

    public void Play()
    {
        Play(bellPrototype);
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
            swc.started = false;
            swc.completed = false;
            bp.tones[i] = swc;
        }

        lock (_generatingToneLock)
        {
            if (_generatingTone) { _queuedBells.Add(bp); }
            else { _activeBells.Add(bp); }
        }

        if (!_source.isPlaying)
        {
            _source.Play();
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        lock (_generatingToneLock)
        {
            _generatingTone = true;

            double now = AudioSettings.dspTime;
            foreach (BellPrototype bell in _activeBells)
            {
                if (bell.completed) continue;

                float dspTimeIncrement = 1 / (_sampleRate * bell.envelopeTimeScale);
                int expiredCount = 0;

                for (int b = 0, bEnd = bell.tones.Length; b < bEnd; b++)
                {
                    Tone tone = bell.tones[b];

                    if (!tone.started)
                    {
                        tone.startTime = now;
                        tone.started = true;
                    }

                    float age = (float)(now - tone.startTime) / bell.envelopeTimeScale;
                    float increment = bell.frequencyMultiplier * tone.frequency * TwoPi / _sampleRate;

                    for (int i = 0, iEnd = data.Length; i < iEnd; i += channels)
                    {
                        tone.phase += increment;
                        float envelope = Mathf.Max(1 - (age / tone.duration), 0);
                        envelope *= envelope;
                        age += dspTimeIncrement;

                        float value = (envelope * tone.gain * bell.gain * Mathf.Sin(tone.phase + tone.phaseOffset));
                        data[i] += value;

                        if (channels == 2)
                        {
                            data[i + 1] += value;
                        }
                    }

                    bell.tones[b] = tone;

                    if (age > tone.duration)
                    {
                        expiredCount++;
                    }
                }

                if (expiredCount == bell.tones.Length)
                {
                    bell.completed = true;
                }
            }

            _generatingTone = false;
        }
    }
}
