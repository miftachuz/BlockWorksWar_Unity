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
            if (IsColliding(connectionCandidates.Select(c => c.Other)))
            {
                // New chunk would collide with itself
                return (AlignState.BlockingWithItself, new SocketPair[0]);
            }

            var closeSocketPairs = FilterOutDistantSockets(connectionCandidates, MaxSocketDistanceEpsilon).ToArray();
            if (closeSocketPairs.Length < 2)
            {
                if (connections != closeSocketPairs.Length)
                {
                    // Sockets are too far away
                    return (AlignState.SocketsTooFar, new SocketPair[0]);
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
                var thisPosition = socketPair.This.transform.position;
                var otherPosition = socketPair.Other.transform.position;
                var socketDistance = otherPosition.Distance(thisPosition);
                return socketDistance < maxDistanceThreashold;
            });
        }

        private IEnumerable<SocketPair> GetSocketConnectionCandidates(Chunk owner)
        {
            var realSockets = owner.GetComponentsInChildren<Socket>().ToSet();
            var previewSockets = GetComponentsInChildren<Socket>();

            return previewSockets
                .Select(socket => new SocketPair {This = socket, Other = socket.Trigger().FirstOrDefault()})
                .Where(socketPair => socketPair.Other != null)
                .Where(socketPair => realSockets.Contains(socketPair.Other) == false);
        }

        private bool IsColliding(IEnumerable<Socket> connectionCandidates)
        {
            var chunkSnapCandidates = connectionCandidates
                .Select(o => o.GetComponentInParent<Chunk>())
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
            var connections = chunkSource.GetConnections();

            // TODO refactor + test
            connections = FilterOutCollinear(connections);
            // connections = FilterOutClose(connections);

            // Chose two closes connections and choose origin and alignment.
            // If only one connection is available use that one.
            if (connections.Length == 0)
            {
                return (default, default, 0, false);
            }

            if (connections.Length == 1)
            {
                var thisSocket = connections[0].thisSocket;
                var otherSocket = connections[0].otherSocket;

                var (position1, rotation1) = AlignShadowSingle(thisSocket, otherSocket, chunkSource.transform);
                return (position1, rotation1, 1, true);
            }

            var thisSocketA = connections[0].thisSocket;
            var otherSocketA = connections[0].otherSocket;
            var thisSocketB = connections[1].thisSocket;
            var otherSocketB = connections[1].otherSocket;

            var (position, rotation) = AlignShadow(thisSocketA, thisSocketB, otherSocketA, otherSocketB, chunkSource.transform);
            return (position, rotation, connections.Length, true);
        }

        private static (Transform thisSocket, Transform otherSocket)[] FilterOutCollinear(
            (Transform thisSocket, Transform otherSocket)[] connections)
        {
            var output = new List<(Transform thisSocket, Transform otherSocket)>();
            var marked = new bool[connections.Length];
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

        private (Vector3 position, Quaternion rotation) AlignShadowSingle(Transform thisA, Transform otherA, Transform blockSource)
        {
            var possibleDirs = new[]
            {
                (0, otherA.forward.normalized),
                (1, -otherA.forward.normalized),
                (2, otherA.right.normalized),
                (3, -otherA.right.normalized)
            };

            var closestDirection = possibleDirs.OrderByDescending(p => p.Item2.Angle(thisA.right.normalized)).First().Item1;

            var thisDir = thisA.right.normalized;
            var otherDir = otherA.right.normalized;

            switch (closestDirection)
            {
                case 0:
                    otherDir = otherA.forward.normalized;
                    break;
                case 1:
                    otherDir = -otherA.forward.normalized;
                    break;
                case 2:
                    otherDir = otherA.right.normalized;
                    break;
                case 3:
                    otherDir = -otherA.right.normalized;
                    break;
            }

            // TODO Direction has to be inverted? Maybe its because the different default direction of female/male socket
            otherDir = -otherDir;

            return AlignShadow(thisA, thisDir, otherA, otherDir, blockSource);
        }

        private (Vector3 position, Quaternion rotation) AlignShadow(Transform thisA, Transform thisB, Transform otherA,
            Transform otherB, Transform blockSource)
        {
            var thisDir = (thisB.position - thisA.position).normalized;
            var otherDir = (otherB.position - otherA.position).normalized;

            return AlignShadow(thisA, thisDir, otherA, otherDir, blockSource);
        }

        private (Vector3 position, Quaternion rotation) AlignShadow(Transform thisA, Vector3 thisDir, Transform otherA,
            Vector3 otherDir, Transform blockSource)
        {
            var thisToOtherRotation = Quaternion.FromToRotation(thisDir, otherDir);

            // Correct for rotation along the direction (multiple valid states for resulting rotation)
            var correctedUpVector = thisToOtherRotation * -thisA.up;
            var angle = Vector3.SignedAngle(correctedUpVector, otherA.up, otherDir);
            var correction = Quaternion.AngleAxis(angle, otherDir);

            var targetRotation = correction * thisToOtherRotation * blockSource.rotation;

            var blockSocketLocalPosition = blockSource.InverseTransformPoint(thisA.position);
            var shadowSocketWorldPosition = transform.TransformPoint(blockSocketLocalPosition);
            var adjustedWorldPosition = shadowSocketWorldPosition - transform.position;
            var targetPosition = otherA.position - adjustedWorldPosition;

            return (targetPosition, targetRotation);
        }
    }
}