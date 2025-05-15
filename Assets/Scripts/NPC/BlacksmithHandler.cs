using UnityEngine;

public class BlacksmithHandler : MonoBehaviour
{
    [SerializeField] private BlacksmithUI blacksmithUI;
    [SerializeField] private string blacksmithUIPrefabPath = "UI/BlacksmithUI"; // Resources klasöründeki prefab yolu
    [SerializeField] private bool useResourcesForUI = true; // Prefabı Resources'dan yükleme seçeneği
   // [SerializeField] private AudioClip greetingSound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        // BlacksmithUI referansını hemen bul veya yükle
        EnsureBlacksmithUIExists();
    }
    
    // BlacksmithUI'ın varlığını kontrol et ve gerekirse oluştur
    private void EnsureBlacksmithUIExists()
    {
        // Önce singleton instance'ı kontrol et
        if (BlacksmithUI.Instance != null)
        {
            blacksmithUI = BlacksmithUI.Instance;
            return;
        }
        
        // Referans yoksa, sahnede bulmayı dene
        if (blacksmithUI == null)
        {
            blacksmithUI = Object.FindFirstObjectByType<BlacksmithUI>();
            
            // Hala bulunamadıysa ve Resources kullanma seçeneği açıksa, prefab'dan yükle
            if (blacksmithUI == null && useResourcesForUI)
            {
                LoadBlacksmithUIFromResources();
            }
        }
    }
    
    // BlacksmithUI prefabını Resources klasöründen yükle
    private void LoadBlacksmithUIFromResources()
    {
        try
        {
            // Prefabı Resources'dan yükle
            GameObject prefab = Resources.Load<GameObject>(blacksmithUIPrefabPath);
            
            if (prefab != null)
            {
                // Prefabı instantiate et
                GameObject uiObj = Instantiate(prefab);
                
                // DontDestroyOnLoad ile sahne geçişlerinde korunmasını sağla
                DontDestroyOnLoad(uiObj);
                
                // BlacksmithUI bileşenini al
                blacksmithUI = uiObj.GetComponent<BlacksmithUI>();
                
                Debug.Log("BlacksmithUI prefabı başarıyla yüklendi");
            }
            else
            {
                Debug.LogError($"BlacksmithUI prefabı bulunamadı: {blacksmithUIPrefabPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BlacksmithUI prefabı yüklenirken hata: {e.Message}");
        }
    }
    
    public void OpenBlacksmith(Player player)
    {
        if (player == null)
        {
            Debug.LogError("BlacksmithHandler: Player is null!");
            return;
        }
        
        // UI referansının varlığını kontrol et
        EnsureBlacksmithUIExists();
        
        // Singleton instance varsa onu kullan, yoksa local referansı kullan
        BlacksmithUI ui = BlacksmithUI.Instance != null ? BlacksmithUI.Instance : blacksmithUI;
        
        // BlacksmithUI hala yoksa, bu noktada işlemi durdur
        if (ui == null)
        {
            Debug.LogError("BlacksmithHandler: BlacksmithUI referansı bulunamadı!");
            return;
        }
        
        // BlacksmithManager kontrolü
        if (BlacksmithManager.Instance == null)
        {
            Debug.LogError("BlacksmithHandler: BlacksmithManager.Instance null!");
            
            // Manager'ı bulmayı dene
            BlacksmithManager manager = FindFirstObjectByType<BlacksmithManager>();
            if (manager != null)
            {
                Debug.Log("BlacksmithHandler: BlacksmithManager bulundu, instance atanıyor.");
                BlacksmithManager.Instance = manager;
            }
            else
            {
                Debug.LogError("BlacksmithHandler: BlacksmithManager bulunamadı, UI açılamıyor!");
                return;
            }
        }
        
        // Get player stats
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        
        if (playerStats == null)
        {
            Debug.LogError("BlacksmithHandler: Player does not have PlayerStats component!");
            return;
        }
        
        // Open blacksmith UI
        if (ui != null)
        {
            Debug.Log("BlacksmithHandler: Opening BlacksmithUI");
            ui.OpenBlacksmith(playerStats);
        }
        else
        {
            Debug.LogError("BlacksmithHandler: BlacksmithUI reference not set!");
        }
    }
}