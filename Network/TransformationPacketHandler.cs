using DBZGoatLib.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DBZGoatLib.Network
{
    internal class TransformationPacketHandler : PacketHandler
    {
        public const byte SYNC_TRANFORMATIONS = 1;

        public TransformationPacketHandler(byte handlerType) : base(handlerType)
        {
        }

        public override void HandlePacket(BinaryReader reader, int fromWho)
        {
            switch (reader.ReadByte())
            {
                case SYNC_TRANFORMATIONS:
                    ReceiveFormChanges(reader, fromWho);
                    break;
            }
        }

        public void SendFormChanges(int toWho, int fromWho, int whichPlayer, int buffId, int duration)
        {
            ModPacket packet = GetPacket(SYNC_TRANFORMATIONS);

            packet.Write(whichPlayer);
            packet.Write(buffId);
            packet.Write(duration);
            packet.Send(toWho, fromWho);
        }

        public void ReceiveFormChanges(BinaryReader reader, int fromWho)
        {
            int whichPlayer = reader.ReadInt32();
            int buffID = reader.ReadInt32();
            int duration = reader.ReadInt32();
            if (Main.netMode == NetmodeID.Server)
            {
                SendFormChanges(-1, fromWho, whichPlayer, buffID, duration);
            }
            else
            {
                Player player = Main.player[whichPlayer];
                if (duration == 0)
                    TransformationHandler.EndTranformation(player, TransformationHandler.GetTransformation(buffID));
                else
                    TransformationHandler.Transform(player, TransformationHandler.GetTransformation(buffID));
            }
        }
    }
}
