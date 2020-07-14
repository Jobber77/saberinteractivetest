using FluentAssertions;
using Serialization;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using System.Diagnostics.CodeAnalysis;

namespace SerializationTests
{
    public class BinaryListSerializerTests
    {
        private BinaryListSerializer _serializer;

        public BinaryListSerializerTests()
        {
            _serializer = new BinaryListSerializer();
        }

        [Theory]
        [MemberData(nameof(ListNodeDataGenerator.GetDeepCopyTestData), MemberType = typeof(ListNodeDataGenerator))]
        public async Task DeepCopy_ResultNodeList_AreNotTheSameObjectsAsSource(ListNode head, int nodeCount)
        {
            var copy = await _serializer.DeepCopy(head);

            Assert.NotSame(head, copy);
            for (var i = 1; i < nodeCount; i++)
            {
                head = head.Next;
                copy = copy.Next;
                Assert.NotSame(head, copy);
            }
        }

        [Theory]
        [MemberData(nameof(ListNodeDataGenerator.GetDeepCopyTestData), MemberType = typeof(ListNodeDataGenerator))]
        public async Task DeepCopy_SetDataOnCopies_EqualToSourceNodes(ListNode head, int nodeCount)
        {
            var copy = await _serializer.DeepCopy(head);

            Assert.Equal(head.Data, copy.Data);
            for (var i = 1; i < nodeCount; i++)
            {
                head = head.Next;
                copy = copy.Next;
                Assert.Equal(head.Data, copy.Data);
            }
        }

        [Theory]
        [MemberData(nameof(ListNodeDataGenerator.GetDeepCopyTestData), MemberType = typeof(ListNodeDataGenerator))]
        public async Task DeepCopy_SetRandomOnCopies_ToSourceEquivalent(ListNode head, int nodeCount)
        {
            var copy = await _serializer.DeepCopy(head);

            Assert.Equal(head.Random?.Data, copy.Random?.Data);
            for (var i = 1; i < nodeCount; i++)
            {
                head = head.Next;
                copy = copy.Next;
                Assert.Equal(head.Random?.Data, copy.Random?.Data);
            }
        }

        [Theory]
        [MemberData(nameof(ListNodeDataGenerator.GetDeepCopyTestData), MemberType = typeof(ListNodeDataGenerator))]
        public async Task DeepCopy_SetPreviousOnCopies_ToSourceEquivalent(ListNode head, int nodeCount)
        {
            var copy = await _serializer.DeepCopy(head);
            ListNode tailCopy = copy;
            ListNode originalTail = head;

            for (var i = 1; i < nodeCount; i++)
            {
                originalTail = originalTail.Next;
                tailCopy = tailCopy.Next;
            }

            Assert.Equal(originalTail.Data, tailCopy.Data);
            for (var i = 1; i < nodeCount; i++)
            {
                originalTail = originalTail.Previous;
                tailCopy = tailCopy.Previous;
                Assert.Equal(originalTail.Data, tailCopy.Data);
            }
        }

        [Theory]
        [MemberData(nameof(ListNodeDataGenerator.GetSerializationTestData), MemberType = typeof(ListNodeDataGenerator))]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Reuse GetSerializationTestData arguments")]
        public async Task Serialize_UseExpectedBinaryFormat(ListNode head, byte[] expectedByteArray, int _)
        {
            using var stream = new MemoryStream();
            await _serializer.Serialize(head, stream);

            var serializedBytes = stream.ToArray();
            serializedBytes.Should().BeEquivalentTo(expectedByteArray, options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task Deserialize_WhenStreamContainNotValidData_ThrowArgumentException()
        {
            using var stream = new MemoryStream(new byte[10]);

            await Assert.ThrowsAsync<ArgumentException>(() => _serializer.Deserialize(stream));
        }

        [Theory]
        [MemberData(nameof(ListNodeDataGenerator.GetSerializationTestData), MemberType = typeof(ListNodeDataGenerator))]
        public async Task Deserialize_SetData(ListNode expectedNodes, byte[] incomingBytes, int nodeCount)
        {
            using var stream = new MemoryStream(incomingBytes);

            var nodes = await _serializer.Deserialize(stream);

            Assert.Equal(expectedNodes.Data, nodes.Data);
            for (var i = 1; i < nodeCount; i++)
            {
                expectedNodes = expectedNodes.Next;
                nodes = nodes.Next;
                Assert.Equal(expectedNodes.Data, nodes.Data);
            }
        }

        [Theory]
        [MemberData(nameof(ListNodeDataGenerator.GetSerializationTestData), MemberType = typeof(ListNodeDataGenerator))]
        public async Task Deserialize_SetRandom(ListNode expectedNodes, byte[] incomingBytes, int nodeCount)
        {
            using var stream = new MemoryStream(incomingBytes);

            var nodes = await _serializer.Deserialize(stream);

            Assert.Equal(expectedNodes.Random?.Data, nodes.Random?.Data);
            for (var i = 1; i < nodeCount; i++)
            {
                expectedNodes = expectedNodes.Next;
                nodes = nodes.Next;
                Assert.Equal(expectedNodes.Random?.Data, nodes.Random?.Data);
            }
        }

        [Theory]
        [MemberData(nameof(ListNodeDataGenerator.GetSerializationTestData), MemberType = typeof(ListNodeDataGenerator))]
        public async Task Deserialize_SetPrevious(ListNode expectedNodes, byte[] incomingBytes, int nodeCount)
        {
            using var stream = new MemoryStream(incomingBytes);

            var nodes = await _serializer.Deserialize(stream);

            ListNode tail = nodes;
            ListNode expectedTail = expectedNodes;

            for (var i = 1; i < nodeCount; i++)
            {
                expectedTail = expectedTail.Next;
                tail = tail.Next;
            }

            Assert.Equal(expectedTail.Data, tail.Data);
            for (var i = 1; i < nodeCount; i++)
            {
                expectedTail = expectedTail.Previous;
                tail = tail.Previous;
                Assert.Equal(expectedTail.Data, tail.Data);
            }
        }

    }
}
