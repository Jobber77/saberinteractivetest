# Test task for Saber Interactive
Made assumptions:
- Nothing mentioned in task about human readable format for serialization. Taking in consideration requirement for high performance, I choose binary serialization.
- I expect the the stream to be passed into Task<ListNode> Deserialize(Steam s) contain the data serialized by corresponding Task Serialize(ListNode head, Stream s) to make it work.
- Few edge-case checks in methods are skipped according to task Notes.
