using FlatBuffers.Protocol.Request;
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
            Character.Direction = direction;
            Character.BeginMoveTime = DateTime.Now;
            switch (direction)
            {
                case Direction.Left:
                    Character.Velocity = new System.Numerics.Vector2(-10.0f, 0);
                    break;

                case Direction.Right:
                    Character.Velocity = new System.Numerics.Vector2(10.0f, 0);
                    break;

                default:
                    return;
            }
            Send(Move.Bytes(new Vector2.Model(Character.Position.X, Character.Position.Y), (int)Character.Direction));
        }

        [CommandEvent("stop")]
        public void OnStop()
        {
            if (Character.BeginMoveTime == null)
                return;

            var elapsed = (DateTime.Now - Character.BeginMoveTime.Value).Ticks;
            var diff = elapsed / 1000000 * Character.Velocity.X;
            Character.Position = new System.Numerics.Vector2(Character.Position.X + diff, 0);
            Character.Velocity = new System.Numerics.Vector2(0, 0);
            Send(Stop.Bytes(new Vector2.Model(Character.Position.X, Character.Position.Y)));
        }

        [CommandEvent("warp")]
        public void OnWarp()
        {
            Send(Warp.Bytes(new Vector2.Model(Character.Position.X, Character.Position.Y)));
        }

        [CommandEvent("cheat/position")]
        public void Cheat_OnPosition()
        { 
            Send(CheatPosition.Bytes(new Vector2.Model(Character.Position.X, Character.Position.Y)));
        }

        [CommandEvent("click")]
        public void OnClick(int sequence)
        {
            Send(Click.Bytes(sequence));
        }

        [CommandEvent("dialog/next")]
        public void OnDialogNext()
        {
            Send(DialogResult.Bytes(true));
        }

        [CommandEvent("dialog/quit")]
        public void OnDialogQuit()
        {
            Send(DialogResult.Bytes(false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">one based</param>
        [CommandEvent("dialog/select")]
        public void OnDialogSelect(int index)
        {
            Send(DialogIndexResult.Bytes(index));
        }

        [CommandEvent("kill")]
        public void OnKill(int sequence)
        {
            Send(CheatKill.Bytes(sequence));
        }

        [CommandEvent("item/active")]
        public void OnActiveItem(ulong id)
        {
            Send(ActiveItem.Bytes(id));
        }

        [CommandEvent("item/inactive")]
        public void OnInactiveItem(ulong id)
        {
            Send(InactiveItem.Bytes(id));
        }
    }
}
