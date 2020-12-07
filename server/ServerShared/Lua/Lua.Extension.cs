using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KeraLua
{
    public interface ILuable
    { }

    public static class Static
    {
        // 빌트인 함수 규칙
        //   1. Builtin 으로 시작
        //   2. 뒤에 붙는 이름의 lowercase로 등록
        public static readonly string BUILTIN_PREFIX = "Builtin";
        public static Lua Main { get; private set; } = new Lua();

        static Static()
        {
            Main.LoadBuiltinFunctions();
        }

        private static Dictionary<Lua, List<GCHandle>> allocatedGCHandlerDict = new Dictionary<Lua, List<GCHandle>>();

        public static LuaStatus Resume(this Lua lua, int arguments)
        {
            var result = lua.Resume(lua, arguments);
            switch (result)
            {
                case LuaStatus.Yield:
                    break;

                default:
                    if (allocatedGCHandlerDict.TryGetValue(lua, out var allocatedList) == false)
                        break;

                    allocatedList.ForEach(x => x.Free());
                    allocatedGCHandlerDict.Remove(lua);
                    break;
            }

            return result;
        }

        public static T ToLuable<T>(this Lua lua, int offset) where T : class, ILuable
        {
            try
            {
                var ud = Marshal.ReadIntPtr(lua.ToUserData(offset));
                var allocated = GCHandle.FromIntPtr(ud);
                var casted = allocated.Target as T;
                return casted;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static List<string> ToStringList(this Lua lua, int offset)
        {
            var size = lua.RawLen(offset);

            var list = new List<string>();
            for (int i = 0; i < size; i++)
            {
                lua.RawGetInteger(offset, i + 1);
                list.Add(lua.ToString(-1));
            }

            return list;
        }

        public static bool PushLuable<T>(this Lua lua, T luable) where T : ILuable
        {
            try
            {
                var allocated = GCHandle.Alloc(luable, GCHandleType.Weak);
                Marshal.WriteIntPtr(lua.NewUserData(IntPtr.Size), GCHandle.ToIntPtr(allocated));
                lua.GetMetaTable(typeof(T).Name);
                lua.SetMetaTable(-2);

                if (allocatedGCHandlerDict.ContainsKey(lua) == false)
                    allocatedGCHandlerDict.Add(lua, new List<GCHandle>());
                allocatedGCHandlerDict[lua].Add(allocated);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static void LoadBuiltinFunctions(this Lua lua, string assemblyName = null)
        {
            var assembly = string.IsNullOrEmpty(assemblyName) ?
                Assembly.GetEntryAssembly() :
                AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == assemblyName);

            var luableTypes = assembly.GetTypes().Where(x => typeof(ILuable).IsAssignableFrom(x)).ToList();   // 모든 루아 오브젝트
            var sortedLuableTypes = luableTypes.OrderByDescending(x => luableTypes.Count(luableType => luableType.IsSubclassOf(x))).ToList();   // 상속순으로 정렬된 루아 오브젝트
            foreach (var luableType in sortedLuableTypes)
            {
                var builtinMethods = luableType.GetMethods()
                    .Where(x =>
                    {
                        if (x.IsStatic == false)
                            return false;

                        if (x.Name.StartsWith(BUILTIN_PREFIX) == false)
                            return false;

                        if (x.ReturnType != typeof(int))
                            return false;

                        var parameters = x.GetParameters();
                        if (parameters.Length != 1)
                            return false;

                        if (parameters.First().ParameterType != typeof(IntPtr))
                            return false;

                        return true;
                    });

                var registerList = builtinMethods.Select(x =>
                {
                    var builtinFunctionName = x.Name.Replace(BUILTIN_PREFIX, string.Empty).ToLower();
                    var buildinFunctionParameterse = x.GetParameters().Select(x => x.ParameterType).Concat(new[] { x.ReturnType });
                    return new LuaRegister
                    {
                        name = builtinFunctionName,
                        function = x.CreateDelegate(typeof(LuaFunction)) as LuaFunction
                    };
                }).Concat(new[] { new LuaRegister { name = null, function = null } }).ToArray();


                lua.NewMetaTable(luableType.Name);
                if (luableType.BaseType != typeof(ILuable)) // 상속
                {
                    lua.GetMetaTable(luableType.BaseType.Name);
                    lua.SetMetaTable(-2);
                }
                lua.PushCopy(-1);
                lua.SetField(-2, "__index");
                lua.SetFuncs(registerList, 0);
            }
        }
    }
}
