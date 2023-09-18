using System;
using System.Collections.Generic;
using System.Linq;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;

namespace Blocks
{
    public static class ChunkFactory
    {
        public static Chunk Connect(IEnumerable<SocketPair> socketPairs)
        {
            var allChunks = GetChunksFromSocketPairs(socketPairs);
            var (main, rest) = allChunks.SeparateMainGroup(chunk => chunk.Blocks);
            main.Connect(socketPairs, rest);
            return main;
        }

        public static void Disconnect(Chunk chunk, IEnumerable<Block> blocks)
        {
            var groups = SplitToGroups(chunk, blocks);
            var sockets = GetSockets(groups, blocks);
            var (main, rest) = groups.Append(blocks).SeparateMainGroup(c => c);
            chunk.Disconnect(sockets, rest);
        }

        private static IEnumerable<Chunk> GetChunksFromSocketPairs(IEnumerable<SocketPair> socketPairs)
        {
            var chunks = new HashSet<Chunk>();
            foreach (var (thisSocket, otherSocket) in socketPairs)
            {
                var thisChunk = thisSocket.Block.Chunk;
                var otherChunk = otherSocket.Block.Chunk;
                chunks.Add(thisChunk);
                chunks.Add(otherChunk);
            }

            return chunks;
        }

        private static IEnumerable<Socket> GetSockets(IEnumerable<IEnumerable<Block>> groups, IEnumerable<Block> detached)
        {
            var detachedSet = detached.ToSet();
            foreach (var group in groups)
            {
                foreach (var block in group)
                {
                    foreach (var socket in block.Sockets)
                    {
                        if (socket.ConnectedSocket != null)
                        {
                            if (detachedSet.Contains(socket.ConnectedSocket.Block))
                            {
                                yield return socket;
                            }
                        }
                    }
                }
            }
        }

        private static (T main, IEnumerable<T> rest) SeparateMainGroup<T>(this IEnumerable<T> groups, Func<T, IEnumerable<Block>> selector)
        {
            var set = groups.ToSet();
            var selected = set.FirstOrDefault(g => selector(g).Any(b => b.IsAnchored)) ?? set.MaxBy(g => selector(g).Count());
            set.Remove(selected);
            return (selected, set);
        }

        private static IEnumerable<IEnumerable<Block>> SplitToGroups(Chunk chunk, IEnumerable<Block> blocks)
        {
            var all = chunk.Blocks.First().GetAllConnectedBlocks().ToList();
            var rest = all.Except(blocks).ToList();

            var groups = new List<ISet<Block>>();
            while (rest.Any())
            {
                var first = rest.First();
                var groupA = first.GetAllConnectedBlocks(blocks);
                rest = rest.Except(groupA).ToList();
                groups.Add(groupA);
            }

            return groups;
        }
    }
}