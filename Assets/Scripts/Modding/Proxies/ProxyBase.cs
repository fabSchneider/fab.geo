using MoonSharp.Interpreter;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class LuaHelpInfoAttribute : Attribute
    {
        readonly string info;
        public string Info => info;

        public LuaHelpInfoAttribute(string info)
        {
            this.info = info;
        }
    }

    public abstract class ProxyBase
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public virtual string GetFullDescription()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Description);
            sb.AppendLine("\n<b>Methods:</b>\n");
            
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(m => Attribute.IsDefined(m, typeof(LuaHelpInfoAttribute)));
            
            foreach (MethodInfo m in methods)
            {
                sb.AppendLine(string.Format("<b>{0}</b> ( {1} )",
                    m.Name,
                    string.Join(" , ", m.GetParameters().Select(p => p.Name))).PadRight(32, ' '));

                LuaHelpInfoAttribute helpInfo = m.GetCustomAttribute<LuaHelpInfoAttribute>();
                sb.AppendLine($"  <i>{helpInfo.Info}</i>\n");
            }

            var properies = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => Attribute.IsDefined(p, typeof(LuaHelpInfoAttribute)));

            sb.AppendLine("\n<b>Properties:</b>\n");

            foreach (PropertyInfo p in properies)
            {
                sb.AppendLine($"<b>{p.Name}</b>");

                LuaHelpInfoAttribute helpInfo = p.GetCustomAttribute<LuaHelpInfoAttribute>();
                sb.AppendLine($"  <i>{helpInfo.Info}</i>\n");
            }

            return sb.ToString();
        }
    }

    [MoonSharpUserData]
    public abstract class ProxyBase<T> : ProxyBase
    {

        private T value;

        [MoonSharpHidden]
        public T Value => value;

        [MoonSharpHidden]
        public ProxyBase(T value)
        {
            this.value = value;
        }

        public bool IsNull()
        {
            if (value is UnityEngine.Object obj)
                return !obj;

            return value == null;
        }
    }
}
