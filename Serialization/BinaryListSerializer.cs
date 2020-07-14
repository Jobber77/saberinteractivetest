using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Serialization
{
    public class BinaryListSerializer : IListSerializer
    {
        private Encoding TextEncoding => Encoding.Unicode;

        public async Task Serialize(ListNode head, Stream s)
        {
            s.Seek(0, SeekOrigin.Begin);
            var currentId = 0;
            var currentNode = head;
            var nodeIdToRandomNode = new Dictionary<int, ListNode>();
            var nodeToIdMap = new Dictionary<ListNode, int>();
            var buffer = new List<byte>();
            while (currentNode != null)
            {
                SaveNodeInMappings(currentId, currentNode, nodeIdToRandomNode, nodeToIdMap);
                PushNodeBytesToBuffer(currentNode, buffer);
                currentId++;
                currentNode = currentNode.Next;
            }

            var nodesCountBytes = BitConverter.GetBytes(currentId);
            await s.WriteAsync(nodesCountBytes);
            await s.WriteAsync(buffer.ToArray());
            await WriteNodeToRandomNodeMapping(s, nodeIdToRandomNode, nodeToIdMap);
        }

        private static void SaveNodeInMappings(int currentId, ListNode currentNode, 
            Dictionary<int, ListNode> nodeIdToRandomNode, Dictionary<ListNode, int> nodeToIdMap)
        {
            nodeToIdMap[currentNode] = currentId;
            if (currentNode.Random == null)
                return;
            nodeIdToRandomNode[currentId] = currentNode.Random;
        }

        private void PushNodeBytesToBuffer(ListNode currentNode, List<byte> buffer)
        {
            var nodeData = TextEncoding.GetBytes(currentNode.Data);
            var nodeDataLength = nodeData.Length;
            var nodeDataLengthBytes = BitConverter.GetBytes(nodeDataLength);
            buffer.AddRange(nodeDataLengthBytes);
            buffer.AddRange(nodeData);
        }

        private static async Task WriteNodeToRandomNodeMapping(Stream s, Dictionary<int, ListNode> nodeIdToRandomNode,
            Dictionary<ListNode, int> nodeToIdMap)
        {
            foreach (var random in nodeIdToRandomNode)
            {
                var nodeIdBytes = BitConverter.GetBytes(random.Key);
                var randomNodeId = BitConverter.GetBytes(nodeToIdMap[random.Value]);
                await s.WriteAsync(nodeIdBytes);
                await s.WriteAsync(randomNodeId);
            }
        }

        public async Task<ListNode> Deserialize(Stream s)
        {
            try
            {
                return await DeserializeInternal(s);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Stream contains invalid data", ex);
            }
        }

        private async Task<ListNode> DeserializeInternal(Stream s)
        {
            s.Seek(0, SeekOrigin.Begin);
            var nodesCountBytes = new byte[sizeof(int)];
            await s.ReadAsync(nodesCountBytes, 0, sizeof(int));
            var nodesCount = BitConverter.ToInt32(nodesCountBytes);
            var indexToNodesMap = new Dictionary<int, ListNode>();

            for (var index = 0; index < nodesCount; index++)
            {
                var node = await GetNextNode(s);
                indexToNodesMap[index] = node;
                if (index < 1)
                    continue;
                node.Previous = indexToNodesMap[index - 1];
                indexToNodesMap[index - 1].Next = node;
            }

            await SetRandomNodes(s, indexToNodesMap);

            return indexToNodesMap[0];
        }

        private static async Task SetRandomNodes(Stream s, Dictionary<int, ListNode> indexToNodesMap)
        {
            while (s.Position < s.Length)
            {
                var nodeIndexBytes = new byte[sizeof(int)];
                await s.ReadAsync(nodeIndexBytes, 0, sizeof(int));
                var randomNodeIndexBytes = new byte[sizeof(int)];
                await s.ReadAsync(randomNodeIndexBytes, 0, sizeof(int));
                var nodeId = BitConverter.ToInt32(nodeIndexBytes);
                var randomNodeId = BitConverter.ToInt32(randomNodeIndexBytes);
                indexToNodesMap[nodeId].Random = indexToNodesMap[randomNodeId];
            }
        }

        private async Task<ListNode> GetNextNode(Stream s)
        {
            var stringSizeBytes = new byte[sizeof(int)];
            await s.ReadAsync(stringSizeBytes, 0, sizeof(int));
            var stringSize = BitConverter.ToInt32(stringSizeBytes);
            var stringBytes = new byte[stringSize];
            await s.ReadAsync(stringBytes, 0, stringSize);
            var data = TextEncoding.GetString(stringBytes);

            return new ListNode { Data = data };
        }

        // We have no truly async actions here, so leave this method synchronious.
        // Wrapping this code in something like await Task.Run() will add overhead on task management.
        // I would leave this method signature without tasks. 
        // btw: It feels like this method not belongs to IListSerializer interface at all.
        public Task<ListNode> DeepCopy(ListNode head)
        {
            var sourceToCopyMap = new Dictionary<ListNode, ListNode>();
            var copyHead = GetOrCreateNodeCopy(head, sourceToCopyMap);
            SetRandomProperty(head, copyHead, sourceToCopyMap);

            var currentSourceNode = head;
            var currentCopyNode = copyHead;
            while (currentSourceNode != null)
            {
                currentSourceNode = currentSourceNode.Next;
                if (currentSourceNode == null)
                    break;

                var nextCopyNode = GetOrCreateNodeCopy(currentSourceNode, sourceToCopyMap);
                nextCopyNode.Previous = currentCopyNode;
                currentCopyNode.Next = nextCopyNode;
                SetRandomProperty(currentSourceNode, nextCopyNode, sourceToCopyMap);
                currentCopyNode = nextCopyNode;
            }
            return Task.FromResult(copyHead);
        }

        private ListNode GetOrCreateNodeCopy(ListNode sourceNode, Dictionary<ListNode, ListNode> map)
        {
            if (map.ContainsKey(sourceNode))
                return map[sourceNode];
            var copyNode = new ListNode { Data = sourceNode.Data };
            map[sourceNode] = copyNode;
            return copyNode;
        }

        private void SetRandomProperty(ListNode currentSourceNode, ListNode currentCopyNode,
            Dictionary<ListNode, ListNode> sourceToCopyMap)
        {
            var randomNode = currentSourceNode.Random;
            if (randomNode == null)
                return;
            var copyRandom = GetOrCreateNodeCopy(randomNode, sourceToCopyMap);
            currentCopyNode.Random = copyRandom;
        }
    }
}
