# DEMİRCİ SİSTEMİ DOKÜMANTASYONU

Bu dokümantasyon, oyunda eklenen demirci sisteminin nasıl kullanılacağını, nasıl yapılandırılacağını ve özelleştirileceğini açıklar.

## İÇİNDEKİLER
1. Genel Bakış
2. Kurulum Adımları
3. Silah Yükseltme Sistemi
4. Demirci UI'ı
5. Yapılandırma Seçenekleri
6. Kod Entegrasyonu

## 1. GENEL BAKIŞ

Demirci sistemi, oyuncunun silahlarını yükseltmesine ve daha güçlü hale getirmesine olanak tanır. Her silah türü (Kılıç, Bumerang, Büyü Kitabı) için ayrı yükseltmeler yapılabilir ve her yükseltme, silahın hasar değerini artırır.

Ana bileşenler:
- `WeaponData`: Her silahın özelliklerini ve yükseltme parametrelerini içerir
- `BlacksmithManager`: Silah yükseltmelerini yöneten singleton
- `BlacksmithUI`: Demirci arayüzü
- `Blacksmith`: Demirci NPC'sinin etkileşim bileşeni

## 2. KURULUM ADIMLARI

1. Prefabrikler klasöründen "Blacksmith_NPC" prefabını sahnenize ekleyin
2. Canvas prefabından "BlacksmithUI" prefabını canvas'ınıza ekleyin
3. "BlacksmithManager" nesnesinin sahnede otomatik oluşturulduğunu doğrulayın
4. (İsteğe bağlı) BlacksmithManager'daki silah verilerini özelleştirin

Dikkat: BlacksmithManager, ilk oluşturulduğunda standart silah değerlerini otomatik olarak ayarlar.

## 3. SİLAH YÜKSELTME SİSTEMİ

Her silah için şu özellikler ayarlanabilir:
- Baz hasar bonusu: Silahın 1. seviyede sağladığı temel hasar bonusu
- Hasar artış değeri: Her seviyede eklenen ek hasar miktarı
- Maksimum seviye: Silahın ulaşabileceği en yüksek seviye (varsayılan: 5)
- Yükseltme maliyeti: İlk seviye yükseltmenin altın maliyeti
- Maliyet çarpanı: Her seviyede maliyet artış katsayısı (örn: 1.5 = %50 artış)

Yükseltme formülü:
- Toplam Hasar Bonusu = Baz Hasar + (Seviye - 1) * Hasar Artış Değeri
- Yükseltme Maliyeti = Baz Maliyet * (Maliyet Çarpanı ^ Mevcut Seviye)

## 4. DEMİRCİ UI'I

BlacksmithUI prefabı şu bileşenlerden oluşur:
- Silah seçim butonları: Mevcut tüm silahları gösterir
- Yükseltme bölümü: Seçilen silahın bilgilerini ve yükseltme seçeneklerini gösterir
- Seviye göstergeleri: Silahın mevcut seviyesini görsel olarak gösterir
- Altın göstergesi: Oyuncunun mevcut altın miktarını gösterir

## 5. YAPILANDIRMA SEÇENEKLERİ

BlacksmithManager üzerinde şu özellikler ayarlanabilir:
- `weaponDatabase`: Tüm silahların verilerini içeren liste
  - Her silah için ID, isim, tür, baz hasar, seviye ve maliyet değerleri

BlacksmithUI üzerinde şu özellikler ayarlanabilir:
- Görsel temalar (renkler, fontlar)
- Ses efektleri (seçim, yükseltme, açılış sesleri)
- Buton ve panel pozisyonları

## 6. KOD ENTEGRASYONU

Kendi kodunuzda demirci sistemini entegre etmek için:

```csharp
// Silah yükseltmelerini oyuncuya uygulamak için
BlacksmithManager.Instance.ApplyWeaponUpgrades(playerStats);

// Belirli bir silahı programatik olarak yükseltmek için
BlacksmithManager.Instance.UpgradeWeapon("sword", playerStats);

// Tüm silahların listesini almak için
List<WeaponData> weapons = BlacksmithManager.Instance.GetAllWeapons();

// Belirli bir silahın verilerini almak için
WeaponData sword = BlacksmithManager.Instance.GetWeapon("sword");

// Programatik olarak demirci UI'ını açmak için
blacksmithUI.OpenBlacksmith(playerStats);
```

Tüm veriler otomatik olarak PlayerPrefs ile kaydedilir, manuel kaydetme işlemi gerektirmez. 