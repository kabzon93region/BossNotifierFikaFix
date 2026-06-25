using BossNotifier;
using EFT;
using Fika.Core.Main.Utils;
using HarmonyLib;

namespace BossNotifierFikaFix.Patches
{
    internal static class BossNotifierCatalogPatches
    {
        internal static void Apply(Harmony harmony)
        {
            var monoType = BossNotifierReflection.GetBossNotifierMonoType();
            var botBossPatchType = BossNotifierReflection.GetBotBossPatchType();

            harmony.Patch(
                AccessTools.Method(monoType, "GenerateBossNotifications"),
                prefix: new HarmonyMethod(typeof(BossNotifierCatalogPatches), nameof(GenerateBossNotificationsPrefix)));

            harmony.Patch(
                AccessTools.Method(monoType, "Start"),
                postfix: new HarmonyMethod(typeof(BossNotifierCatalogPatches), nameof(BossNotifierMonoStartPostfix)));

            harmony.Patch(
                AccessTools.Method(botBossPatchType, "PatchPostfix"),
                postfix: new HarmonyMethod(typeof(BossNotifierCatalogPatches), nameof(BotBossSpawnPostfix)));
        }

        private static void GenerateBossNotificationsPrefix()
        {
            BossNotifierReflection.EnsureLocalBossCatalog();
        }

        private static void BossNotifierMonoStartPostfix()
        {
            if (FikaBackendUtils.IsServer)
            {
                BossCatalogSync.ScheduleHostBroadcast(2f);
            }
            else
            {
                BossNotifierReflection.EnsureLocalBossCatalog();
                BossNotifierReflection.RefreshNotifications();
            }
        }

        private static void BotBossSpawnPostfix(object __instance)
        {
            if (!FikaBackendUtils.IsServer || __instance == null)
            {
                return;
            }

            var ownerProp = __instance.GetType().GetProperty("Owner");
            var owner = ownerProp?.GetValue(__instance);
            var profileProp = owner?.GetType().GetProperty("Profile");
            var profile = profileProp?.GetValue(owner);
            var infoProp = profile?.GetType().GetProperty("Info");
            var info = infoProp?.GetValue(profile);
            var settingsProp = info?.GetType().GetProperty("Settings");
            var settings = settingsProp?.GetValue(info);
            var roleProp = settings?.GetType().GetProperty("Role");
            var role = roleProp?.GetValue(settings);

            if (role is WildSpawnType spawnType)
            {
                var bossName = BossNotifierPlugin.GetBossName(spawnType);
                if (!string.IsNullOrEmpty(bossName))
                {
                    BossCatalogSync.BroadcastBossSpawned(bossName);
                    BossCatalogSync.BroadcastCatalogNow();
                }
            }
        }
    }
}
