using System;

namespace RosettaUI
{
    public interface IBinder : IGetter
    {
        bool IsReadOnly { get; }

        object GetObject();
        void SetObject(object obj);

        void SubscribeValueChange(Action<Action<object>, object, object> onValueChange);
    }

    public interface IBinder<T> : IBinder, IGetter<T>
    {
        void Set(T v);
    }
}