using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public class LinearHistory : IHistory
    {
        private struct HistoryRecord
        {
            public Action toBefore;
            public Action toAfter;
        }

        private Stack<HistoryRecord> UndoStack = new();
        private Stack<HistoryRecord> RedoStack = new();

        public void PushRecord<T>(Action<T> setState, T beforeState, T afterState)
        {
            var record = new HistoryRecord { toBefore = () => setState?.Invoke(beforeState), toAfter =  () => setState?.Invoke(afterState) };
            RedoStack.Clear();
            UndoStack.Push(record);
        }

        public void Undo()
        {
            if (!UndoStack.TryPop(out var record))
            {
                return;
            }

            record.toBefore();
            RedoStack.Push(record);
        }

        public void Redo()
        {
            if (!RedoStack.TryPop(out var record))
            {
                return;
            }

            record.toAfter();
            UndoStack.Push(record);
        }
    }
}
