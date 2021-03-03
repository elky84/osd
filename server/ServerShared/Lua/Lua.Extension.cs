using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace KeraLua
{
    public interface ILuable
    { }

    public static class Static
    {
        private static readonly int MAX_THREAD_POOL_SIZE = 10000;

        private static readonly Dictionary<Type, LuaRegister[]> _builtinFunctions = new Dictionary<Type, LuaRegister[]>();
        private static readonly Dictionary<Type, Dictionary<string, LuaFunction>> _builtinGlobalFunctions = new Dictionary<Type, Dictionary<string, LuaFunction>>();
        private static volatile Queue<Lua> _luaThreadPool = new Queue<Lua>();

        // 빌트인 함수 규칙
        //   1. Builtin 으로 시작
        //   2. 뒤에 붙는 이름의 snake case로 등록 (대문자 시작기준)
        public static readonly string BUILTIN_PREFIX = "Builtin";
        public static Lua Main { get; private set; } = new Lua { Encoding = Encoding.UTF8 };

        static Static()
        {
            LoadBuiltinFunctions();

            for (int i = 0; i < MAX_THREAD_POOL_SIZE; i++)
            {
                var lua = NewLuaState();
                _luaThreadPool.Enqueue(lua);
            }
        }

        private static Lua NewLuaState()
        {
            var lua = new Lua
            {
                Encoding = Encoding.UTF8
            };

            foreach (var (luableType, funcSet) in _builtinFunctions)
            {
                lua.NewMetaTable(luableType.Name);
                if (luableType.BaseType != typeof(ILuable)) // 상속
                {
                    lua.GetMetaTable(luableType.BaseType.Name);
                    lua.SetMetaTable(-2);
                }
                lua.PushCopy(-1);
                lua.SetField(-2, "__index");
                lua.SetFuncs(funcSet, 0);
            }

            foreach (var (type, funcSet) in _builtinGlobalFunctions)
            {
                foreach(var (name, func) in funcSet)
                    lua.Register(name, func);
            }
            
            return lua;
        }

        public static Lua Get()
        {
            if (_luaThreadPool.Count == 0)
            {
                var created = NewLuaState();
                return created;
            }

            return _luaThreadPool.Dequeue();
        }

        public static void Release(this Lua lua)
        {
            if (_luaThreadPool.Contains(lua) == false)
                _luaThreadPool.Enqueue(lua);
        }

        public static LuaStatus Resume(this Lua lua, int arguments)
        {
            var result = lua.Resume(lua, arguments);
            switch (result)
            {
                case LuaStatus.Yield:
                    break;

                default:
                    //lua.GarbageCollector(LuaGC.Collect, 0);
                    lua.Release();
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

        public static int BuiltinGC(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var ud = Marshal.ReadIntPtr(lua.ToUserData(1));
            var allocated = GCHandle.FromIntPtr(ud);
            allocated.Free();
            return 0;
        }

        public static T PushLuable<T>(this Lua lua, T luable) where T : class, ILuable
        {
            return PushLuable(lua, luable, typeof(T)) as T;
        }

        public static ILuable PushLuable(this Lua lua, ILuable luable, Type type)
        {
            var allocated = GCHandle.Alloc(luable, GCHandleType.Normal);
            var data = lua.NewUserData(IntPtr.Size);
            Marshal.WriteIntPtr(data, GCHandle.ToIntPtr(allocated));
            lua.GetMetaTable(type.Name);
            lua.PushCFunction(BuiltinGC);
            lua.SetField(-2, "__gc");
            lua.SetMetaTable(-2);
            return luable;
        }

        private static void LoadBuiltinFunctions(string assemblyName = null)
        {
            var assembly = string.IsNullOrEmpty(assemblyName) ?
                Assembly.GetEntryAssembly() :
                AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == assemblyName);

            var luableTypes = assembly.GetTypes().Where(x => typeof(ILuable).IsAssignableFrom(x)).ToList();   // 모든 루아 오브젝트
            var sortedLuableTypes = luableTypes.OrderByDescending(x => luableTypes.Count(luableType => luableType.IsSubclassOf(x))).ToList();   // 상속순으로 정렬된 루아 오브젝트
            foreach (var luableType in sortedLuableTypes)
            {
                // 레퍼런스가 없으면 가비지컬렉터에 의해서 해제되어 빌트인 함수가 호출되는 시점에서 프로세스 크래시
                // https://blog.msalt.net/298
                _builtinFunctions[luableType] = luableType
                    .BuiltinFunctions()
                    .Select(pair => new LuaRegister { name = pair.Key, function = pair.Value })
                    .Concat(new[] { new LuaRegister { name = null, function = null } }).ToArray();
            }
        }

        public static Dictionary<string, LuaFunction> BuiltinFunctions(this Type type)
        { 
            return type
                .GetMethods()
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
                })
                .ToDictionary(x => string.Join('_', Regex.Split(x.Name.Replace(BUILTIN_PREFIX, string.Empty), @"(?<!^)(?=[A-Z])").Select(x => x.ToLower())), 
                              x => x.CreateDelegate(typeof(LuaFunction)) as LuaFunction);
        }

        public static Dictionary<string, LuaFunction> BindGlobalBuiltinFunctions<T>()
        {
            var type = typeof(T);
            var builtinFunctions = type.BuiltinFunctions();
            foreach (var (name, func) in builtinFunctions)
            {
                foreach(var thread in _luaThreadPool)
                    thread.Register(name, func);
            }

            _builtinGlobalFunctions[type] = builtinFunctions;
            return builtinFunctions;
        }
    }
}
