using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASKMNGMT.Models.HelperClasses
{
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        public void AddRange(IEnumerable<T> items)
        {
            _suppressNotification = true; // Suppress notifications

            foreach (var item in items)
            {
                Items.Add(item); // Add items to the collection without firing CollectionChanged events
            }

            _suppressNotification = false;
            // Notify once after all items are added
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Only trigger CollectionChanged if notifications are not suppressed
            if (!_suppressNotification)
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}
