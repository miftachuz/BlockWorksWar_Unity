using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blocks.Builder;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks
{
    public class Chunk : MonoBehaviour
    {
        private void Start()
        {
            var rb = GetComponent<Rigidbody>();
            rb.mass = GetComponentsInChildren<Block>().Sum(block => block.Mass);
            rb.ResetCenterOfMass();
        }

        public (Transform thisSocket, Transform otherSocket)[] GetConnections()
        {
            var sockets = transform.GetComponentsInChildren<Socket>();
            var connected = new Socket[sockets.Length];

            // Find all connected pairs
            for (var i = 0; i < sockets.Length; i++)
            {
                var socket = sockets[i];
                var candidate = socket.GetComponent<Socket>().Trigger();
                if (candidate.IsEmpty() == false)
                {
                    var closest = candidate
                        .OrderBy(o => o.transform.position.Distance(socket.transform.position))
                        .First();

                    var sockets2 = closest;

                    connected[i] = closest;

                }
            }

            // Sort connection for the closest one
            return sockets.Select((socket, i) =>
                {
                    if (connected[i] == null) return null;
                    var dist = sockets[i].transform.position.Distance(connected[i].transform.position);
                    return new {Index = i, Distance = dist};
                })
                .Where(it => it != null)
                .OrderBy(arg => arg.Distance)
                .Select(arg => (sockets[arg.Index].transform, connected[arg.Index].transform))
                .ToArray();
        }

        public bool IsAnchored => Blocks.Any(l => l.IsAnchored);

        public IEnumerable<Block> Blocks => GetComponentsInChildren<Block>();

        private void OnDrawGizmosSelected()
        {
            var connections = GetConnections();
            for (var i = 0; i < connections.Length; i++)
            {
                var color = Color.white;
                var size = 0.01f;
                if (i == 0) color = Color.red;
                if (i == 1) color = Color.blue;
                if (i >= 2) size = 0.005f;

                var from = connections[i].thisSocket.transform.position;
                var to = connections[i].otherSocket.transform.position;

                Gizmos.color = color.SetAlpha(.5f);
                Gizmos.DrawSphere(from, size);
                Gizmos.DrawSphere(to, size);
                Gizmos.DrawLine(from, to);
            }

            foreach (var (a, b) in GetComponentInChildren<Block>().GetAllConnections())
            {
                Gizmos.color = Color.yellow;

                var from = a.transform.TransformPoint(a.gameObject.GetCompositeMeshBounds().center);
                var to = b.transform.TransformPoint(b.gameObject.GetCompositeMeshBounds().center);
                Gizmos.DrawLine(from, to);
                Gizmos.DrawSphere(from, .0025f);
                Gizmos.DrawSphere(to, .0025f);
            }

            var centerOfMass = transform.InverseTransformPoint(GetComponent<Rigidbody>().centerOfMass);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(centerOfMass, .0025f);
        }

        public void Connect(IEnumerable<SocketPair> socketPairs, IEnumerable<Chunk> chunks)
        {
            ConnectSockets(socketPairs);
            ConnectChunks(chunks);
        }

        private void ConnectSockets(IEnumerable<SocketPair> socketPairs)
        {
            foreach (var (thisSocket, otherSocket) in socketPairs)
            {
                thisSocket.Connect(otherSocket);
            }
        }

        private void ConnectChunks(IEnumerable<Chunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                foreach (var block in chunk.Blocks)
                {
                    block.ConnectTo(this);
                }

                Destroy(chunk.gameObject);
            }
        }

        public void Disconnect(IEnumerable<Socket> sockets, IEnumerable<IEnumerable<Block>> groups)
        {
            DisconnectSockets(sockets);
            DisconnectChunks(groups);
        }

        private void DisconnectSockets(IEnumerable<Socket> sockets)
        {
            foreach (var socket in sockets)
            {
                socket.Disconnect();
            }
        }

        private void DisconnectChunks(IEnumerable<IEnumerable<Block>> groups)
        {
            foreach (var group in groups)
            {
                var chunk = CreateChunk();

                // Align the chunk origin with first block
                var firstBlockTransform = group.First().transform;
                chunk.transform.position = firstBlockTransform.position;
                chunk.transform.rotation = firstBlockTransform.rotation;

                foreach (var block in group)
                {
                    block.ConnectTo(chunk);
                }
            }
        }

        private Chunk CreateChunk()
        {
            var chunk = new GameObject().AddComponent<Chunk>();

            var snapper = chunk.gameObject.AddComponent<BuildPreviewManager>();
            var chunkRb = chunk.gameObject.AddComponent<Rigidbody>();
            chunkRb.interpolation = RigidbodyInterpolation.Interpolate;
            chunkRb.isKinematic = chunk.IsAnchored;

            return chunk;
        }

        

    }
}