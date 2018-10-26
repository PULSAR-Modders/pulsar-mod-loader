# PULSAR Plugin Loader

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

Additionally, reference the following assemblies:

 * `PulsarPluginLoader.dll` (PPL code)
 * `Assembly-CSharp.dll` (Game code)
 * `UnityEngine.CoreModule.dll` (optional; depends if using engine calls)

Plugins must also contain a class with a public static method decorated using `[PulsarPluginLoader.PluginEntryPoint]`.  Naming of class or method does not matter.  This method will be run when the plugin loads (currently before `PLGlobal.Awake()`), so do plugin setup here.

Using [`Harmony`](https://github.com/pardeike/Harmony) to hook PULSAR methods is strongly recommended due to its simple API and tools that help multiple plugins play nicely when modifying the same methods (stacking hooks instead of overwriting each other, prioritization, etc).  Any class using Harmony decorators will magically hook their methods, so simply calling `HarmonyInstance.PatchAll()` in the plugin's entry point is sufficient.

Basic example:

```csharp
using Harmony;
using System.Reflection;

namespace ExamplePlugin
{
    class Plugin
    {
        [PulsarPluginLoader.PluginEntryPoint]
        public static void StartPlugin()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.example.pulsar.plugins");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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
			__instance.VersionString += " (PPL)";
		}
	}
}
```

Distribute plugins as `.dll` assemblies.  To install, simply drop the assembly into the `Plugins` folder; any `.dll` with a `[PulsarPluginLoader.PluginEntryPoint]`-decorated method will be automatically loaded.