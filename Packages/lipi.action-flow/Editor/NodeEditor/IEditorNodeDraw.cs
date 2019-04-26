using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    public interface IEditorNodeDraw 
    {
        void Create(EditorActionNode node, ScriptableObject asset);

        void InputDraw(EditorActionNode node, ScriptableObject asset);

        void OutputDraw(EditorActionNode node, ScriptableObject asset);

        void ExtensionDraw(EditorActionNode node, ScriptableObject asset);

        void DoubleClick(EditorActionNode node, ScriptableObject asset);

    }

}
