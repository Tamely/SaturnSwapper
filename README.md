# Saturn (V3 - C++)

> **Project Discontinued**
>
> This project was officially discontinued as of February 2025. The code in this repository is provided as-is for archival and educational purposes. The source code, particularly for this V3 branch, is not clean as it was never intended to be open-source. For cleaner, more readable code, please see the [`v2.2.7`](https://github.com/Tamely/SaturnSwapper/tree/2.2.7) C\# branch.

This branch contains the code to build the last official version of Saturn V3. This includes the file provider, plugin scripting language, plugin runtime, plugin web API, and the item generators.

-----

## What was Saturn?

Saturn was an advanced, feature-rich modding tool for games built on Unreal Engine. While it was configured by default for **Fortnite**, its powerful backend could be adapted for other Unreal Engine titles by request through the official Discord.

The user interface was built with modern web technologies (HTML, CSS, JS) and rendered directly on the GPU using [**Ultralight**](https://ultralig.ht/), combining the smooth, responsive feel of a website with the power of a native C++ application.

-----

## Core Features

  * **Advanced File & Asset Handling**:

      * Full **read/write support** for `.pak`, `.utoc`, and `.ucas` files.
      * A custom, high-performance **Unreal Engine file parser** built with C++ modules.
      * A robust **asset serializer** with support for **IoStore** containers and **unversioned properties**, ensuring 1:1 serialization with the target engine version or game fork.
      * Native support for **Oodle**, **GZIP**, and **ZLIB** compression and decompression for all asset operations.

  * **Powerful Scripting & Extensibility**:

      * A custom scripting language built on **Duktape** with a rich callback system, allowing for complex mod creation.
      * An integrated **web server** that enabled two-way communication between the game and the Saturn client, allowing mods to react to in-game events in real time.

  * **User-Friendly Features**:

      * Automated **Item Generators** that read game files to create item "presets," allowing users to easily apply mods without needing to use the scripting system for common changes.
      * A flexible plugin system that enabled the creation of fully custom items, from meshes and textures to unique game logic.

-----

## How can I build this?

1.  Install Git and CMake (links below).
2.  Clone the repository recursively to include all submodules.
    ```cmd
    git clone https://github.com/Tamely/SaturnSwapper --recursive
    ```
3.  Navigate to the cloned directory.
    ```cmd
    cd SaturnSwapper
    ```
4.  Generate the build files using CMake.
    ```cmd
    cmake -B build
    ```
5.  Build the project. For the smallest executable size, use the `MinSizeRel` configuration.
    ```cmd
    cmake --build build --config MinSizeRel
    ```
6.  The final executable will be located in the `/build/MinSizeRel/` directory.

-----

## Reporting Security Issues

Security issues and bugs should be reported privately, via DM on Discord, to any Manager role holder in [Saturn's Discord](https://discord.gg/SaturnSwapper). You should receive a response within 24 hours. If for whatever reason you do not, please DM `@tamely` directly. You may be entitled to financial compensation for your report depending on the severity of the issue.

-----

## Filing Issues

This repository is for issues tied directly to the codebase. If a feature of the project is not working as intended, please use the `Support` channel in [Saturn's Discord](https://discord.gg/SaturnSwapper) to resolve your issue.

-----

## Useful Links

  * [Saturn's Discord](https://discord.gg/SaturnSwapper)
  * [CMake Download](https://cmake.org/download/)
  * [Git Download](https://gitforwindows.org/)

-----

## License

Saturn is licensed under the [GNU General Public License v3.0](https://www.google.com/search?q=LICENSE).
