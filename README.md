# Siren

Modular synthesizer for Grasshopper. Siren is built using [Naudio](https://github.com/naudio/NAudio).

- Create control voltage (CV) from geometry
- Convert audio to geometry, convert geometry to audio.
- voltage controlled sin, sawtooth, square and triangle core oscillators.
- voltage controlled multimode filter
- voltage controlled attenator
- Import and trigger samples
- Preview sounds within grasshopper and export to wav.

![grasshopper example](https://github.com/AlasdairMott/Siren/blob/main/documentation/siren.jpg)

## Running Siren

Make sure to copy NAudio dlls from Siren\bin into the grasshopper libraries folder. [Example](https://github.com/AlasdairMott/Siren/tree/main/Examples) grasshopper files are provided.

## Todo
- Delay effect
- Midi parser
- Wave goo to wrap "Cached wave", not "wave stream"
- Endless stream goo
- voltage offset
- Looping
- Sample trim and stitch
- Audio interface outputs

