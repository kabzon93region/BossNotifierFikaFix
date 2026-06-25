# Changelog

## v1.0.0 (2026-06-22)

- Fika/headless: синхронизация каталога боссов host → client (`BossCatalogPacket`, `BossSpawnedPacket`)
- Клиент: fallback — боссы из живых игроков на карте, если `bossesInRaid` пуст (типично для Fika client)
- Harmony: дополняет `GenerateBossNotifications` перед построением списка уведомлений
- Исправляет «No Bosses Located» при нажатии O, когда босс на карте есть
- Зависимости: `Mattexe.BossNotifier`, `com.fika.core` (soft)
