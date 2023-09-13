using System;

namespace RosettaUI
{
    /// <summary>
    /// Binder, which targets a portion of the parents
    /// </summary>
    public abstract class ChildBinder<TParent, TValue> : BinderBase<TValue>
    {
        protected readonly IBinder<TParent> parentBinder;

        protected ChildBinder(IBinder<TParent> parentBinder)
        {
            this.parentBinder = parentBinder;
            getter = Getter.Create(() => GetFromParent(parentBinder.Get()));
        }


        public override bool IsNull => parentBinder.IsNull || base.IsNull;
        public override bool IsNullable => parentBinder.IsNullable || base.IsNullable;

        public override bool IsReadOnly => false;

        protected abstract TValue GetFromParent(TParent parent);
        protected abstract TParent SetToParent(TParent parent, TValue value);

        protected override void SetInternal(TValue value)
        {
            var parent = parentBinder.Get();
            var prevValue = GetFromParent(parent);
            parent = SetToParent(parent, value);
            parentBinder.Set(parent);
            onValueChanged?.Invoke(prevValue, value);
        }

        public event Action<TValue, TValue> onValueChanged;

        public override void SubscribeValueChange(Action<Action<object>, object, object> func)
        {
            Action<object> setObject = obj =>
            {
                var parent = parentBinder.Get();
                parent = SetToParent(parent, (TValue) obj);
                parentBinder.Set(parent);
            };
            onValueChanged += (prevVal, currVal) =>
            {
                if (!Equals(prevVal, currVal)) func(setObject, prevVal, currVal);
            };
        }
    }
}