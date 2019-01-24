using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static PulsarPluginLoader.Events.Event;

namespace PulsarPluginLoader.Events
{
    class EventHelper
    {
        public static Dictionary<Type, List<MethodInfo>> EventHandlers = new Dictionary<Type, List<MethodInfo>>();

        //Called for every plugin loaded
        public static void RegisterEventHandlers(string name, PulsarPlugin plugin)
        {
            Assembly asm = plugin.GetType().Assembly;
            foreach (Type classType in asm.GetTypes())
            {
                foreach (MethodInfo method in classType.GetMethods())
                {
                    ParameterInfo[] param = method.GetParameters();
                    if (method.GetCustomAttributes(typeof(PPLEventHandler), false).Any() && method.ReturnType == typeof(void) && param.Count() == 1 && param[0].ParameterType.IsSubclassOf(typeof(Event)))
                    {
                        if (EventHandlers.TryGetValue(param[0].ParameterType, out List<MethodInfo> methods))
                        {
                            methods.Add(method);
                        }
                        else
                        {
                            methods = new List<MethodInfo> { method };
                            EventHandlers.Add(param[0].ParameterType, methods);
                        }
                    }
                }
            }
        }

        //Everything beyond this point is called from Cecil modified code.

        public static void OnPlayerJoin(PLPlayer player)
        {
            if (EventHandlers.TryGetValue(typeof(PlayerJoinEvent), out List<MethodInfo> methods))
            {
                object[] o = new object[] { new PlayerJoinEvent(player) };
                foreach (MethodInfo method in methods)
                {
                    method.Invoke(null, o);
                }
            }
        }
    }
}
