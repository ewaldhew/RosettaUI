﻿using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace RosettaUI
{
    public static partial class UI
    {
        #region targetExpression

        public static Element List<TList> (
            Expression<Func<TList>> targetExpression,
            in ListViewOption option
        )
            where TList : IList
            => List(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, null, option);

        public static Element List<TList>(
            Expression<Func<TList>> targetExpression,
            Func<IBinder, int, Element> createItemElement = null,
            in ListViewOption? option = null
        )
            where TList : IList
            => List(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, createItemElement, option);

        public static Element List<TList>(
            LabelElement label,
            Expression<Func<TList>> targetExpression,
            in ListViewOption option
        )
            where TList : IList
            => List(label, targetExpression, null, option);

        public static Element List<TList>(
            LabelElement label,
            Expression<Func<TList>> targetExpression,
            Func<IBinder, int, Element> createItemElement = null,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(
                label,
                ExpressionUtility.CreateBinder(targetExpression),
                createItemElement,
                option ?? CalcDefaultOptionOf(targetExpression)
            );
        }

        #endregion


        #region readValue/writeValue

        public static Element List<TList>(
            Expression<Func<TList>> targetExpression,
            Action<TList> writeValue,
            in ListViewOption option
        )
            where TList : IList
            => List(targetExpression, writeValue, null, option);

        public static Element List<TList>(
            LabelElement label,
            Func<TList> readValue,
            Action<TList> writeValue,
            in ListViewOption option
        )
            where TList : IList
            => List(label, readValue, writeValue, null, option);

        public static Element List<TList>(
            Expression<Func<TList>> targetExpression,
            Action<TList> writeValue,
            Func<IBinder, int, Element> createItemElement = null,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(
                ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression.Compile(),
                writeValue,
                createItemElement,
                option ?? CalcDefaultOptionOf(targetExpression));
        }

        public static Element List<TList>(
            LabelElement label,
            Func<TList> readValue,
            Action<TList> writeValue,
            Func<IBinder, int, Element> createItemElement = null,
            in ListViewOption? option = null
        )
            where TList : IList
            => List(label, Binder.Create(readValue, writeValue), createItemElement, option);

        #endregion


        #region ReadOnly

        public static Element ListReadOnly<TList>(
            Expression<Func<TList>> targetExpression,
            in ListViewOption option
        )
            where TList : IList
            => ListReadOnly(targetExpression, null, option);

        public static Element ListReadOnly<TList>(
            Expression<Func<TList>> targetExpression,
            Func<IBinder, int, Element> createItemElement = null,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            return List(
                ExpressionUtility.CreateLabelString(targetExpression),
                UIInternalUtility.CreateReadOnlyBinder(targetExpression),
                createItemElement,
                option ?? CalcDefaultOptionOf(targetExpression));
        }

        public static Element ListReadOnly<TList>(
            LabelElement label,
            Func<TList> readValue,
            in ListViewOption option
        )
            where TList : IList
            => ListReadOnly(label, readValue, null, option);

        public static Element ListReadOnly<TList>(
            LabelElement label,
            Func<TList> readValue,
            Func<IBinder, int, Element> createItemElement = null,
            in ListViewOption? option = null
        )
            where TList : IList
        {
            var binder = Binder.Create(readValue, null);
            return List(label, binder, createItemElement, option);
        }

        #endregion


        #region Core

        public static Element List(LabelElement label, IBinder listBinder, Func<IBinder, int, Element> createItemElement = null, in ListViewOption? optionNullable = null)
        {
            var option = optionNullable ?? ListViewOption.Default;

            var listItemContainer = ListItemContainer(listBinder, createItemElement, option);
            var ret = listItemContainer;

            if (option.header)
            {
                var countField = ListCounterField(listBinder, listItemContainer, option);

                ret = Fold(
                    label, countField,
                    new[]
                    {
                        listItemContainer
                    }
                ).Open();
            }

            UIInternalUtility.SetInteractableWithBinder(ret, listBinder);
            UIInternalUtility.RegisterHistoryRecorder(listItemContainer.Children.First(), listBinder);

            return ret;
        }

        public static Element ListCounterField(IBinder listBinder,　Element itemContainerElement, in ListViewOption option)
        {
            var interactable = !ListBinder.IsReadOnly(listBinder) && !option.fixedSize;

            return Field(null,
                () => ListBinder.GetCount(listBinder),
                count =>
                {
                    // ListViewItemContainerElementが存在していたら新しいcountを通知
                    // NullGuardで存在してない場合もありそのときはlistBinderに直接セットする
                    var containerElement = itemContainerElement.Query<ListViewItemContainerElement>().FirstOrDefault();
                    if (containerElement != null)
                    {
                        containerElement.ListItemCount = count;
                    }
                    else
                    {
                        ListBinder.SetCount(listBinder, count);
                    }
                },
                new FieldOption { disableHistory = true }
                ).SetMinWidth(50f).SetInteractable(interactable);
        }

        private static Element ListItemContainer(IBinder listBinder, Func<IBinder, int, Element> createItemElement, in ListViewOption option)
        {
            var optionCaptured = option;
            return NullGuard(null, listBinder,
                () => new ListViewItemContainerElement(
                    listBinder,
                    createItemElement ?? ListItemDefault,
                    optionCaptured)
            );
        }

        public static Element ListItemDefault(IBinder binder, int index) => Field($"Item {index}", binder);

        private static ListViewOption? CalcDefaultOptionOf(LambdaExpression expression)
            => ExpressionUtility.GetAttribute<NonReorderableAttribute>(expression) != null
                    ? new ListViewOption(reorderable: false)
                    : null;


        #endregion
    }
}