[![Build status](https://ci.appveyor.com/api/projects/status/jkary3fagowm4adm/branch/master?svg=true)](https://ci.appveyor.com/project/TomRichter/pulsar-plugin-loader/branch/master)

# [PULSAR Plugin Loader](https://github.com/TomRichter/pulsar-plugin-loader)

Injects a basic plugin loader into [*PULSAR: Lost Colony*](http://www.pulsarthegame.com/).

## Usage

```
.\PulsarPluginBootstrapper.exe [Path\To\Pulsar\PULSAR_LostColony_Data\Managed\Assembly-CSharp.dll]
```

By default, PPL will attempt to patch the Steam version of the game in Steam's default install location.  To patch a PULSAR installation in another location (non-Steam version, copy of client, etc), simply specify the path to `Assembly-CSharp.dll` as shown above.

Afterwards, add plugins to then `PULSAR_LostColony_Data\Managed\Plugins` directory, then run PULSAR normally.  `PulsarPluginLoader.exe` is no longer necessary.

### Removal

Use Steam's `Verify Integrity of Game Files` option to restore any modified files with minimal download.

Non-Steam users can attempt to rename `Assembly-CSharp.dll.bak` to `Assembly-CSharp.dll`, assuming no official patches were released since it was last generated.  Otherwise, restore a clean copy from the official non-Steam download.

Optionally remove `PulsarPluginLoader.dll`, `Assembly-CSharp.dll.bak`, and the `Plugins` directory from `PULSARLostColony\PULSAR_LostColony_Data\Managed`

## Creating Plugins

All plugins must be C# Class Libraries targeting .NET Framework 3.5 (to match Unity Engine).  See [this screenshot](https://i.imgur.com/X7bDnYr.png) for an example of project creation settings in Visual Studio Community 2017 ([free download](https://visualstudio.microsoft.com/vs/community/)).

Additionally, reference the following assemblies in your plugin project:

 * `PulsarPluginLoader.dll` (PPL code)
 * `Assembly-CSharp.dll` (Game code)
 * `UnityEngine.CoreModule.dll` (Engine code)
 * `UnityEngine.*.dll` (optional; specific DLL depends what changes are made)

Plugins must also contain a subclass of `[PulsarPluginLoader.PulsarPlugin]` for plugin initialization.  This class is instantiated once during game startup (currently before `PLGlobal.Awake()`), so do plugin setup its constructor.  The base constructor already loads Harmony, so only overriding `protected string HarmonyIdentifier()` may be enough for simple plugins.  To extend setup, override the constructor with a call to base:

```csharp
class MyPlugin : PulsarPlugin
{
    public MyPlugin() : base()
    {
        // Do additional setup here
    }
    
    [...]
}
```

Using [`Harmony`](https://github.com/pardeike/Harmony) to hook PULSAR methods is strongly recommended due to its simple API and tools that help multiple plugins play nicely when modifying the same methods (stacking hooks instead of overwriting each other, prioritization, etc).  Any class using Harmony decorators will magically hook their methods.

### Basic Plugin Example

```csharp
using PulsarPluginLoader;

namespace ExamplePlugin
{
    class MyPlugin : PulsarPlugin
    {
        protected override string HarmonyIdentifier()
        {
            // Make this unique to your plugin!
            return "com.example.pulsar.plugins";
        }
    }
}
```

```csharp
using Harmony;

namespace ExamplePlugin
{
	[HarmonyPatch(typeof(PLNetworkManager))]
	[HarmonyPatch("Start")]
	class VersionModifier
	{
		static void Postfix(PLNetworkManager __instance)
		{
			__instance.VersionString += "\nCool Kid Version";
		}
	}
}
```

Distribute plugins as `.dll` assemblies.  To install, simply drop the assembly into the `Plugins` folder; any properly-defined `*.dll` plugin assemblies are automatically loaded.