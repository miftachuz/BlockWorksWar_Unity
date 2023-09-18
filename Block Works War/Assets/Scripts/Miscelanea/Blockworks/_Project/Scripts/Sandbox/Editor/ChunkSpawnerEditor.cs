using UnityEditor;
using UnityEngine;

namespace Sandbox.Editor
{
    [CustomEditor(typeof(ChunkSpawner))]
    public class ChunkSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Spawn"))
            {
                (target as ChunkSpawner).Spawn();
            }
        }
    }
}