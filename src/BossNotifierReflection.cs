using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BossNotifier;
using Comfort.Common;
using EFT;

namespace BossNotifierFikaFix
{
    internal static class BossNotifierReflection
    {
        private static readonly Type BossNotifierMonoType = typeof(BossNotifierPlugin).Assembly.GetType("BossNotifier.BossNotifierMono", true);
        private static readonly Type BossLocationSpawnPatchType = typeof(BossNotifierPlugin).Assembly.GetType("BossNotifier.BossLocationSpawnPatch", true);
        private static readonly Type BotBossPatchType = typeof(BossNotifierPlugin).Assembly.GetType("BossNotifier.BotBossPatch", true);

        private static readonly FieldInfo BossesInRaidField = BossLocationSpawnPatchType.GetField("bossesInRaid", BindingFlags.Public | BindingFlags.Static);
        private static readonly FieldInfo SpawnedBossesField = BotBossPatchType.GetField("spawnedBosses", BindingFlags.Public | BindingFlags.Static);
        private static readonly FieldInfo DeadBossesField = BotBossPatchType.GetField("deadBosses", BindingFlags.Public | BindingFlags.Static);
        private static readonly FieldInfo InstanceField = BossNotifierMonoType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo GenerateBossNotificationsMethod = BossNotifierMonoType.GetMethod("GenerateBossNotifications", BindingFlags.Public | BindingFlags.Instance);

        internal static Dictionary<string, string> GetBossesInRaid()
        {
            return BossesInRaidField?.GetValue(null) as Dictionary<string, string>;
        }

        internal static HashSet<string> GetSpawnedBosses()
        {
            return SpawnedBossesField?.GetValue(null) as HashSet<string>;
        }

        internal static void RefreshNotifications()
        {
            var instance = InstanceField?.GetValue(null);
            if (instance != null && GenerateBossNotificationsMethod != null)
            {
                GenerateBossNotificationsMethod.Invoke(instance, null);
            }
        }

        internal static void EnsureLocalBossCatalog()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                return;
            }

            var bossesInRaid = GetBossesInRaid();
            var spawnedBosses = GetSpawnedBosses();
            if (bossesInRaid == null || spawnedBosses == null)
            {
                return;
            }

            ScanAliveBossesInto(bossesInRaid, spawnedBosses);

            foreach (var bossName in spawnedBosses)
            {
                if (!bossesInRaid.ContainsKey(bossName))
                {
                    bossesInRaid[bossName] = string.Empty;
                }
            }
        }

        internal static void MergeCatalog(Dictionary<string, string> incoming)
        {
            if (incoming == null || incoming.Count == 0)
            {
                return;
            }

            var bossesInRaid = GetBossesInRaid();
            var spawnedBosses = GetSpawnedBosses();
            if (bossesInRaid == null || spawnedBosses == null)
            {
                return;
            }

            foreach (var pair in incoming)
            {
                if (bossesInRaid.TryGetValue(pair.Key, out var existing))
                {
                    if (string.IsNullOrEmpty(existing) && !string.IsNullOrEmpty(pair.Value))
                    {
                        bossesInRaid[pair.Key] = pair.Value;
                    }
                }
                else
                {
                    bossesInRaid[pair.Key] = pair.Value ?? string.Empty;
                }

                spawnedBosses.Add(pair.Key);
            }

            EnsureLocalBossCatalog();
            RefreshNotifications();
        }

        internal static Dictionary<string, string> CopyCatalog()
        {
            var copy = new Dictionary<string, string>();
            var bossesInRaid = GetBossesInRaid();
            if (bossesInRaid == null)
            {
                return copy;
            }

            foreach (var pair in bossesInRaid)
            {
                copy[pair.Key] = pair.Value ?? string.Empty;
            }

            EnsureLocalBossCatalog();
            foreach (var pair in bossesInRaid)
            {
                if (!copy.ContainsKey(pair.Key))
                {
                    copy[pair.Key] = pair.Value ?? string.Empty;
                }
            }

            return copy;
        }

        internal static void MarkBossSpawned(string bossName)
        {
            if (string.IsNullOrEmpty(bossName))
            {
                return;
            }

            var bossesInRaid = GetBossesInRaid();
            var spawnedBosses = GetSpawnedBosses();
            if (bossesInRaid == null || spawnedBosses == null)
            {
                return;
            }

            spawnedBosses.Add(bossName);
            if (!bossesInRaid.ContainsKey(bossName))
            {
                bossesInRaid[bossName] = string.Empty;
            }
        }

        internal static Type GetBossNotifierMonoType() => BossNotifierMonoType;
        internal static Type GetBotBossPatchType() => BotBossPatchType;

        private static void ScanAliveBossesInto(Dictionary<string, string> bossesInRaid, HashSet<string> spawnedBosses)
        {
            var players = Singleton<GameWorld>.Instance?.AllAlivePlayersList;
            if (players == null)
            {
                return;
            }

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || player.IsYourPlayer)
                {
                    continue;
                }

                var role = player.Profile?.Info?.Settings?.Role;
                if (!role.HasValue || !BossNotifierPlugin.IsBoss(role.Value))
                {
                    continue;
                }

                var bossName = BossNotifierPlugin.GetBossName(role.Value);
                if (string.IsNullOrEmpty(bossName))
                {
                    continue;
                }

                if (!bossesInRaid.ContainsKey(bossName))
                {
                    bossesInRaid[bossName] = string.Empty;
                }

                spawnedBosses.Add(bossName);
            }
        }
    }
}
