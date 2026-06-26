# Publish to GitHub — BossNotifier Fika Fix

**Статус:** `ready`  
**GitHub:** Release + zip  
**Версия:** `1.0.0`  
**Deployment:** `(headless_all)`

## 1. Подготовка (уже сделано этим скриптом)

Папка: `github-repos/BossNotifierFikaFix/`

## 2. Создать репозиторий и запушить

```powershell
cd github-repos/BossNotifierFikaFix
git init
git add .
git commit -m "Source backup BossNotifier Fika Fix v1.0.0"
git branch -M main
git remote add origin https://github.com/kabzon93region/BossNotifierFikaFix.git
git push -u origin main
```

Или автоматически:

```powershell
python CURSORAIMODING/tools/publish/publish_github_release.py BossNotifierFikaFix --create-repo
```

## 3. GitHub Release

Прикрепить zip (только игровые файлы, без INSTALL.md):

`\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\BossNotifierFikaFix_(headless_all)_v1.0.0_2026-06-26.zip`

```powershell
gh release create v1.0.0 "\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\BossNotifierFikaFix_(headless_all)_v1.0.0_2026-06-26.zip" ^
  --title "BossNotifier Fika Fix v1.0.0" ^
  --notes-file CHANGELOG.md
```

## Описание репозитория (suggested)

Синхронизация каталога BossNotifier в headless-coop.

SPT 4.0 + Fika 2.3 headless stack. Deployment: `(headless_all)`.
