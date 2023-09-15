using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace RosettaUI
{
    public static class UIInternalUtility
    {
        public static IBinder<T> CreateReadOnlyBinder<T>(Expression<Func<T>> targetExpression)
        {
            return Binder.Create(
                ExpressionUtility.CreateReadFunc(targetExpression),
                null
            );
        }

        public static IBinder<T> CreateBinder<T>(Expression<Func<T>> targetExpression)
        {
            return ExpressionUtility.CreateBinder(targetExpression);
        }


        public static (IGetter<T>, IGetter<T>) CreateMinMaxGetterFromRangeAttribute<T>(Expression<Func<T>> targetExpression)
        {
            var rangeAttribute = typeof(IConvertible).IsAssignableFrom(typeof(T))
                ? ExpressionUtility.GetAttribute<RangeAttribute>(targetExpression)
                : null;

            return RangeUtility.CreateGetterMinMax<T>(rangeAttribute);
        }


        public static void SetInteractableWithBinder(Element element, IBinder binder)
        {
            element.Interactable = !binder.IsReadOnly;
        }

        private static Dictionary<IBinder, UIHistory.IHistoryListener<object>> BinderToRecorder = new();
        private static HashSet<IBinder> ExcludedBinders = new();
        public static void RegisterHistoryRecorder(Element element, IBinder binder)
        {
            if (!element.Interactable || binder.IsReadOnly) return;

            // Do not record from parent binders, work on the smallest unit (field/property).
            if (binder is IPropertyOrFieldBinder pfBinder)
            {
                var parentBinder = pfBinder.ParentBinder;
                if (BinderToRecorder.TryGetValue(parentBinder, out var existingRecorder))
                {
                    existingRecorder.Enabled = false;
                    parentBinder.onValueChanged -= existingRecorder.GetListener(parentBinder.SetObject);
                }
                ExcludedBinders.Add(parentBinder);
            }
            if (ExcludedBinders.Contains(binder) || BinderToRecorder.ContainsKey(binder)) return;

            var recorder = UIHistory.GetHistoryRecorder<object>();
            BinderToRecorder[binder] = recorder;

            binder.onValueChanged += recorder.GetListener(binder.SetObject);
        }
    }
}