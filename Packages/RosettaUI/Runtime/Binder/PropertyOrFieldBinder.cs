﻿using System;

namespace RosettaUI
{
    // 型とフィールドかプロパティ名から生成されるBinder
    public static class PropertyOrFieldBinder
    {
        public static IBinder Create(IBinder binder, string propertyOrFieldName)
        {
            var parent = binder.ValueType;
            var memberType = TypeUtility.GetPropertyOrFieldType(parent, propertyOrFieldName);
            var binderType = typeof(PropertyOrFieldBinder<,>).MakeGenericType(parent, memberType);

            return Activator.CreateInstance(binderType, binder, propertyOrFieldName) as IBinder;
        }
    }

    public interface IPropertyOrFieldBinder
    {
        IBinder ParentBinder { get; }
        string PropertyOrFieldName { get; }
    }

    public class PropertyOrFieldBinder<TParent, TValue> : ChildBinder<TParent, TValue>, IPropertyOrFieldBinder
    {
        public IBinder ParentBinder => parentBinder;
        public string PropertyOrFieldName { get; protected set; }

        public PropertyOrFieldBinder(IBinder<TParent> parentBinder, string propertyOrFieldName)
            : base(parentBinder,
                PropertyOrFieldGetterSetter<TParent, TValue>.GetGetterSetter(propertyOrFieldName).Item1,
                PropertyOrFieldGetterSetter<TParent, TValue>.GetGetterSetter(propertyOrFieldName).Item2
            )
        {
            PropertyOrFieldName = propertyOrFieldName;
        }
    }
}