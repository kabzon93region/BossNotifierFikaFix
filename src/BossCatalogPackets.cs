using System.Collections.Generic;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace BossNotifierFikaFix
{
    internal struct BossCatalogPacket : INetSerializable
    {
        public Dictionary<string, string> BossesInRaid;

        public void Serialize(NetDataWriter writer)
        {
            var bosses = BossesInRaid ?? new Dictionary<string, string>();
            writer.Put(bosses.Count);
            foreach (var pair in bosses)
            {
                writer.Put(pair.Key ?? string.Empty);
                writer.Put(pair.Value ?? string.Empty);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            int count = reader.GetInt();
            BossesInRaid = new Dictionary<string, string>(count);
            for (int i = 0; i < count; i++)
            {
                BossesInRaid[reader.GetString()] = reader.GetString();
            }
        }
    }

    internal struct BossSpawnedPacket : INetSerializable
    {
        public string BossName;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(BossName ?? string.Empty);
        }

        public void Deserialize(NetDataReader reader)
        {
            BossName = reader.GetString();
        }
    }
}
