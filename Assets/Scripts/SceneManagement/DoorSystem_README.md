# Door System - Stabil Kapı Sistemi 🚪

En basit ve güvenilir kapı sistemi. SceneManager tarafından otomatik olarak yönetilir.

## 🎮 Nasıl Çalışır

1. **DoorTrigger** → Kapının yanına koy, W/↑ ile sahne geçişi
2. **DoorSpawn** → Hedef sahnede kapının önüne koy, otomatik spawn
3. **SceneManager** → Her şeyi otomatik halleder

## 📋 Kurulum (Süper Basit!)

### 1. Ana Sahne (Kapı Var)
```
DoorTrigger
├── BoxCollider2D (Is Trigger ✓)
├── DoorTrigger Script
│   └── Target Scene Index: [Hedef sahne numarası]
└── InteractionPrompt (opsiyonel)
    └── W tuşu sprite'ı
```

### 2. Hedef Sahne (İç Mekan)
```
DoorSpawn
└── Transform pozisyonu kapının önünde
```

## ⚙️ Ayarlar

### DoorTrigger
- **Target Scene Index**: Gidilecek sahnenin build index'i
- **Player Tag**: "Player" (varsayılan)
- **Interaction Prompt**: W tuşu sprite'ı (opsiyonel)
- **Door Open Sound**: Kapı sesi (opsiyonel)

### DoorSpawn
- Sadece **isim**: "DoorSpawn" olmalı
- **Pozisyon**: Kapının önünde
- Script gerekmez!

## 🔧 Sistem Özellikleri

✅ **Otomatik Kamera Takibi** - Spawn sonrası kamera oyuncuyu bulur  
✅ **IInteractable Uyumlu** - Mevcut interaction sistemi  
✅ **Physics Sıfırlama** - Spawn sonrası velocity temizlenir  
✅ **Hata Toleransı** - DoorSpawn yoksa varsayılan spawn  
✅ **Debug Bilgileri** - Console'da detaylı loglar  

## 📁 Dosya Yapısı

```
Scene_MainArea/
├── DoorTrigger_House
│   ├── Target Scene Index: 2
│   └── InteractionPrompt
└── Other objects...

Scene_HouseInterior/
├── DoorSpawn (kapının önünde)
├── PlayerSpawnPoint (varsayılan spawn)
└── Other objects...
```

## 🎯 Örnek Kullanım

### Ev Sistemi
1. **Dış Sahne**: DoorTrigger → Target Scene Index: 2
2. **Ev Sahnesi**: "DoorSpawn" objesi kapının önünde

### Mağara Sistemi  
1. **Açık Alan**: DoorTrigger → Target Scene Index: 3
2. **Mağara**: "DoorSpawn" objesi girişte

## 🔄 Akış Diagramı

```
[Ana Sahne] 
    ↓ 
[Kapıya Yaklaş] 
    ↓ 
[W Tuşuna Bas] 
    ↓ 
[SceneManager: Spawn bilgisi kaydet]
    ↓ 
[Hedef sahne yüklen]
    ↓ 
[SceneManager: "DoorSpawn" bul]
    ↓ 
[Oyuncuyu spawn et + Kamerayı güncelle]
```

## 🚨 Önemli Notlar

- **Her iç mekanda "DoorSpawn" objesi olmalı**
- **Build Settings'te sahneler ekli olmalı**
- **Player "Player" tag'ine sahip olmalı**
- **CameraManager sahnede olmalı**

## 🐛 Sorun Giderme

**Oyuncu görünmüyor?**
- Console'da "Camera updated to follow player" mesajını kontrol et
- CameraManager var mı?

**Spawn çalışmıyor?**
- "DoorSpawn" objesi var mı?
- Console'da spawn mesajlarını kontrol et

**Kamera takip etmiyor?**
- SceneManager otomatik düzelir, 0.1s bekle

Bu sistem **%100 stabil** ve **minimum setup** gerektirir! 🎮 