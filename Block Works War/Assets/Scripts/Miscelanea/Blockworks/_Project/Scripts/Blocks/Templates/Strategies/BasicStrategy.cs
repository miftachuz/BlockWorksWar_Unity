using Blocks.Sockets;
using UnityEngine;

namespace Blocks.Templates.Strategies
{
    public class BasicStrategy : TemplateStrategy
    {
        private Mesh blockMesh;
        private Vector3Int size;

        public BasicStrategy(Mesh blockMesh, Mesh pinMesh, Vector3Int size) : base(pinMesh)
        {
            this.blockMesh = blockMesh;
            this.size = size;
        }

        public override void Build(Block block)
        {
            var mesh = BasicMesh();

            block.GetComponent<MeshFilter>().mesh = mesh;

            var collider = block.gameObject.AddComponent<BoxCollider>();
            collider.center = mesh.bounds.center;
            collider.size = mesh.bounds.size;

            AddSocket(block, new Vector3(0, 0.02f * size.y, 0), size.x, size.z);
            AddSocket(block, new Vector3(0, 0, 0), size.x, size.z);
        }

        private Mesh BasicMesh()
        {
            var bounds = blockMesh.bounds;
            var vertices = blockMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];
                var x = vert.x > bounds.center.x / 2 ? vert.x + (size.x - 1) * bounds.size.x : vert.x;
                var y = vert.y > bounds.center.y / 2 ? vert.y + (size.y - 1) * bounds.size.y : vert.y;
                var z = vert.z > bounds.center.z / 2 ? vert.z + (size.z - 1) * bounds.size.z : vert.z;
                vertices[i] = new Vector3(x, y, z);
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