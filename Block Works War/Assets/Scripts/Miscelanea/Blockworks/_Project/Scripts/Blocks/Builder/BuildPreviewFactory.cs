using System;
using System.Linq;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks.Builder
{
    public class BuildPreviewFactory
    {
        public static BuildPreview Build(Chunk chunk)
        {
            var chunkPreviewGo = new GameObject($"{chunk.name} Preview");
            CopyHierarchyAndComponents(chunk.transform, chunkPreviewGo.transform);

            var chunkPreview = chunkPreviewGo.AddComponent<BuildPreview>();
            chunkPreview.Owner = chunk;
            chunkPreview.Connector = chunkPreviewGo.AddComponent<BuildPreviewConnector>();

            // Set all materials to translucent
            var translucentMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            translucentMat.SetupMaterialWithBlendMode(MaterialExtensions.Mode.Fade);
            var renderers = chunkPreviewGo.GetComponentsInChildren<Renderer>();
            chunkPreview.Renderers = renderers;
            foreach (var r in renderers)
            {
                r.materials = r.materials.Select(m => translucentMat).ToArray();
            }

            chunkPreview.Material = translucentMat;

            // Setup rigidbody
            var component = chunkPreviewGo.AddComponent<Rigidbody>();
            component.isKinematic = true;
            component.interpolation = RigidbodyInterpolation.Interpolate;

            // Link real sockets to preview sockets
            var chunkSockets = chunk.GetComponentsInChildren<Socket>();
            var previewSockets = chunkPreviewGo.GetComponentsInChildren<Socket>();
            chunkPreview.PreviewToRealSocketMap = chunkSockets
                .Zip(previewSockets, (s0, s1) => (Chunk: s0, Preview: s1))
                .ToDictionary(tuple => tuple.Preview, tuple => tuple.Chunk);

            return chunkPreview;
        }

        private static void CopyHierarchyAndComponents(Transform from, Transform to)
        {
            CopyComponents(from, to);

            foreach (Transform fromChild in from)
            {
                var toChild = new GameObject().transform;
                toChild.SetParent(to, false);
                toChild.CopyLocalFrom(fromChild);
                toChild.name = fromChild.name;

                CopyHierarchyAndComponents(fromChild, toChild);
            }
        }

        private static void CopyComponents(Transform from, Transform to)
        {
            CopyCollider(from, to);
            CopyMeshFilter(from, to);
            CopyRenderer(from, to);
            CopySocket(from, to);
        }

        private static void CopySocket(Transform from, Transform to)
        {
            var fromSocket = from.GetComponent<Socket>();
            if (fromSocket)
            {
                var toSocket = to.gameObject.AddComponent<Socket>();
                toSocket.Block = fromSocket.Block;
                toSocket.Active = false;
            }
        }

        private static void CopyRenderer(Transform from, Transform to)
        {
            var fromRenderer = from.GetComponent<MeshRenderer>();
            if (fromRenderer)
            {
                var toRenderer = to.gameObject.AddComponent<MeshRenderer>();
                toRenderer.materials = new Material[fromRenderer.materials.Length];
            }
        }

        private static void CopyMeshFilter(Transform from, Transform to)
        {
            var fromMf = from.GetComponent<MeshFilter>();
            if (fromMf)
            {
                var toMf = to.gameObject.AddComponent<MeshFilter>();
                toMf.mesh = fromMf.mesh;
            }
        }

        private static void CopyCollider(Transform from, Transform to)
        {
            var isBlock = from.GetComponent<Block>() == true;
            if (isBlock)
            {
                var collider = from.GetComponent<Collider>();
                if (collider is BoxCollider == false)
                {
                    throw new InvalidOperationException("Only box colliders are supported at this time.");
                }

                var fromBox = collider as BoxCollider;

                var toBox = to.gameObject.AddComponent<BoxCollider>();
                toBox.center = fromBox.center;
                toBox.isTrigger = true;
                toBox.size = fromBox.size - Vector3.one * .001f;
            }
        }
    }
}