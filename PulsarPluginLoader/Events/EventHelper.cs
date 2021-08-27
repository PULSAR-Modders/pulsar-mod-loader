using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                    if (method.GetCustomAttributes(typeof(EventHandler), false).Any() && method.ReturnType == typeof(void) &&
                        param.Count() == 1 && (param[0].ParameterType.IsSubclassOf(typeof(Event)) || param[0].ParameterType == typeof(Event)))
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

        public static void PostEvent(Type eventType, object[] eventObject)
        {
            //post events for this class and all base classes of this class
            while (eventType != typeof(object))
            {
                if (EventHandlers.TryGetValue(eventType, out List<MethodInfo> methods))
                {
                    foreach (MethodInfo method in methods)
                    {
                        method.Invoke(null, eventObject);
                    }
                }
                eventType = eventType.BaseType;
            }
        }

        //Everything beyond this point is called from Cecil modified code.

        public static void OnPlayerAdded(PLPlayer player)
        {
            PostEvent(typeof(PlayerAddedEvent), new object[] { new PlayerAddedEvent(player) });
        }

        public static void OnPlayerRemoved(PLPlayer player)
        {
            PostEvent(typeof(PlayerRemovedEvent), new object[] { new PlayerRemovedEvent(player) });
        }
    }
}
