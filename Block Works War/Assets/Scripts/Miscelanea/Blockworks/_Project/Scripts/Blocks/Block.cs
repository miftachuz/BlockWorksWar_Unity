using System;
using System.Collections.Generic;
using System.Linq;
using Blocks.Builder;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks
{
    public class Block : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Chunk chunk;
        [SerializeField] private Socket[] sockets;

        [Header("Parameters")]
        public bool IsAnchored;
        public BlockMaterial BlockMaterial;

        public Chunk Chunk
        {
            get => chunk;
            set => chunk = value;
        }

        private IEnumerable<Block> Connections
        {
            get
            {
                for (int i = 0; i < sockets.Length; i++)
                {
                    if (sockets[i].IsConnected)
                    {
                        yield return sockets[i].ConnectedSocket.Block;
                    }
                }

                // for (var i = 0; i < sockets.Length; i++)
                // {
                //     var socket = sockets[i];
                //     if (socket.ConnectedSocket != null)
                //     {
                //         yield return socket.ConnectedSocket.Block;
                //     }
                // }
            }
        }

        public IEnumerable<Socket> Sockets => sockets;

        public float Mass
        {
            get
            {
                var boundsSize = GetComponent<Collider>().bounds.size;
                return boundsSize.x * boundsSize.y * boundsSize.z * BlockMaterial.Density;
            }
        }

        private void Start()
        {
            if (sockets != null && sockets.Length > 0)
            {
                for (int i = 0; i < sockets.Length; i++)
                {
                    if (sockets[i].IsInitialized)
                        continue;
                        
                    sockets[i].Init(this);
                }
            }
        }

        public HashSet<Block> GetAllConnectedBlocks(IEnumerable<Block> ignore = null)
        {
            var allConnections = new HashSet<Block>();
            allConnections.Add(this);
            var ignoreSet = ignore?.ToSet();
            GetAllConnectedBlocks(this, allConnections, ignoreSet);
            return allConnections;
        }

        public HashSet<(Block from, Block to)> GetAllConnections()
        {
            var allConnections = new HashSet<(Block, Block)>();
            GetAllConnections(this, allConnections);
            return allConnections;
        }

        public void AddSocket(Socket socket)
        {
            if (sockets == null)
                sockets = new Socket[0];

            sockets = sockets.Append(socket).ToArray();
        }   

        public void AddSocket(Vector3 pos, Quaternion rot, bool isActive)
        {
            Socket s = new Socket { LocalPosition = pos, LocalOrientation = rot, IsActive = isActive }; 
            s.Init(this);
            AddSocket(s);
        }

        private void GetAllConnectedBlocks(Block parent, HashSet<Block> allConnections, ISet<Block> ignore)
        {
            foreach (var connection in parent.Connections)
            {
                if (ignore != null && ignore.Contains(connection))
                {
                    continue;
                }

                if (!allConnections.Contains(connection))
                {
                    allConnections.Add(connection);
                    GetAllConnectedBlocks(connection, allConnections, ignore);
                }
            }
        }

        private void GetAllConnections(Block parent, ISet<(Block, Block)> allConnections)
        {
            foreach (var connection in parent.Connections)
            {
                var edge = (parent, connection);
                if (!allConnections.Contains(edge))
                {
                    allConnections.Add(edge);
                    GetAllConnections(connection, allConnections);
                }
            }
        }

        public void ConnectTo(Chunk newChunk)
        {
            transform.SetParent(newChunk.transform, true);
            FixBlockOffset();
            Chunk = newChunk;
        }

        private void FixBlockOffset()
        {
            transform.localPosition = transform.localPosition.RoundTo(0.05f, 0.02f, 0.05f);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.RoundTo(90, 90, 90));
        }

        private void OnDrawGizmos()
        {
            if (sockets.Length <= 0)
                return;

            Gizmos.color = Color.yellow.SetAlpha(0.55f);
            foreach (Socket socket in sockets)
            {
                if (!socket.IsInitialized && !Application.isPlaying)
                    socket.Init(this);

                Gizmos.DrawRay(socket.Position, socket.Up() * 0.0225f);

                if (socket.IsConnected)
                    Gizmos.color = Color.red.SetAlpha(0.55f);
                else
                    Gizmos.color = Color.yellow.SetAlpha(0.55f);

                Gizmos.DrawSphere(socket.Position, socket.Radius);
            }
        }

        [ContextMenu("Disconnect")]
        private void Disconnect()
        {
            ChunkFactory.Disconnect(chunk, new[] { this });
        }

        [ContextMenu("Generate Sockets From Existing")]
        private void GenerateSocketsFromExisting()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Unable to generate sockets while playing");
                return;
            }

            List<Socket> sockets = new List<Socket>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform c = transform.GetChild(i);
                string s = c.name.ToLower();
                if (s.Contains("male") || s.Contains("female"))
                {
                    Socket instance = new Socket { LocalPosition = c.transform.localPosition, LocalOrientation = c.transform.localRotation };
                    instance.Init(this);
                    sockets.Add(instance);
                }
            }

            Debug.Log("Generated " + sockets.Count + " sockets");
            this.sockets = sockets.ToArray();
        }

    }
}