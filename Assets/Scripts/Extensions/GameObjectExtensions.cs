using UnityEngine;

public static class GameObjectExtensions
{
    /// <summary>
    /// Nesnenin üzerinde hasar metni gösterir
    /// </summary>
    /// <param name="gameObject">Hasar metninin gösterileceği nesne</param>
    /// <param name="damage">Gösterilecek hasar miktarı</param>
    /// <param name="isCritical">Kritik vuruş mu (rengi etkiler)</param>
    public static void ShowDamageText(this GameObject gameObject, float damage, bool isCritical = false)
    {
        if (FloatingTextManager.Instance != null)
        {
            // Nesnenin biraz üzerinde pozisyon hesapla
            Vector3 position = gameObject.transform.position + Vector3.up * 1.5f;
            FloatingTextManager.Instance.ShowDamageText(damage, position);
        }
    }
    
    /// <summary>
    /// Nesnenin üzerinde özel içerikli yüzen metin gösterir
    /// </summary>
    /// <param name="gameObject">Nesne</param>
    /// <param name="text">Gösterilecek metin</param>
    /// <param name="color">Metnin rengi</param>
    public static void ShowFloatingText(this GameObject gameObject, string text, Color color)
    {
        if (FloatingTextManager.Instance != null)
        {
            // Nesnenin biraz üzerinde pozisyon hesapla
            Vector3 position = gameObject.transform.position + Vector3.up * 1.5f;
            // Hazır metodu kullan
            FloatingTextManager.Instance.ShowCustomText(text, position, color);
        }
    }
    
    /// <summary>
    /// Nesnenin üzerinde sihir hasarı metni gösterir (mavi renkte)
    /// </summary>
    /// <param name="gameObject">Hasar metninin gösterileceği nesne</param>
    /// <param name="damage">Gösterilecek hasar miktarı</param>
    public static void ShowMagicDamageText(this GameObject gameObject, float damage)
    {
        if (FloatingTextManager.Instance != null)
        {
            // Nesnenin biraz üzerinde pozisyon hesapla
            Vector3 position = gameObject.transform.position + Vector3.up * 1.5f;
            FloatingTextManager.Instance.ShowMagicDamageText(damage, position);
        }
    }
    
    /// <summary>
    /// Nesnenin üzerinde iyileşme metni gösterir (yeşil renkte)
    /// </summary>
    /// <param name="gameObject">İyileşme metninin gösterileceği nesne</param>
    /// <param name="healAmount">İyileşme miktarı</param>
    
    
    /// <summary>
    /// Nesnenin üzerinde düşmandan düşen altın miktarını gösterir
    /// </summary>
    /// <param name="gameObject">Nesne</param>
    /// <param name="goldAmount">Altın miktarı</param>
    public static void ShowGoldDropText(this GameObject gameObject, int goldAmount)
    {
        if (FloatingTextManager.Instance != null)
        {
            // Nesnenin biraz üzerinde pozisyon hesapla
            Vector3 position = gameObject.transform.position + Vector3.up * 2f;
            string goldText = $"+{goldAmount} Altın";
            Color goldColor = new Color(1f, 0.84f, 0f); // Altın rengi
            FloatingTextManager.Instance.ShowCustomText(goldText, position, goldColor);
        }
    }
} 