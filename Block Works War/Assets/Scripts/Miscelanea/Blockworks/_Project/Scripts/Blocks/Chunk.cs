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
            RecalculateMass();
        }

        public IEnumerable<Socket> Sockets
        {
            get
            {
                foreach (Block b in Blocks)
                {
                    if (!b.Sockets.Any())
                        continue;
                        
                    foreach (Socket socket in b.Sockets)
                    {
                        yield return socket;
                    }
                }
            }
        }

        public IEnumerable<Socket> EmptySockets
        {
            get
            {
                foreach (Socket socket in Sockets)
                {
                    if (!socket.IsConnected)
                        yield return socket;
                }
            }
        }

        public bool IsAnchored => Blocks.Any(l => l.IsAnchored);

        // TODO: Cache this
        public IEnumerable<Block> Blocks => GetComponentsInChildren<Block>();

        public IEnumerable<SocketPair> GetConnections()
        {
            HashSet<SocketPair> connected = new HashSet<SocketPair>();
            foreach (Socket s in EmptySockets)
            {
                Socket candidate = s.GetSocketCandidate();
                if (candidate != null)
                {
                    connected.Add(new SocketPair { This = s, Other = candidate });
                }
            }

            return connected;
        }

        private void OnDrawGizmosSelected()
        {
            // var connections = GetConnections();
            // for (var i = 0; i < connections.Length; i++)
            // {
            //     var color = Color.white;
            //     var size = 0.01f;
            //     if (i == 0) color = Color.red;
            //     if (i == 1) color = Color.blue;
            //     if (i >= 2) size = 0.005f;

            //     var from = connections[i].thisSocket.transform.position;
            //     var to = connections[i].otherSocket.transform.position;

            //     Gizmos.color = color.SetAlpha(.5f);
            //     Gizmos.DrawSphere(from, size);
            //     Gizmos.DrawSphere(to, size);
            //     Gizmos.DrawLine(from, to);
            // }

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
            RecalculateMass();
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

            RecalculateMass();
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

        private void RecalculateMass()
        {
            var rb = GetComponent<Rigidbody>();
            rb.mass = GetComponentsInChildren<Block>().Sum(b => b.Mass);
            rb.ResetCenterOfMass();
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