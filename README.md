# BossNotifier Fika Fix

Fix для **BossNotifier** + **Fika** / **headless** coop.

**GUID:** `com.dematch.bossnotifier-fika`  
**Путь:** `BepInEx/plugins/BossNotifierFikaFix.dll`

## Зависимости

| Мод | Путь |
|-----|------|
| BossNotifier | `BepInEx/plugins/BossNotifier.dll` |
| BossNotifier Fika Sync | `BepInEx/plugins/BossNotifier.Fika.dll` |
| Fika | `BepInEx/plugins/Fika/` |

## Установка

Поставить на **headless-хост и каждого клиента** (тот же набор, что BossNotifier).

## Проверка

`BepInEx/LogOutput.log`:

```
BossNotifier Fika Fix v1.0.0 loaded
[BOSSNOTIFIER_FIKA] Packets registered
[BOSSNOTIFIER_FIKA] Received BossCatalogPacket entries=...
```

На клиенте в рейде **O** — список боссов, не «No Bosses Located».

## Сборка / релиз

```powershell
python pack_bossnotifier_fikafix.py
```
