using BepInEx;
using BepInEx.Logging;
using BossNotifierFikaFix.Patches;
using HarmonyLib;

namespace BossNotifierFikaFix
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    [BepInDependency("Mattexe.BossNotifier", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class PluginCore : BaseUnityPlugin
    {
        internal static ManualLogSource FixLogger { get; private set; }

        private Harmony _harmony;

        private void Awake()
        {
            FixLogger = Logger;
            _harmony = new Harmony(PluginInfo.GUID);
            BossNotifierCatalogPatches.Apply(_harmony);
            BossCatalogSync.Initialize(Logger);
            Logger.LogInfo($"{PluginInfo.NAME} v{PluginInfo.VERSION} loaded");
        }

        private void OnDestroy()
        {
            BossCatalogSync.Shutdown();
        }
    }
}
