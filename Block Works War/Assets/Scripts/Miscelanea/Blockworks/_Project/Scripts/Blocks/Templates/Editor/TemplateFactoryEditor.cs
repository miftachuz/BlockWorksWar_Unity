using System.Collections.Generic;
using System.IO;
using System.Linq;
using ElasticSea.Framework.Extensions;
using UnityEditor;
using UnityEngine;

namespace Blocks.Templates.Editor
{
    [CustomEditor(typeof(TemplateFactory))]
    public class TemplateFactoryEditor : UnityEditor.Editor
    {
        private BlockTemplate currentTemplate;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var factory = target as TemplateFactory;

            GUILayout.Space(16);
            GUILayout.Label("Templates");
            foreach (var template in Templates)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(template.Name))
                {
                    currentTemplate = template;
                    var built = factory.Build(currentTemplate);
                    factory.transform.DestroyChildren(true);
                    built.transform.SetParent(factory.transform, true);
                }

                ;

                if (GUILayout.Button("Show", GUILayout.MaxWidth(50)))
                {
                    EditorGUIUtility.PingObject(template);
                }

                ;

                if (template.name != template.Name)
                {
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(template), template.Name);
                }

                GUILayout.EndHorizontal();
            }

            if (currentTemplate)
            {
                GUILayout.Space(16);
                GUILayout.Label("Current Template");
                var editor = CreateEditor(currentTemplate);
                editor.DrawDefaultInspector();
                if (GUILayout.Button("Duplicate"))
                {
                    var newTemplate = Instantiate(currentTemplate);
                    newTemplate.Name += " (1)";
                    newTemplate.prefab = null;
                    newTemplate.meshPrefab = null;
                    AssetDatabase.CreateAsset(newTemplate,
                        AssetDatabase.GetAssetPath(currentTemplate).Replace(currentTemplate.Name, newTemplate.Name));
                }
            }

            GUILayout.Space(16);
            if (GUILayout.Button("Build"))
            {
                foreach (var template in Templates)
                {
                    var built = factory.Build(template);
                    var path = serializedObject.FindProperty("blockPrefabPath").stringValue;

                    var meshPath = template.meshPrefab == null
                        ? Path.Combine(path, $"{template.Name}_mesh.asset")
                        : AssetDatabase.GetAssetPath(template.meshPrefab);

                    AssetDatabase.CreateAsset(built.GetComponentInChildren<MeshFilter>().sharedMesh, meshPath);
                    template.meshPrefab = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

                    if (template.meshPrefab.name != template.Name)
                    {
                        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(template.meshPrefab), template.Name);
                    }

                    var prefabPath = template.prefab == null
                        ? Path.Combine(path, $"{template.Name}.prefab")
                        : AssetDatabase.GetAssetPath(template.prefab);

                    template.prefab = PrefabUtility.SaveAsPrefabAsset(built, prefabPath);

                    if (template.prefab.name != template.Name)
                    {
                        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(template.prefab), template.Name);
                    }

                    AssetDatabase.SaveAssets();
                    DestroyImmediate(built);
                }
            }
        }

        private IEnumerable<BlockTemplate> Templates => AssetDatabase
            .FindAssets($"t:{nameof(BlockTemplate)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<BlockTemplate>);
    }
}