using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ActionFlow
{
    public interface IEditorNodeDraw 
    {
        void Create(EditorActionNode node, INode asset);

        void InputDraw(EditorActionNode node, INode asset);

        void OutputDraw(EditorActionNode node, INode asset);

        void ExtensionDraw(EditorActionNode node, INode asset);

        void DoubleClick(EditorActionNode node, INode asset);

    }

}
