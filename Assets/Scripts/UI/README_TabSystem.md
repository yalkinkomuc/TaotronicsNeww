# Tab Navigation System

Bu sistem, oyunlarda Q/E (PC) ve RB/LB (Gamepad) tuşları ile menü geçişleri yapmaya olanak sağlar. Sistem tamamen genişletilebilir ve yeni menüler kolayca eklenebilir.

## Özellikler

- ✅ Q/E (PC) ve RB/LB (Gamepad) ile menü geçişi
- ✅ Görsel platform göstergeleri (PC/Gamepad ikonları)
- ✅ Ses efektleri desteği
- ✅ Fade animasyonları
- ✅ Dinamik menü ekleme/çıkarma
- ✅ Event sistemi
- ✅ Tam genişletilebilirlik

## Sistem Bileşenleri

### 1. TabManager
Ana tab yönetim sistemi. Tüm tab'ları yönetir ve input handling yapar.

### 2. TabData
Her tab için konfigürasyon data'sı:
```csharp
[System.Serializable]
public class TabData
{
    public string tabName;
    public GameObject tabPanel;
    public Button tabButton;
    public Action onTabSelected;
    public Color activeColor;
    public Color inactiveColor;
}
```

### 3. TabNavigationHints
Platform-specific navigation hint'leri gösterir (Q/E veya RB/LB).

### 4. Updated Input System
```csharp
// IPlayerInput interface'ine eklendi:
bool tabLeftInput { get; }   // Q (PC) / LB (Gamepad)
bool tabRightInput { get; }  // E (PC) / RB (Gamepad)
```

## Kurulum

### 1. Mevcut Sistem (AdvancedInventoryUI)
Sistem AdvancedInventoryUI'ya entegre edildi:
- Inventory menüsü: Tab 0
- Collectibles menüsü: Tab 1

### 2. Inspector'da Konfigürasyon
`AdvancedInventoryUI` inspector'ında:
1. `Tab Manager` referansını atayın
2. TabManager object'inde tab'ları konfigüre edin:
   - Tab 0: Inventory (inventoryPanel, inventoryPageButton)
   - Tab 1: Collectibles (collectiblesPanel, collectiblesPageButton)

### 3. Navigation Hints Kurulumu
`TabNavigationHints` component'ini UI'ya ekleyin ve gerekli referansları atayın:
- hintsPanel: Hint UI container
- leftHintText/rightHintText: Platform text'leri
- leftHintIcon/rightHintIcon: Platform ikonları
- Platform ikonları (Q, E, LB, RB sprites)

## Yeni Menü Ekleme

### Yöntem 1: Inspector'da
1. Yeni tab panel'ini oluşturun
2. Tab button'u ekleyin
3. TabManager'ın tabs listesine yeni TabData ekleyin

### Yöntem 2: Kod ile (Dinamik)
```csharp
TabData newTab = new TabData
{
    tabName = "Settings",
    tabPanel = settingsPanel,
    tabButton = settingsButton,
    onTabSelected = () => {
        // Tab seçildiğinde yapılacaklar
        InitializeSettingsPanel();
    }
};

tabManager.AddTab(newTab);
```

## Kullanım Örnekleri

### Temel Tab Kontrolü
```csharp
// Programmatik tab değiştirme
tabManager.SelectTab(0); // Inventory
tabManager.SelectTab(1); // Collectibles

// Tab bilgisi alma
int currentTab = tabManager.CurrentTabIndex;
string currentTabName = tabManager.CurrentTabName;
```

### Event Handling
```csharp
private void Start()
{
    // Tab değişim event'ini dinle
    TabManager.OnTabChanged += OnTabChanged;
}

private void OnTabChanged(int tabIndex, string tabName)
{
    Debug.Log($"Tab changed to: {tabName} (Index: {tabIndex})");
    
    // Tab'a özgü işlemler
    switch (tabIndex)
    {
        case 0:
            // Inventory tab seçildi
            break;
        case 1:
            // Collectibles tab seçildi
            break;
    }
}
```

### Conditional Tab Addition
```csharp
public void UnlockCraftingTab()
{
    if (player.HasSkill("Crafting"))
    {
        TabData craftingTab = new TabData
        {
            tabName = "Crafting",
            tabPanel = craftingPanel,
            tabButton = craftingButton,
            onTabSelected = () => InitializeCrafting()
        };
        
        tabManager.AddTab(craftingTab);
    }
}
```

## Input Mapping

| Platform | Tab Left | Tab Right |
|----------|----------|-----------|
| PC       | Q        | E         |
| Gamepad  | LB       | RB        |

## Genişletme Örnekleri

### Settings Menüsü
```csharp
private void AddSettingsTab()
{
    TabData settingsTab = new TabData
    {
        tabName = "Settings",
        tabPanel = settingsPanel,
        tabButton = settingsTabButton,
        onTabSelected = () => {
            // Audio settings
            // Graphics settings
            // Control settings
        }
    };
    
    tabManager.AddTab(settingsTab);
}
```

### Stats Menüsü
```csharp
private void AddStatsTab()
{
    TabData statsTab = new TabData
    {
        tabName = "Stats",
        tabPanel = statsPanel,
        tabButton = statsTabButton,
        onTabSelected = () => {
            // Player level/XP
            // Attribute points
            // Skill tree progress
        }
    };
    
    tabManager.AddTab(statsTab);
}
```

## Best Practices

1. **Performance**: Tab panel'leri sadece aktif olduklarında güncelle
2. **UX**: Navigation hint'leri kısa süre göster
3. **Accessibility**: Both button clicks ve keyboard navigation destekle
4. **Audio**: Tab değişim seslerini kısık tut
5. **Animation**: Smooth geçişler için fade kullan

## Troubleshooting

### Tab'lar çalışmıyor
- TabManager referansının atandığından emin olun
- Input system'in doğru çalıştığından emin olun
- UI InputBlocker ile çakışma olup olmadığını kontrol edin

### Input yanıt vermiyor
- UI açık olduğunda gameplayInputEnabled=false olmalı
- TabManager Update() çalıştığından emin olun

### Visual hint'ler görünmüyor
- TabNavigationHints component'inin aktif olduğundan emin olun
- Platform ikonlarının atandığından emin olun
- TabCount > 1 olduğundan emin olun

## Future Expansion Ideas

- Quest Journal menüsü
- Map/Minimap menüsü
- Achievement/Trophy menüsü
- Social/Friend list menüsü
- Options/Preferences menüsü
- Help/Tutorial menüsü

Sistem tamamen modüler ve genişletilebilir olarak tasarlandı. Yeni menüler eklemek için sadece TabData oluşturup tabManager.AddTab() çağırmak yeterli! 