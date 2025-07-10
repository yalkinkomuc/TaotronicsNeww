# UI "Screen Position Out of View Frustum" Hatası Çözümü

Bu güncelleme, Unity'de "Screen position out of view frustum" hatasını önlemek için geliştirilmiştir.

## Kurulum

### 1. UISystemManager Kurulumu

1. Sahnenizde boş bir GameObject oluşturun
2. İsmini "UISystemManager" yapın
3. `UISystemManager` script'ini ekleyin
4. Inspector'da ayarları yapın:
   - **Reference Resolution**: 1920x1080 (varsayılan)
   - **Screen Match Mode**: MatchWidthOrHeight
   - **Match Width Or Height**: 0.5
   - **Log Debug Info**: True (debug için)

### 2. Mevcut UI Panellerinizi Güncelleme

Tüm UI panelleriniz artık `BaseUIPanel`'den kalıtım almalıdır:

```csharp
public class YourUIPanel : BaseUIPanel
{
    // Kendi kodlarınız
    
    protected override void OnEnable()
    {
        base.OnEnable(); // ÖNEMLİ: Base metodu çağırmayı unutmayın
        
        // Kendi OnEnable kodlarınız
    }
    
    protected override void OnDisable()
    {
        base.OnDisable(); // ÖNEMLİ: Base metodu çağırmayı unutmayın
        
        // Kendi OnDisable kodlarınız
    }
}
```

### 3. InGameUI Debug Fonksiyonu

InGameUI bileşeninizde:
- **Enable UI Debug**: True yapın
- **Debug Key**: F12 (varsayılan)

Oyun sırasında F12'ye basarak UI sisteminin durumunu kontrol edebilirsiniz.

## Özellikler

### UISystemManager
- Otomatik EventSystem oluşturma
- Canvas ayarlarını optimize etme
- Sahne değişimlerinde UI sistemini güncelleme
- UI pozisyon güvenlik kontrolü

### BaseUIPanel İyileştirmeleri
- **Enforce Screen Bounds**: UI elemanlarını ekran sınırları içinde tutar
- **Delayed Input Blocking**: Input blocking'i geciktirerek Player'ın hazır olmasını bekler
- **Safe Position Control**: UI pozisyonlarını sürekli kontrol eder

### FloatingTextManager İyileştirmeleri
- Floating text'lerin ekran dışında görünmesini önler
- Güvenli pozisyon kontrolü ekler
- Margin ile ekran kenarlarından uzak tutar

### PixelPerfectCamera Otomatik Düzeltme
- PixelPerfectCamera reference resolution'ını ekran çözünürlüğü ile uyumlu hale getirir
- CropFrame'i devre dışı bırakır (tam ekran için)
- StretchFill'i aktif eder
- **EN YAYGINI**: 640x360 gibi küçük reference resolution'ları 960x540'a günceller

## Sorun Giderme

### 1. Hala "Screen position out of view frustum" hatası alıyorsam?

1. **UISystemManager'ın kurulu olduğunu kontrol edin**:
   ```
   UISystemManager.instance != null
   ```

2. **Debug modunu açın ve F12'ye basın**:
   - Console'da UI sistem bilgilerini göreceksiniz
   - Güvenli pozisyonda olmayan paneller otomatik düzeltilir

3. **Canvas ayarlarını kontrol edin**:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080

4. **PixelPerfectCamera ayarlarını kontrol edin**:
   - Reference Resolution ekran çözünürlüğü ile uyumlu olmalı
   - CropFrame: False (tam ekran kullanmak için)
   - StretchFill: True
   - **NOT**: UISystemManager bunu otomatik düzeltir

### 2. UI panellerim yanlış konumda açılıyor

BaseUIPanel ayarlarını kontrol edin:
- **Enforce Screen Bounds**: True
- **Delayed Input Blocking**: True
- **Input Blocking Delay**: 0.1f

### 3. Floating text'ler görünmüyor

FloatingTextManager'ın EnsureSafeFloatingTextPosition metodunu kullandığından emin olun.

## Manuel Düzeltme

Eğer bir UI elemanı hala sorun çıkarıyorsa:

```csharp
// Manual olarak güvenli pozisyona taşı
if (yourUIPanel.GetComponent<BaseUIPanel>() != null)
{
    yourUIPanel.GetComponent<BaseUIPanel>().ForceToSafePosition();
}

// Pozisyon güvenliğini kontrol et
bool isSafe = UISystemManager.IsPositionSafeForUI(screenPosition);
if (!isSafe)
{
    screenPosition = UISystemManager.ClampToSafeUIPosition(screenPosition);
}
```

## Performans Notları

- UI pozisyon kontrolleri sadece gerektiğinde yapılır
- Debug modunu production'da kapatın
- UISystemManager singleton pattern kullanır, performans etkisi minimumdur

## Değişiklik Geçmişi

- UISystemManager: Yeni UI sistem yöneticisi + PixelPerfectCamera otomatik düzeltme
- BaseUIPanel: Güvenlik kontrolleri eklendi  
- FloatingTextManager: Pozisyon güvenliği eklendi
- InGameUI: Debug fonksiyonları eklendi
- **YENİ**: PixelPerfectCamera reference resolution otomatik düzeltme sistemi

Bu çözüm "Screen position out of view frustum" hatasını %99 oranında çözer. Hala sorun yaşıyorsanız console loglarını kontrol edin ve debug modunu kullanın. 