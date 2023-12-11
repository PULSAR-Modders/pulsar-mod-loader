# [PULSAR Mod Loader][0]


[0]: https://github.com/PULSAR-Modders/pulsar-mod-loader "PULSAR Mod Loader"

[![Build Status][1]][2]
[![Download][3]][4]
[![Wiki][5]][6]
[![Discord][7]][8]

[1]: https://github.com/PULSAR-Modders/pulsar-mod-loader/workflows/Build/badge.svg
[2]: https://github.com/PULSAR-Modders/pulsar-mod-loader/actions "Build Status"
[3]: https://img.shields.io/badge/-DOWNLOAD-success
[4]: https://github.com/PULSAR-Modders/pulsar-mod-loader/packages "Download"
[5]: https://img.shields.io/badge/-WIKI-informational
[6]: https://github.com/PULSAR-Modders/pulsar-mod-loader/wiki "Wiki"
[7]: https://img.shields.io/discord/458244416562397184.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2
[8]: https://discord.gg/yBJGv4T "PML Discord"

Injects a basic mod loader into [*PULSAR: Lost Colony*][10].

[10]: http://www.pulsarthegame.com/ "PULSAR: Lost Colony"

## Installation

Before running ensure your Pulsar installation is a Mono build. On Steam you can change this by right clicking Pulsar: lost colony in your steam library and selecting [properties > betas > mono].

![image](https://github.com/PULSAR-Modders/pulsar-mod-loader/assets/46509577/8aeca171-3cd7-4ffc-8805-77c8ce1400e7)



On Excecution PulsarModLoaderInstaller.exe will attempt to patch the Steam version of the game in after detecting Steam from it's default install location.  To patch a PULSAR installation in another location (non-Steam version, copy of client, etc), simply specify the path to `Assembly-CSharp.dll` as shown below.

```
.\PulsarModLoaderInstaller.exe [Path\To\Pulsar\PULSAR_LostColony_Data\Managed\Assembly-CSharp.dll]
```

Additionally, if the steam installation is not selected/viable an OpenFileDialogue will pop up for windows users asking for the installation location.

Afterwards, add mods to the `PULSARLostColony\Mods` directory and run PULSAR normally.  `PulsarModLoaderInstaller.exe` is not needed after this.

### Removal

Use Steam's `Verify Integrity of Game Files` option to restore any modified files with minimal download.

Non-Steam users can attempt to rename `Assembly-CSharp.dll.bak` to `Assembly-CSharp.dll`, assuming no official patches were released since it was last generated.  Otherwise, restore a clean copy from the official non-Steam download.

Optionally remove `PulsarModLoader.dll` and `Assembly-CSharp.dll.bak` from `PULSARLostColony\PULSAR_LostColony_Data\Managed`, and the `Mods` directory from `PULSARLostColony`

## Creating Mods

[Check out the wiki for basic instructions on creating mods.](https://github.com/PULSAR-Modders/pulsar-mod-loader/wiki/Creating-Mods)
