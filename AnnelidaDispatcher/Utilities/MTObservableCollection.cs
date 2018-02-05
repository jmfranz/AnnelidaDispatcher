//THIS CLASS IS WAS NOT CREATED BY ME
//origin maybe: http://stackoverflow.com/users/740378/nathan-phillips

#pragma warning disable 1591

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace AnnelidaDispatcher.Utilities
{
    /// <summary>
    /// Multi-thread Observable collection class taht allows for non-UI threads to dispach events in the UI-thread
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public class MTObservableCollection<T> : ObservableCollection<T>
    {

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler CollectionChanged = this.CollectionChanged;
            if (CollectionChanged != null)
                foreach (NotifyCollectionChangedEventHandler nh in CollectionChanged.GetInvocationList())
                {
                    DispatcherObject dispObj = nh.Target as DispatcherObject;
                    if (dispObj != null)
                    {
                        Dispatcher dispatcher = dispObj.Dispatcher;
                        if (dispatcher != null && !dispatcher.CheckAccess())
                        {
                            dispatcher.BeginInvoke(
                                (Action)(() => nh.Invoke(this,
                                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
                                DispatcherPriority.DataBind);
                            continue;
                        }
                    }
                    nh.Invoke(this, e);
                }
        }
    }
}
