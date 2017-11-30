using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Conesoft.Engine.ResourceLoader
{
    public interface IResourceLoader
    {
        T Load<T>(string Resource);
        IEnumerable<KeyValuePair<string, T>> Load<T>(params string[] Resources);
    }

    namespace Implementation
    {
        class ResourceLoader : IResourceLoader
        {
            public ContentManager Manager { get; set; }

            public T Load<T>(string Resource)
            {
                return Manager.Load<T>(Resource);
            }

            public IEnumerable<KeyValuePair<string, T>> Load<T>(params string[] Resources)
            {
                foreach (var resource in Resources)
                {
                    yield return new KeyValuePair<string, T>(resource, Load<T>(resource));
                }
            }
        }
    }
}
