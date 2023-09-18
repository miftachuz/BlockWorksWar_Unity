using UnityEditor;
using UnityEngine;

namespace Blocks.Builder.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildPreviewManager))]
    public class ChunkSnapperEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Begin Snap")) foreach (BuildPreviewManager target in targets) target.StartPreview();
            if (GUILayout.Button("End Snap")) foreach (BuildPreviewManager target in targets) target.StopPreview();
            GUILayout.EndHorizontal();
        }
    }
}