using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calibratie {
    public sealed class Scene {
        public T[] get<T>() where T : SPoint {
            return getIE<T>().ToArray();
        }
        public IEnumerable<T> getIE<T>() where T : SPoint {
            return objects.Where(o => o.GetType() == typeof(T)).Cast<T>();
        }
        
        [Obsolete("wordt private")]
        public List<SPoint> objects = new List<SPoint>();

        public void Add<T>(T item) where T : SPoint {
            objects.Add(item);
        }
        public void AddRange<T>(IEnumerable<T> items) where T : SPoint {
            objects.AddRange(items);
        }
    }
}
