using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SceneManager
{
    public sealed class Scene {
        public T[] get<T>() where T : SObject {
            return getIE<T>().ToArray();
        }
        public IEnumerable<T> getIE<T>() where T : SObject {
            return objects.Where(o => o.GetType() == typeof(T)).Cast<T>();
        }


        public List<SObject> objects = new List<SObject>();

        public void Add<T>(T item) where T : SObject {
            objects.Add(item);
        }
        public void AddRange<T>(IEnumerable<T> items) where T : SObject {
            objects.AddRange(items);
        }
    }
}
