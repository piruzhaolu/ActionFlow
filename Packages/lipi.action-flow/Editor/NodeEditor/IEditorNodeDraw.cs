using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ActionFlow
{
    public interface IEditorNodeDraw 
    {
        void Create(EditorActionNode node, SerializedProperty asset);

        void InputDraw(EditorActionNode node, SerializedProperty asset);

        void OutputDraw(EditorActionNode node, SerializedProperty asset);

        void ExtensionDraw(EditorActionNode node, SerializedProperty asset);

        void DoubleClick(EditorActionNode node, SerializedProperty asset);

    }

}
