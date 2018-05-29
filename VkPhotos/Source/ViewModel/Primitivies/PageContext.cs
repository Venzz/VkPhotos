using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venz.Data;

namespace VkPhotos.ViewModel
{
    public class PageContext: ObservableObject
    {
        private Object ProgressNotificationSync = new Object();
        private List<ProgressNotificationUpdate> Updates = new List<ProgressNotificationUpdate>();

        internal ProgressNotificationUpdate ProgressNotification { get; private set; }

        internal event EventHandler<ProgressNotificationUpdate> ProgressNotificationUpdated = delegate { };



        public ProgressNotificationId ShowProgress(String message)
        {
            lock (ProgressNotificationSync)
            {
                var update = new ProgressNotificationUpdate() { IsVisible = true, IsIndeterminate = true, Message = message, Value = null };
                Updates.Add(update);
                OnProgressNotificationUpdated(update);
                return update.Id;
            }
        }

        public async Task ShowProgressMessageAsync(String message)
        {
            var update = new ProgressNotificationUpdate() { IsVisible = true, IsIndeterminate = false, Message = message, Value = null };
            lock (ProgressNotificationSync)
            {
                Updates.Add(update);
                OnProgressNotificationUpdated(update);
            }
            await Task.Delay(2000);
            HideProgress(update.Id);
        }

        public ProgressNotificationId StartProgress(String messageFormat, params Object[] args)
        {
            lock (ProgressNotificationSync)
            {
                var update = new ProgressNotificationUpdate() { IsVisible = true, IsIndeterminate = false, Message = String.Format(messageFormat, args), Value = 0 };
                Updates.Add(update);
                OnProgressNotificationUpdated(update);
                return update.Id;
            }
        }

        public void ContinueProgress(ProgressNotificationId notificationId, Double progress, String message)
        {
            lock (ProgressNotificationSync)
            {
                if (Updates.Count == 0)
                    return;

                var isActive = Updates[Updates.Count - 1].Id == notificationId;
                var update = isActive ? Updates[Updates.Count - 1].Clone() : Updates.Find(a => a.Id == notificationId);
                if (update == null)
                    return;

                update.Message = message;
                update.Value = progress;

                if (isActive)
                {
                    Updates[Updates.Count - 1] = update;
                    OnProgressNotificationUpdated(update);
                }
            }
        }

        public void EndProgress(ProgressNotificationId notificationId)
        {
            if (notificationId == null)
                return;

            lock (ProgressNotificationSync)
            {
                Updates.RemoveAll(a => a.Id == notificationId);
                if (Updates.Count == 0)
                    OnProgressNotificationUpdated(null);
                else if ((ProgressNotification != Updates[Updates.Count - 1]))
                    OnProgressNotificationUpdated(ProgressNotification);
            }
        }

        public void HideProgress(ProgressNotificationId notificationId)
        {
            if (notificationId == null)
                return;

            lock (ProgressNotificationSync)
            {
                Updates.RemoveAll(a => a.Id == notificationId);
                if (Updates.Count == 0)
                    OnProgressNotificationUpdated(null);
                else if ((ProgressNotification != Updates[Updates.Count - 1]))
                    OnProgressNotificationUpdated(ProgressNotification);
            }
        }

        public async void HideProgress(ProgressNotificationId notificationId, String message)
        {
            HideProgress(notificationId);
            await ShowProgressMessageAsync(message);
        }

        private void OnProgressNotificationUpdated(ProgressNotificationUpdate value)
        {
            ProgressNotification = value;
            ProgressNotificationUpdated(this, value);
        }

        internal class ProgressNotificationUpdate
        {
            internal ProgressNotificationId Id { get; private set; }
            internal Boolean IsVisible { get; set; }
            internal Boolean IsIndeterminate { get; set; }
            internal String Message { get; set; }
            internal Double? Value { get; set; }

            internal ProgressNotificationUpdate() { Id = ProgressNotificationId.Create(); }

            internal ProgressNotificationUpdate Clone()
            {
                return new ProgressNotificationUpdate()
                {
                    Id = Id,
                    IsVisible = IsVisible,
                    IsIndeterminate = IsIndeterminate,
                    Message = Message,
                    Value = Value
                };
            }
        }

        public class ProgressNotificationId
        {
            private static Object Sync = new Object();
            private static Int32 Prev32;
            private Int32 Value;

            private ProgressNotificationId() { }

            internal static ProgressNotificationId Create()
            {
                var id = new ProgressNotificationId();
                lock (Sync) { Prev32 += 1; id.Value = Prev32; }
                return id;
            }

            public static Boolean operator ==(ProgressNotificationId id1, ProgressNotificationId id2)
            {
                if (((Object)id1 == null) && ((Object)id2 == null))
                    return true;
                if (((Object)id1 == null) || ((Object)id2 == null))
                    return false;
                return (id1.Value == id2.Value);
            }

            public static Boolean operator !=(ProgressNotificationId id1, ProgressNotificationId id2)
            {
                if (((Object)id1 == null) && ((Object)id2 == null))
                    return false;
                if (((Object)id1 == null) || ((Object)id2 == null))
                    return true;
                return (id1.Value != id2.Value);
            }

            public override Boolean Equals(Object obj)
            {
                var peerId = obj as ProgressNotificationId;
                return this == peerId;
            }

            public override Int32 GetHashCode()
            {
                unchecked
                {
                    Int32 hash = 17;
                    hash = hash * 23 + Value.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
