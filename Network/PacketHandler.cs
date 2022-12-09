using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace DBZGoatLib.Network
{
    internal abstract class PacketHandler
    {
        internal byte HandlerType { get; set; }

        public abstract void HandlePacket(BinaryReader reader, int fromWho);

        protected PacketHandler(byte handlerType) => this.HandlerType = handlerType;

        protected ModPacket GetPacket(byte packetType)
        {
            ModPacket packet;
            packet = DBZGoatLib.Instance.GetPacket(256);
            packet.Write(HandlerType);
            packet.Write(packetType);
            return packet;
        }
    }
}
