using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComponentOwl.BetterListView;

namespace CalibratieForms {
    /// <summary>
    /// ONLY USE ADD,ADDRANGE,REMOVE,CLEAR
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LVList<T> : List<T> where T : INotifyPropertyChanged {
        private BetterListView _parentLV;

        public BetterListView ParentLV {
            get { return _parentLV; }
            set { _parentLV = value;
                value.DrawItem += onDraw;
            }
        }
        
        public RefAction CollumnDisplay2;
        public delegate void RefAction(T obj, BetterListViewItem item);

        private BetterListViewItemCollection betterListViewItems { get { return ParentLV.Items; } }

        private readonly List<bool> updateList = new List<bool>();

        private void onDraw(object lvSender,BetterListViewDrawItemEventArgs eventArgs) {
            BetterListView lview = (BetterListView) lvSender;
            var item = eventArgs.Item;
            T tag = (T) item.Tag;
            var index = base.IndexOf(tag);
            if (updateList[index] == false) {
                updateList[index] = true;
                CollumnDisplay2(tag, item);
            }
        }

        public new void Add(T item) {
            base.Add(item);
            item.PropertyChanged += item_PropertyChanged;
            updateList.Add(false);
            betterListViewItems.Add(new BetterListViewItem(){Tag = item});
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var index = base.IndexOf((T)sender);
            updateList[index] = false;
            _parentLV.InvokeIfRequired(_parentLV.RedrawItems);
        }
        public new void AddRange(IEnumerable<T> items) {
            foreach (var item in items) {
                Add(item);
            }
        }
        public new void Remove(T item) {
            var index = base.IndexOf(item);
            item.PropertyChanged -= item_PropertyChanged;
            RemoveAt(index);
            updateList.RemoveAt(index);
            betterListViewItems.RemoveAt(index);
        }
        public new void Clear() {
            base.Clear();
            updateList.Clear();
            betterListViewItems.Clear();
        }
    }
}
