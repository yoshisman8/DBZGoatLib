using System.IO;

namespace DBZGoatLib.Network {

    public class NetworkHelper {
        public const byte TRANSFORMATION_HANDLER = 1;

        internal static readonly TransformationPacketHandler transSync = new(TRANSFORMATION_HANDLER);

        public static void HandlePacket(BinaryReader r, int fromWho) {
            switch (r.ReadByte()) {
                case TRANSFORMATION_HANDLER:
                    transSync.HandlePacket(r, fromWho);
                    break;
            }
        }
    }
}