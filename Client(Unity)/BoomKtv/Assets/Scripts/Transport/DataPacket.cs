using ICelery.Network.Data;
using UnityEngine;

namespace Transport
{
    public class DataPacket : ByteBuffer
    {
        public Opcode Opcode { get; private set; }

        private DataPacket(int capacity = 0) : base(capacity)
        {
        }

        private DataPacket(byte[] bytes) : base(bytes)
        {
        }

        public static DataPacket Allocate(Opcode opcode)
        {
            var packet = new DataPacket {Opcode = opcode};
            packet.WriteShort((short) opcode);
            return packet;
        }

        public static DataPacket Parse(byte[] bytes)
        {
            var packet = new DataPacket(bytes);
            packet.Opcode = (Opcode) packet.ReadShort();
            return packet;
        }

        public void Flush()
        {
            var data = ToArray();
            Client.Instance.Send(data, 0, data.Length);
            Debug.Log($"Send Opcode={Opcode}, Length={data.Length}");
        }
    }
}