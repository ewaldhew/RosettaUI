using System;
using UnityEngine;

namespace RosettaUI.Example
{
    public class HistoryExample : MonoBehaviour, IElementCreator
    {
        private void Start()
        {
            UIHistory.Create<LinearHistory>();
        }

        public Element CreateElement(LabelElement label)
        {
            return UI.Column(
                UI.Button("Undo", UIHistory.Undo),
                UI.Button("Redo", UIHistory.Redo)
            );
        }
    }
}
