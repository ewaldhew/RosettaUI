﻿using System;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class ElementBindingExtension
    {
        // Subscribe element -> field
        public static void SubscribeValueOnUpdateCallOnce<T>(this ReadOnlyValueElement<T> element, INotifyValueChanged<T> field)
        {
            element.GetViewBridge().SubscribeValueOnUpdateCallOnce(field.SetValueWithoutNotifyIfNotEqual);
        }
        
        // Subscribe field -> element
        private static void Subscribe<TValue>(this BaseField<TValue> field, FieldBaseElement<TValue> element)
        {
            field.Subscribe(element, v => v);
        }
        
        // Subscribe field -> element
        private static void Subscribe<TFieldValue, TElementValue>(
            this BaseField<TFieldValue> field,
            FieldBaseElement<TElementValue> element,
            Func<TFieldValue, TElementValue> fieldValueToElementValue)
        {
            var viewBridge = element.GetViewBridge();
            
            field.RegisterValueChangedCallback(OnValueChanged);
            viewBridge.onUnsubscribe += () => field?.UnregisterValueChangedCallback(OnValueChanged);
            
            // TFieldValueがstringのとき
            // ラベルの更新とBaseFieldの値の更新の区別がつかないのでラベルのイベントは止めておく
            field.labelElement.RegisterValueChangedCallback(e => e.StopPropagation());

            void OnValueChanged(ChangeEvent<TFieldValue> evt)
            {
                viewBridge.SetValueFromView(fieldValueToElementValue(evt.newValue));
            }
        }

        // Subscribe element <-> field
        public static void Bind<T>(this FieldBaseElement<T> element, BaseField<T> field)
        {
            element.SubscribeValueOnUpdateCallOnce(field);
            field.Subscribe(element);
        }

        // Subscribe element <-(change value)-> field
        public static void Bind<TElementValue, TFieldValue>(
            this FieldBaseElement<TElementValue> element,
            BaseField<TFieldValue> field,
            Func<TElementValue, TFieldValue> elementValueToFieldValue,
            Func<TFieldValue, TElementValue> fieldValueToElementValue
        )
        {
            var viewBridge = element.GetViewBridge();
            
            viewBridge.SubscribeValueOnUpdateCallOnce(v => field.SetValueWithoutNotifyIfNotEqual(elementValueToFieldValue(v)));
            field.Subscribe(element, fieldValueToElementValue);
        }
    }
}