# Saturn (V2.2.7 - C\#)

> **Project Discontinued**
>
> This version of the project was officially discontinued as of December 8, 2023. The code is provided as-is for archival and educational purposes. This branch contains the more stable and cleaner C\# implementation of Saturn.

This repository contains the source code for Saturn v2.2.7, a powerful and feature-rich modding tool built with C\# and Blazor Hybrid.

-----

## What was Saturn?

Saturn was an advanced, user-friendly modding tool designed for Unreal Engine games, with a primary focus on Fortnite. It provided a seamless experience by automating complex processes like file detection and decryption. Its modern UI was built using Blazor Hybrid, allowing for a web-based interface within a native application shell.

While Saturn shipped with a default configuration for Fortnite, its architecture was flexible. Pre-configured builds for other Unreal Engine games were available by request through the official Discord community.

-----

## Core Features

  * **Extensible and Safe Plugin System**:

      * Features a **custom programming language, compiler, and runtime** ([Radon](https://github.com/Tamely/SaturnSwapper/tree/2.2.7/Lang)) that allowed users to create and share their own powerful plugins.
      * The scripting runtime included security **guardrails** to prevent plugins from performing malicious actions like file writing or network requests.
      * A **Plugin Marketplace** allowed users to share and download community-created plugins that were pre-verified for safety.

  * **Advanced Unreal Engine Asset Handling**:

      * Utilized a fork of **[CUE4Parse](https://github.com/Tamely/SaturnSwapper/tree/2.2.7/CUE4Parse)** for parsing Unreal Engine's main container files.
      * Employed a custom-built **[UAssetAPI](https://github.com/Tamely/SaturnSwapper/tree/2.2.7/UAssetAPI)** library for serializing `.uasset` files. This supported both versioned and IoStore unversioned assets, using mappings to correctly deserialize, edit, and serialize data back to the file.
      * Full support for **Oodle** compression and decompression of assets.

  * **Fortnite-Specific Automation**:

      * Automatically fetched both **static and dynamic AES keys** for Fortnite.
      * Automatically detected the Fortnite installation path on disk.
      * Used a custom **[Epic Manifest Parser](https://github.com/Tamely/SaturnSwapper/tree/2.2.7/EpicManifestParser)** to verify game files in-house, ensuring that mods were applied to the correct version.

  * **User Experience & Integrations**:

      * **Automatic Preset Generation**: Scanned game files to automatically create item presets, making common mods accessible without scripting.
      * **Discord Integration**: Included Discord RPC for rich presence and optional Discord OAuth for user login to access certain features.
      * **Localization**: Supported translations into the 10 most-used languages within the community.
      * A clean and efficient logger for tracking events and file operations.

-----

## How can I contribute?

We welcome all kinds of contributions\! Many people, listed or not, have bettered this project, and without them, it wouldn't be where it is today.

  * [Contributing](https://www.google.com/search?q=docs/contributing/GetStarted.md) explains what kind of contributions we welcome.
  * [Workflow Instructions](https://www.google.com/search?q=docs/contributing/BuildingAndRunning.md) explains how to build and run the project.
  * [Fortnite's File Documentation](https://www.google.com/search?q=docs/contributing/FortniteFiles.md) gives a quick overview of where everything is located in Fortnite's files and how Saturn takes advantage of it.

-----

## Reporting security issues and security bugs

Security issues and bugs should be reported privately, via DM on Discord, to any Manager/Contributor role holder in [Saturn's Discord](https://discord.gg/SaturnSwapper). You should receive a response within 24 hours. If - for whatever reason - you do not, please DM @tamely directly. You may be entitled to financial compensation for your report depending on the severity of the issue.

## Filing issues

This repo should contain issues that are tied to the actual codebase of the project. If a feature of the project is not working as intended, please use the `Support` channel in [Saturn's Discord](https://discord.gg/SaturnSwapper) to resolve your issue.

-----

## Useful Links

  * [Saturn's Discord](https://discord.gg/SaturnSwapper)
  * [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/#download-section) - Required to run the Blazor Hybrid UI. Please install the x64 Evergreen Bootstrapper if you are not on Windows 11.

-----

## Unlisted Contributors

| Contributor                                                                                 | Contribution                                         |
| ------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| [Amr Satrio](https://github.com/Amrsatrio)                                                  | Helped with asset obfuscation and just general questions |
| [cyclonefreeze](https://github.com/cyclonefreeze)                                           | Developed base which Saturn built upon               |
| [jakedothow](https://github.com/jakedothow)                                                 | Developed base which Saturn built upon               |
| [All CUE4Parse contributors](https://github.com/FabianFG/CUE4Parse/graphs/contributors)     | CUE4Parse is a main part of the swapper              |

-----

## License

Saturn is licensed under the [GNU General Public License v3.0](https://www.google.com/search?q=LICENSE).
