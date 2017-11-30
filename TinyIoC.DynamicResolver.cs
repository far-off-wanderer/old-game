using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyIoC
{
    static class TinyIoC
    {
        public static T ResolveDynamically<T>(this TinyIoCContainer container, string name)
        {
            return (T)container.Resolve(Type.GetType(name));
        }
    }
}
