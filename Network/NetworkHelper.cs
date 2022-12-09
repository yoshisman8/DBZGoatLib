using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBZGoatLib.Network;

namespace DBZGoatLib.Network
{
    public class NetworkHelper
    {
        public const byte TRANSFORMATION_HANDLER = 1;

        internal static TransformationPacketHandler transSync = new TransformationPacketHandler(TRANSFORMATION_HANDLER);


        public static void HandlePacket(BinaryReader r, int fromWho)
        {
            switch (r.ReadByte())
            {
                case TRANSFORMATION_HANDLER:
                    transSync.HandlePacket(r, fromWho);
                    break;
            }
        }
    }
}
