using System;
using System.Collections.Generic;
using System.Linq;
using Blocks.Sockets;
using ElasticSea.Framework.Extensions;
using UnityEngine;

namespace Blocks.Builder
{
    public class BuildPreviewConnector : MonoBehaviour
    {
        private const float MaxSocketDistanceEpsilon = 1E-03f;

        public (AlignState state, IEnumerable<SocketPair> socketPairs) CheckForConnection(Chunk chunk)
        {
            var (position, rotation, connections, valid) = AlignShadow(chunk);
            if (valid == false)
            {
                // Unable to align chunks
                return (AlignState.NoConnections, new SocketPair[0]);
            }

            // Align the preview with possible connection
            transform.position = position;
            transform.rotation = rotation;

            var connectionCandidates = GetSocketConnectionCandidates(chunk).ToList();
            // if (IsColliding(connectionCandidates.Select(c => c.Other)))
            // {
            //     // New chunk would collide with itself
            //     return (AlignState.BlockingWithItself, new SocketPair[0]);
            // }

            var closeSocketPairs = FilterOutDistantSockets(connectionCandidates, MaxSocketDistanceEpsilon).ToArray();
            if (closeSocketPairs.Length < 2)
            {
                if (connections != closeSocketPairs.Length)
                {
                    // Sockets are too far away
                    return (AlignState.SocketsTooFar, connectionCandidates);
                }
            }

            // Valid outcomes
            if (connections < 2)
            {
                // Block will be aligned based on one socket only, that means there might be multiple ways to align the socket
                return (AlignState.OneSocketAligned, connectionCandidates);
            }
            else
            {
                return (AlignState.Aligned, connectionCandidates);
            }
        }

        private IEnumerable<SocketPair> FilterOutDistantSockets(IEnumerable<SocketPair> candidates, float maxDistanceThreashold)
        {
            return candidates.Where(socketPair =>
            {
                var thisPosition = socketPair.This.Position;
                var otherPosition = socketPair.Other.Position;
                var socketDistance = otherPosition.Distance(thisPosition);
                return socketDistance < maxDistanceThreashold;
            });
        }

        private IEnumerable<SocketPair> GetSocketConnectionCandidates(Chunk owner)
        {
            // IEnumerable<Socket> realSockets = owner.Sockets;
            List<Socket> realSockets = owner.EmptySockets.ToList();

            // TODO: This might return null, check the preview object hierarchy structure
            IEnumerable<Socket> previewSockets = GetComponentInChildren<Chunk>().EmptySockets;

            // TODO: This will cause performance issue when called frequently
            return previewSockets
                .Select(s => new SocketPair { This = s, Other = s.GetSocketCandidate() })
                .Where(sp => sp.Other != null)
                .Where(sp => realSockets.Contains(sp.Other) == false);
        }

        private bool IsColliding(IEnumerable<Socket> connectionCandidates)
        {
            var chunkSnapCandidates = connectionCandidates
                .Select(o => o.Block.Chunk)
                .ToSet();

            for (var i = 0; i < colliding.Count; i++)
            {
                var collidingChunk = colliding[i].GetComponentInParent<Chunk>();
                if (collidingChunk)
                {
                    if (chunkSnapCandidates.Contains(collidingChunk))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void FixedUpdate()
        {
            colliding = collidersOverllaping.ToList();
            collidersOverllaping.Clear();
        }

        private HashSet<Collider> collidersOverllaping = new HashSet<Collider>();

        private void OnTriggerStay(Collider other)
        {
            collidersOverllaping.Add(other);
        }

        private List<Collider> colliding = new List<Collider>();

        public (Vector3 position, Quaternion rotation, int connections, bool valid) AlignShadow(Chunk chunkSource)
        {
            SocketPair[] connections = chunkSource.GetConnections().ToArray();

            // var connections = chunkSource.GetConnections();

            // TODO refactor + test
            // connections = FilterOutCollinear(connections);
            // connections = FilterOutClose(connections);

            // Chose two closes connections and choose origin and alignment.
            // If only one connection is available use that one.
            if (connections.Length == 0)
            {
                return (default, default, 0, false);
            }

            if (connections.Length == 1)
            {
                var thisSocket = connections[0].This;
                var otherSocket = connections[0].Other;

                var (position1, rotation1) = AlignShadowSingle(thisSocket, otherSocket, chunkSource.transform);
                return (position1, rotation1, 1, true);
            }

            var thisSocketA = connections[0].This;
            var otherSocketA = connections[0].Other;
            var thisSocketB = connections[1].This;
            var otherSocketB = connections[1].Other;

            var (position, rotation) = AlignShadow(thisSocketA, thisSocketB, otherSocketA, otherSocketB, chunkSource.transform);
            return (position, rotation, connections.Length, true);
        }

        private static (Transform thisSocket, Transform otherSocket)[] FilterOutCollinear(
            (Transform thisSocket, Transform otherSocket)[] connections)
        {
            var output = new List<(Transform thisSocket, Transform otherSocket)>();
            var marked = new bool[connections.Length];
            Debug.Log(connections.Length);
            for (var i = 0; i < connections.Length; i++)
            {
                var isCollinear = false;
                for (var j = 0; j < connections.Length; j++)
                {
                    if (connections[i] != connections[j])
                    {
                        if (marked[i] == false && marked[j] == false)
                        {
                            // Chose two closes connections and choose origin and alignment.
                            var thisSocketA = connections[i].thisSocket;
                            var otherSocketA = connections[i].otherSocket;
                            var thisSocketB = connections[j].thisSocket;
                            var otherSocketB = connections[j].otherSocket;

                            var directionA = otherSocketA.up;
                            var directionB = otherSocketA.position - otherSocketB.position;

                            // Check if the directions are collinear
                            if (Math.Abs(directionA.Dot(directionB)) > 0.0001f)
                            {
                                marked[i] = true;
                                isCollinear = true;
                            }
                        }
                    }
                }

                if (isCollinear == false)
                {
                    output.Add(connections[i]);
                }
            }

            return output.ToArray();
        }

        private (Vector3 position, Quaternion rotation) AlignShadowSingle(Socket thisA, Socket otherA, Transform blockSource)
        {
            var possibleDirs = new[]
            {
                (0, otherA.Forward().normalized),
                (1, -otherA.Forward().normalized),
                (2, otherA.Right().normalized),
                (3, -otherA.Right().normalized)
            };

            var closestDirection = possibleDirs.OrderByDescending(p => p.Item2.Angle(thisA.Right().normalized)).First().Item1;

            var thisDir = thisA.Right().normalized;
            var otherDir = otherA.Right().normalized;

            switch (closestDirection)
            {
                case 0:
                    otherDir = otherA.Forward().normalized;
                    break;
                case 1:
                    otherDir = -otherA.Forward().normalized;
                    break;
                case 2:
                    otherDir = otherA.Right().normalized;
                    break;
                case 3:
                    otherDir = -otherA.Right().normalized;
                    break;
            }

            // TODO Direction has to be inverted? Maybe its because the different default direction of female/male socket
            otherDir = -otherDir;

            return AlignShadow(thisA, thisDir, otherA, otherDir, blockSource);
        }

        private (Vector3 position, Quaternion rotation) AlignShadow(Socket thisA, Socket thisB, Socket otherA,
            Socket otherB, Transform blockSource)
        {
            var thisDir = (thisB.Position - thisA.Position).normalized;
            var otherDir = (otherB.Position - otherA.Position).normalized;

            return AlignShadow(thisA, thisDir, otherA, otherDir, blockSource);
            // return AlignShadow(thisA, thisDir, otherA, otherDir, blockSource);
        }

        private (Vector3 position, Quaternion rotation) AlignShadow(Socket thisA, Vector3 thisDir, Socket otherA,
            Vector3 otherDir, Transform blockSource)
        {
            var thisToOtherRotation = Quaternion.FromToRotation(thisDir, otherDir);

            // Correct for rotation along the direction (multiple valid states for resulting rotation)
            var correctedUpVector = thisToOtherRotation * -thisA.Up();
            var angle = Vector3.SignedAngle(correctedUpVector, otherA.Up(), otherDir);
            var correction = Quaternion.AngleAxis(angle, otherDir);

            var targetRotation = correction * thisToOtherRotation * blockSource.rotation;

            var blockSocketLocalPosition = blockSource.InverseTransformPoint(thisA.Position);
            var shadowSocketWorldPosition = transform.TransformPoint(blockSocketLocalPosition);
            var adjustedWorldPosition = shadowSocketWorldPosition - transform.position;
            var targetPosition = otherA.Position - adjustedWorldPosition;

            return (targetPosition, targetRotation);
        }
    
    }

    // NOTE: Quick and dirty helper class, this can be removed in the future update
    static class SocketHelper
    {
        public static Vector3 Forward(this Socket s)
        {
            return (s.Orientation * Vector3.forward).normalized;
        }

        public static Vector3 Right(this Socket s)
        {
            return (s.Orientation * Vector3.right).normalized;
        }

        public static Vector3 Up(this Socket s)
        {
            return (s.Orientation * Vector3.up).normalized;
        }
    }
}