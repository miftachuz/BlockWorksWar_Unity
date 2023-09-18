using System;
using System.Collections.Generic;
using System.Linq;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks
{
    [ExecuteInEditMode]
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
                for (var i = 0; i < sockets.Length; i++)
                {
                    var socket = sockets[i];
                    if (socket.ConnectedSocket != null)
                    {
                        yield return socket.ConnectedSocket.Block;
                    }
                }
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

        private void Awake()
        {
            sockets = GetComponentsInChildren<Socket>();
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
            sockets = sockets.Append(socket).ToArray();
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

        [ContextMenu("Disconnect")]
        private void Disconnect()
        {
            ChunkFactory.Disconnect(chunk, new[] { this });
        }

    }
}