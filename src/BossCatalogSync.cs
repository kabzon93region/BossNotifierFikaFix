using System.Collections;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using UnityEngine;

namespace BossNotifierFikaFix
{
    internal static class BossCatalogSync
    {
        private static ManualLogSource _logger;
        private static IFikaNetworkManager _networkManager;
        private static bool _packetsRegistered;
        private static MonoBehaviour _runner;

        internal static void Initialize(ManualLogSource logger)
        {
            _logger = logger;
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerDestroyedEvent>(OnNetworkManagerDestroyed);
            TryRegisterPackets();
        }

        internal static void Shutdown()
        {
            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerDestroyedEvent>(OnNetworkManagerDestroyed);
            _networkManager = null;
            _packetsRegistered = false;
            if (_runner != null)
            {
                Object.Destroy(_runner);
                _runner = null;
            }
        }

        internal static void ScheduleHostBroadcast(float delaySeconds = 2f)
        {
            if (!FikaBackendUtils.IsServer)
            {
                return;
            }

            EnsureRunner();
            _runner.StopAllCoroutines();
            _runner.StartCoroutine(BroadcastAfterDelay(delaySeconds));
        }

        internal static void BroadcastCatalogNow()
        {
            if (!FikaBackendUtils.IsServer || !TryRegisterPackets() || _networkManager == null)
            {
                return;
            }

            var packet = new BossCatalogPacket
            {
                BossesInRaid = BossNotifierReflection.CopyCatalog()
            };

            if (packet.BossesInRaid.Count == 0)
            {
                return;
            }

            _networkManager.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
            _logger?.LogInfo($"[BOSSNOTIFIER_FIKA] Sent BossCatalogPacket entries={packet.BossesInRaid.Count}");
        }

        internal static void BroadcastBossSpawned(string bossName)
        {
            if (!FikaBackendUtils.IsServer || string.IsNullOrEmpty(bossName) || !TryRegisterPackets() || _networkManager == null)
            {
                return;
            }

            var packet = new BossSpawnedPacket { BossName = bossName };
            _networkManager.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
            _logger?.LogInfo($"[BOSSNOTIFIER_FIKA] Sent BossSpawnedPacket boss={bossName}");
        }

        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent evt)
        {
            _networkManager = evt.Manager;
            TryRegisterPackets();
        }

        private static void OnNetworkManagerDestroyed(FikaNetworkManagerDestroyedEvent evt)
        {
            _networkManager = null;
            _packetsRegistered = false;
        }

        private static bool TryRegisterPackets()
        {
            if (_packetsRegistered)
            {
                return _networkManager != null;
            }

            if (_networkManager == null)
            {
                _networkManager = Singleton<FikaServer>.Instance as IFikaNetworkManager
                    ?? Singleton<FikaClient>.Instance as IFikaNetworkManager;
            }

            if (_networkManager == null)
            {
                return false;
            }

            _networkManager.RegisterPacket<BossCatalogPacket>(OnCatalogReceived);
            _networkManager.RegisterPacket<BossSpawnedPacket>(OnBossSpawnedReceived);
            _packetsRegistered = true;
            _logger?.LogInfo("[BOSSNOTIFIER_FIKA] Packets registered");
            return true;
        }

        private static void OnCatalogReceived(BossCatalogPacket packet)
        {
            if (FikaBackendUtils.IsServer)
            {
                return;
            }

            _logger?.LogInfo($"[BOSSNOTIFIER_FIKA] Received BossCatalogPacket entries={packet.BossesInRaid?.Count ?? 0}");
            BossNotifierReflection.MergeCatalog(packet.BossesInRaid);
        }

        private static void OnBossSpawnedReceived(BossSpawnedPacket packet)
        {
            if (FikaBackendUtils.IsServer || string.IsNullOrEmpty(packet.BossName))
            {
                return;
            }

            BossNotifierReflection.MarkBossSpawned(packet.BossName);
            BossNotifierReflection.RefreshNotifications();
            _logger?.LogInfo($"[BOSSNOTIFIER_FIKA] Boss spawned sync: {packet.BossName}");
        }

        private static IEnumerator BroadcastAfterDelay(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            BroadcastCatalogNow();
        }

        private static void EnsureRunner()
        {
            if (_runner != null)
            {
                return;
            }

            if (!Singleton<GameWorld>.Instantiated)
            {
                return;
            }

            var go = new GameObject("BossNotifierFikaFixRunner");
            Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<BossCatalogRunner>();
        }

        private sealed class BossCatalogRunner : MonoBehaviour
        {
        }
    }
}
