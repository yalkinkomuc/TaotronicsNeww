# Door Trigger System - Kapı Sistemi

Bu sistem 2D oyunlarda kapı geçişleri için kullanılır. Oyuncu trigger alanına girdiğinde W veya yukarı ok tuşuna basarak sahne geçişi yapabilir. Mevcut InteractionPrompt sistemini kullanır.

## Prefab Oluşturma

1. **Boş GameObject Oluştur**
   - Hierarchy'de sağ tık → Create Empty
   - İsim: "DoorTrigger"

2. **Collider2D Ekle**
   - DoorTrigger objesini seç
   - Add Component → Physics 2D → Box Collider 2D
   - "Is Trigger" kutusunu işaretle ✓
   - Collider boyutunu kapı alanına göre ayarla

3. **DoorTrigger Script Ekle**
   - Add Component → Scripts → DoorTrigger

4. **InteractionPrompt Ekle**
   - DoorTrigger'ın child'ı olarak boş GameObject oluştur
   - İsim: "InteractionPrompt"
   - Add Component → Scripts → InteractionPrompt
   - Prompt Visual olarak joystick tuşu sprite'ını ekle

5. **Prompt Visual Ayarla**
   - InteractionPrompt'un child'ı olarak GameObject oluştur
   - SpriteRenderer ekle
   - Joystick tuşu sprite'ını ata (W veya ↑ tuşu)
   - Kapının üstüne konumlandır

## Script Ayarları

### Scene Settings
- **Target Scene Index**: Gidilecek sahnenin build index'i
- **Player Tag**: Oyuncu objesinin tag'i (genellikle "Player")

### Interaction Settings
- **Interaction Prompt**: InteractionPrompt komponenti (otomatik bulunur)

### Audio Settings
- **Door Open Sound**: Kapı açılma ses efekti
- **Audio Source**: Ses kaynağı (otomatik oluşturulur)

## Kullanım

1. **Prefab Oluştur**
   - DoorTrigger objesini Project'e sürükle
   - Prefab olarak kaydet

2. **Sahneye Yerleştir**
   - Prefab'ı sahneye sürükle
   - Kapının olduğu yere konumlandır
   - Target Scene Index'i ayarla

3. **Farklı Kapılar İçin**
   - Aynı prefab'ı kullan
   - Sadece Target Scene Index'i değiştir
   - Gerekirse prompt sprite'ını değiştir

## Özellikler

- ✅ W veya ↑ tuşu ile aktivasyon
- ✅ Mevcut InteractionPrompt sistemi entegrasyonu
- ✅ Joystick tuşu sprite desteği
- ✅ Ses efekti desteği
- ✅ Oyuncu hareketi durdurma
- ✅ Çoklu kullanım için prefab uyumlu
- ✅ Scene Editor'da görsel gizmo
- ✅ Debug log mesajları

## Hierarchy Yapısı

```
DoorTrigger
├── BoxCollider2D (Is Trigger ✓)
├── DoorTrigger Script
├── AudioSource (otomatik)
└── InteractionPrompt
    ├── InteractionPrompt Script
    └── PromptVisual
        ├── SpriteRenderer
        └── Joystick Tuşu Sprite (W veya ↑)
```

## Notlar

- Oyuncu "Player" tag'ine sahip olmalı
- Trigger alanı oyuncunun collider'ı ile çakışmalı
- InteractionPrompt otomatik olarak bulunur
- Ses efekti opsiyonel (boş bırakılabilir)
- Mevcut interaction sistemi ile uyumlu

## Örnek Kullanım Senaryoları

1. **Ev Kapısı**: Evin dışından içine geçiş
2. **Mağara Girişi**: Açık alandan mağaraya geçiş  
3. **Bina Girişi**: Sokaktan bina içine geçiş
4. **Portal**: Farklı bölgeler arası geçiş

## InteractionPrompt Entegrasyonu

Bu sistem mevcut `InteractionPrompt` sistemini kullanır:
- Oyuncu trigger'a girdiğinde prompt gösterilir
- Oyuncu trigger'dan çıktığında prompt gizlenir
- Joystick tuşu sprite'ları kullanılabilir
- Tutarlı UI deneyimi sağlar 