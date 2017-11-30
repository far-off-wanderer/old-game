using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC
{
    static class TinyIoCDynamicResolver
    {
        public static Type[] FindInterfaces(this Type type, Func<Type, object, bool> filter, object criteria)
        {
            List<Type> results = new List<Type>();
            foreach (Type walk in type.GetInterfaces())
            {
                if (filter(type, criteria))
                    results.Add(walk);
            }

            return results.ToArray();
        }
    }
}
