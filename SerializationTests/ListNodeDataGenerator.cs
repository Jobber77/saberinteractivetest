using Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SerializationTests
{
    class ListNodeDataGenerator
    {
        private static List<ListNode> _testNodes = new List<ListNode>(4);
        private static string[] _testData = new string[] { "test1", "test2222", "testttttt3" };

        static ListNodeDataGenerator()
        {
            var listNodeHead1 = new ListNode { Data = _testData[0] };
            _testNodes.Add(listNodeHead1);

            var listNodeHead2 = new ListNode { Data = _testData[0] };
            var listNode1 = new ListNode { Data = _testData[1], Previous = listNodeHead2 };
            listNodeHead2.Next = listNode1;
            _testNodes.Add(listNodeHead2);

            var listNodeHead3 = new ListNode { Data = _testData[0] };
            var listNode2 = new ListNode { Data = _testData[1], Previous = listNodeHead3 };
            listNodeHead3.Next = listNode2;
            listNodeHead3.Random = listNode2;
            listNode2.Random = listNodeHead3;
            _testNodes.Add(listNodeHead3);

            var listNodeHead4 = new ListNode { Data = _testData[0] };
            var listNode3 = new ListNode { Data = _testData[1], Previous = listNodeHead4 };
            var listNode4 = new ListNode { Data = _testData[2], Previous = listNode3 };
            listNodeHead4.Next = listNode3;
            listNode3.Next = listNode4;
            listNode4.Random = listNode3;
            _testNodes.Add(listNodeHead4);
        }

        public static IEnumerable<object[]> GetDeepCopyTestData()
        {
            yield return new object[] { _testNodes[0], 1 };

            yield return new object[] { _testNodes[1], 2 };
            
            yield return new object[] { _testNodes[2], 2 };

            yield return new object[] { _testNodes[3], 3 };
        }

        public static IEnumerable<object[]> GetSerializationTestData()
        {
            var nodesBytes1 = new List<byte>();
            var nodeData1 = Encoding.Unicode.GetBytes(_testData[0]);
            var nodeDataLength1 = nodeData1.Length;
            var nodeDataLengthBytes1 = BitConverter.GetBytes(nodeDataLength1);
            nodesBytes1.AddRange(nodeDataLengthBytes1);
            nodesBytes1.AddRange(nodeData1);
            var expectedBytes1 = new List<byte>(BitConverter.GetBytes(1));
            expectedBytes1.AddRange(nodesBytes1);
            yield return new object[] { _testNodes[0], expectedBytes1.ToArray(), 1 };

            var nodesBytes2 = new List<byte>(nodesBytes1);
            var nodeData2 = Encoding.Unicode.GetBytes(_testData[1]);
            var nodeDataLength2 = nodeData2.Length;
            var nodeDataLengthBytes2 = BitConverter.GetBytes(nodeDataLength2);
            nodesBytes2.AddRange(nodeDataLengthBytes2);
            nodesBytes2.AddRange(nodeData2);
            var expectedBytes2 = new List<byte>(BitConverter.GetBytes(2));
            expectedBytes2.AddRange(nodesBytes2);
            yield return new object[] { _testNodes[1], expectedBytes2.ToArray(), 2 };

            var nodeBytes3 = new List<byte>(nodesBytes2);
            var nodeIdBytes1 = BitConverter.GetBytes(0);
            var randomNodeId1 = BitConverter.GetBytes(1);
            nodeBytes3.AddRange(nodeIdBytes1);
            nodeBytes3.AddRange(randomNodeId1);
            var nodeIdBytes2 = BitConverter.GetBytes(1);
            var randomNodeIdBytes2 = BitConverter.GetBytes(0);
            nodeBytes3.AddRange(nodeIdBytes2);
            nodeBytes3.AddRange(randomNodeIdBytes2);
            var expectedBytes3 = new List<byte>(BitConverter.GetBytes(2));
            expectedBytes3.AddRange(nodeBytes3);
            yield return new object[] { _testNodes[2], expectedBytes3.ToArray(), 2 };

            var nodeBytes4 = new List<byte>(nodesBytes2);
            var nodeData3 = Encoding.Unicode.GetBytes(_testData[2]);
            var nodeDataLength3 = nodeData3.Length;
            var nodeDataLengthBytes3 = BitConverter.GetBytes(nodeDataLength3);
            nodeBytes4.AddRange(nodeDataLengthBytes3);
            nodeBytes4.AddRange(nodeData3);
            var nodeIdBytes3 = BitConverter.GetBytes(2);
            var randomNodeIdBytes3 = BitConverter.GetBytes(1);
            nodeBytes4.AddRange(nodeIdBytes3);
            nodeBytes4.AddRange(randomNodeIdBytes3);
            var expectedBytes4 = new List<byte>(BitConverter.GetBytes(3));
            expectedBytes4.AddRange(nodeBytes4);
            yield return new object[] { _testNodes[3], expectedBytes4.ToArray(), 3 };
        }
    }
}