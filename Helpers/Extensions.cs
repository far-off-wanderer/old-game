using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Codename___Far_Off_Wanderer.Helpers
{
    public static class Extensions
    {
        public static void AddFromResource<T>(this Dictionary<string, T> _this, string Item, ContentManager manager, string ResourceName = null)
        {
            _this.Add(Item, manager.Load<T>(ResourceName ?? Item));
        }
    }
}
