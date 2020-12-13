using NetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TestClient
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandEventAttribute : Attribute
    {
        public string Command { get; private set; }

        public CommandEventAttribute(string cmd)
        {
            Command = cmd;
        }
    }

    public partial class GameHandler
    {
        private Dictionary<string, MethodInfo> _commandMethodDict = new Dictionary<string, MethodInfo>();

        public GameHandler()
        {
            foreach (var method in GetType().GetMethods())
            {
                var attr = method.GetCustomAttribute<CommandEventAttribute>();
                if (attr == null)
                    continue;

                if (method.ReturnType != typeof(void))
                    continue;

                _commandMethodDict.Add(attr.Command, method);
            }
        }

        public bool Command(string name, params object[] parameters)
        {
            if (_commandMethodDict.ContainsKey(name) == false)
                return false;

            var method = _commandMethodDict[name];
            var methodParameters = method.GetParameters();
            var convertedList = parameters.Select((x, i) => 
            {
                if (methodParameters[i].ParameterType.IsEnum)
                    return Enum.Parse(methodParameters[i].ParameterType, parameters[i] as string);
                else
                    return Convert.ChangeType(parameters[i], methodParameters[i].ParameterType);
            }).ToArray();

            _commandMethodDict[name].Invoke(this, convertedList);
            return true;
        }

        [CommandEvent("move")]
        public void OnMove(Direction direction)
        {
            Console.WriteLine(direction);
        }

        [CommandEvent("stop")]
        public void OnStop()
        {
            Console.WriteLine("stop");
        }
    }
}
