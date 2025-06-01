# Door System - Stabil Kapı Sistemi 🚪

En basit ve güvenilir kapı sistemi. SceneManager tarafından otomatik olarak yönetilir.

## 🎮 Nasıl Çalışır

1. **DoorTrigger** → Kapının yanına koy, W/↑ ile sahne geçişi
2. **Unique Spawn Point** → Hedef sahnede kapının önüne koy, otomatik spawn
3. **SceneManager** → Her şeyi otomatik halleder

## 📋 Kurulum (Süper Basit!)

### 1. Ana Sahne (Kapı Var)
```
DoorTrigger_House
├── BoxCollider2D (Is Trigger ✓)
├── DoorTrigger Script
│   ├── Target Scene Index: [Hedef sahne numarası]
│   └── Target Spawn Point Name: "HouseExit"
└── InteractionPrompt (opsiyonel)
    └── W tuşu sprite'ı
```

### 2. Hedef Sahne (İç Mekan)
```
HouseExit (empty GameObject)
└── Transform pozisyonu kapının önünde
```

## ⚙️ Ayarlar

### DoorTrigger
- **Target Scene Index**: Gidilecek sahnenin build index'i
- **Target Spawn Point Name**: Hedef sahnedeki spawn point'in adı
- **Player Tag**: "Player" (varsayılan)
- **Interaction Prompt**: W tuşu sprite'ı (opsiyonel)
- **Door Open Sound**: Kapı sesi (opsiyonel)

### Spawn Point
- **İsim**: DoorTrigger'daki Target Spawn Point Name ile aynı
- **Pozisyon**: Kapının önünde
- Script gerekmez!

## 🏠 İsimlendirme Örnekleri

### Mantıklı İsimlendirme:
```
House → "HouseExit"
Shop → "ShopExit"  
Cave → "CaveExit"
Inn → "InnExit"
Blacksmith → "BlacksmithExit"
```

### Sahne Bazlı İsimlendirme:
```
MainTown_House → "MainTown_HouseExit"
Forest_Cave → "Forest_CaveExit"
```

### ID Bazlı İsimlendirme:
```
Building_01 → "Building01_Exit"
Building_02 → "Building02_Exit"
```

## 🔧 Sistem Özellikleri

✅ **Unique İsimlendirme** - Her kapı kendi spawn point adını belirtir  
✅ **Otomatik Kamera Takibi** - Spawn sonrası kamera oyuncuyu bulur  
✅ **IInteractable Uyumlu** - Mevcut interaction sistemi  
✅ **Physics Sıfırlama** - Spawn sonrası velocity temizlenir  
✅ **Hata Toleransı** - Spawn point yoksa varsayılan spawn  
✅ **Debug Bilgileri** - Console'da detaylı loglar  

## 📁 Dosya Yapısı

```
Scene_MainTown/
├── DoorTrigger_House
│   ├── Target Scene Index: 2
│   └── Target Spawn Point Name: "HouseExit"
├── DoorTrigger_Shop
│   ├── Target Scene Index: 3
│   └── Target Spawn Point Name: "ShopExit"
└── HouseExit (kapının önünde)

Scene_HouseInterior/
├── DoorTrigger_Exit
│   ├── Target Scene Index: 1 (MainTown)
│   └── Target Spawn Point Name: "HouseExit"
├── PlayerSpawnPoint (giriş spawn)
└── Other objects...
```

## 🎯 Örnek Kullanım

### Ev Sistemi
1. **Ana Sahne**: 
   - DoorTrigger → Target Scene: 2, Target Spawn: "HouseExit"
   - HouseExit objesi kapının önünde
2. **Ev Sahnesi**: 
   - DoorTrigger → Target Scene: 1, Target Spawn: "HouseExit"
   - PlayerSpawnPoint içerde

### Çoklu Bina Sistemi
1. **Ana Sahne**:
   - House: "HouseExit" 
   - Shop: "ShopExit"
   - Inn: "InnExit"
2. **Her bina sahnesinde**:
   - DoorTrigger → ilgili Exit spawn'ına döner

## 🔄 Akış Diagramı

```
[Ana Sahne] 
    ↓ 
[Kapıya Yaklaş] 
    ↓ 
[W Tuşuna Bas] 
    ↓ 
[SceneManager: "HouseExit" kaydet]
    ↓ 
[Hedef sahne yüklen]
    ↓ 
[SceneManager: "HouseExit" bul]
    ↓ 
[Oyuncuyu spawn et + Kamerayı güncelle]
```

## 🚨 Önemli Notlar

- **Her spawn point unique isme sahip olmalı**
- **DoorTrigger'daki isim ile spawn point ismi aynı olmalı**
- **Build Settings'te sahneler ekli olmalı**
- **Player "Player" tag'ine sahip olmalı**

## 🐛 Sorun Giderme

**Spawn çalışmıyor?**
- Spawn point ismi DoorTrigger'daki ile aynı mı?
- Console'da "Spawn noktası bulunamadı" mesajı var mı?

**Yanlış yerde spawn oluyor?**
- Doğru spawn point objesini seçtin mi?
- İsim yazım hatası var mı?

Bu sistem **unique isimlendirme** ile **sınırsız bina** desteği sağlar! 🏠🏪🏨 