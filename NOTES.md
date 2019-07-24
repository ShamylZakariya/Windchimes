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

## Phase 3
- Use Job System to run these rays in parallel