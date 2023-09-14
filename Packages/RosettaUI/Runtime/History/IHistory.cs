using System;

namespace RosettaUI
{
    public interface IHistory
    {
        void PushRecord<T>(Action<T> setState, T beforeState, T afterState);
        void Undo();
        void Redo();
    }
}
