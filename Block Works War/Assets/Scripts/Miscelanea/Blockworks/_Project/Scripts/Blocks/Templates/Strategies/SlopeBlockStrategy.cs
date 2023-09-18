using Blocks.Sockets;
using UnityEngine;

namespace Blocks.Templates.Strategies
{
    public class SlopeBlockStrategy : TemplateStrategy
    {
        private Mesh blockMesh;
        private Vector3Int size;
        private Vector2Int offset;

        public SlopeBlockStrategy(Mesh blockMesh, Mesh pinMesh, Vector3Int size, Vector2Int offset) : base(pinMesh)
        {
            this.blockMesh = blockMesh;
            this.size = size;
            this.offset = offset;
        }

        public override void Build(Block block)
        {
            var mesh = BasicMesh();

            block.GetComponent<MeshFilter>().mesh = mesh;

            var collider = block.gameObject.AddComponent<BoxCollider>();
            collider.center = mesh.bounds.center;
            collider.size = mesh.bounds.size;

            AddSocket(block, new Vector3(0, 0.02f * (size.y + 1), 0), offset.x + 1, size.z);
            AddSocket(block, new Vector3(0, 0, 0), size.x + 1, size.z);
        }

        private Mesh BasicMesh()
        {
            var bounds = blockMesh.bounds;
            var vertices = blockMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];
                if (Mathf.Abs(0.05f - vert.x) < 0.001f)
                {
                    vert.x = vert.x + offset.x * .05f;
                }
                else
                {
                    vert.x = vert.x > .051f ? vert.x + (size.x - 1) * .05f : vert.x;
                }

                if (Mathf.Abs(0.02f - vert.y) < 0.001f)
                {
                    vert.y = vert.y + offset.y * .02f;
                }
                else
                {
                    vert.y = vert.y > .021f ? vert.y + (size.y - 1) * .02f : vert.y;
                }

                vert.z = vert.z > bounds.center.z / 2 ? vert.z + (size.z - 1) * .05f : vert.z;
                vertices[i] = vert;
            }

            var newMesh = new Mesh
            {
                vertices = vertices,
                triangles = blockMesh.triangles,
                normals = blockMesh.normals,
                uv = blockMesh.uv,
                tangents = blockMesh.tangents
            };
            newMesh.RecalculateBounds();
            return newMesh;
        }
    }
}