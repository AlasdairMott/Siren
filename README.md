<div align="center">
<img width=200 src="https://github.com/AlasdairMott/Siren/blob/develop/documentation/logo.svg">

# Siren

[![Build Action](https://github.com/AlasdairMott/Siren/workflows/Build%20Grasshopper%20Plugin/badge.svg)](https://github.com/AlasdairMott/Siren/actions/workflows/grasshopper-build.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

</div>

Modular synthesizer for Grasshopper. Siren is built using [Naudio](https://github.com/naudio/NAudio).

- Create control voltage (CV) from geometry
- Convert audio to geometry, convert geometry to audio.
- voltage controlled sin, sawtooth, square and triangle core oscillators.
- voltage controlled multimode filter
- voltage controlled attenator
- Import and trigger samples
- Preview sounds within grasshopper and export to wav.

![grasshopper example](https://github.com/AlasdairMott/Siren/blob/develop/documentation/siren.jpg)

## Running Siren on PC

Make sure to copy NAudio dlls from Siren\bin into the grasshopper libraries folder. [Example](https://github.com/AlasdairMott/Siren/tree/develop/examples) grasshopper files are provided.

## Running Siren on Mac

Currently Siren does not run on Grasshopper for Mac.

## Contributing

Feedback and pull requests welcome; see [`CONTRIBUTING.md` file](https://github.com/AlasdairMott/Siren/blob/develop/.github/CONTRIBUTING.md).

## Plugin Development

To develop the plugin you will need a copy of Rhinoceros installed, and some knowledge working with [C# code](https://docs.microsoft.com/en-us/dotnet/csharp/), the [Rhinoceros/Grasshopper APIs](http://developer.rhino3d.com), and the [Naudio  API](https://github.com/naudio/NAudio).

Editing and compiling that code is best done in Visual Studio. The [community editions](https://www.visualstudio.com) for Windows or macOS should both work. Upon first build it should fetch the required RhinoCommon, Grasshopper, and third-party references from NuGet (an internet connection is required).

Once you have compiled the project you will need to add the `bin` folder to the folders that Grasshopper looks for components in. To do so use the `GrasshopperDeveloperSettings` command in Rhinoceros.

## References

Project structure and readmes were based on Philip Belesky's  [Groundhog Project](https://github.com/philipbelesky/groundhog#readme). NAudio sample provider's are based on Mark Heath's [NAudio courses](https://www.pluralsight.com/courses/audio-programming-naudio) and blog posts.

## License

This project is licensed under the GPL v3 License - see the [`LICENSE` file](https://github.com/AlasdairMott/Siren/blob/develop/.github/LICENSE) for details.
