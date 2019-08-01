# WindChimes

## Implementation Details
- While I've written some raycasting ops that should run safely in Unity's Job System (e.g., self-contained and threadsafe), looks like the Job System supports raycasts, so for now we're using built-in Physics.Raycast with a layermask for matching only the wind chime layer.

## Phase 1
### Done
- Use simple raycasting to walk spread-out parallel rays in short hops, bearing some small mass, to the wind chimes. 
- Let the rays bounce and hit other chimes.
- Observe collisions between chimes.

## Phase 2
- Generate appropriate loopable bell tones for each chime based on its radius/length/mass and other factors.
- Trigger bell chime at runtime with intensity, duration, and an appropriate envelope based on the impact kinetics.
    - do we need to generate different tones based on the location of the impact on the bell? Or is the bell tone independent of impact site?

### On Bell Tone Synthesis
https://www.soundonsound.com/techniques/synthesizing-bells
- Figure 8 is particularly informative.
- Sine waves are fine.
- We need a periodic warble (is this gain or frequency shifting?), to create the boing-oing-oing-oing quality
    - "This means that bells produce two partials of almost (but not quite) identical frequencies, and these interfere (or 'beat') in the same way as do two synth oscillators of similar frequency."

#### What needs to be built:
At the very minimum, we need to be able to compose pairs of sine waves and envelopes. We need to be able to say, here's a sine wave at x freq, with an envelope of such and such. and then fire off a stack of these to be rendered in real time.
    - so far this works but needs a little more:
        - a phase offset per-channel

#### Neat Ideas
Take a short clip of the kind of tubular bell sound I want, run it through some kind of FFT analysis, and determine which frequencies are making it up. What software would I need?


## Phase 3
- Use Job System to run these rays in parallel

---

## Known Bugs

- My chime rigid body joints don't seem to constrain rotation