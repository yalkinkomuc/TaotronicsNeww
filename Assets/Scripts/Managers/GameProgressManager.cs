using UnityEngine;
using System.Collections.Generic;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager instance;

    // Boss durumlarını tutan dictionary
    private Dictionary<string, bool> defeatedBosses = new Dictionary<string, bool>();
    
    // Oyun ilerlemesini kaydeden PlayerPrefs anahtarları
    private const string NECROMANCER_DEFEATED_KEY = "NecromancerDefeated";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
          //  DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Oyun ilerlemesini yükle
    private void LoadProgress()
    {
        defeatedBosses["Necromancer"] = PlayerPrefs.GetInt(NECROMANCER_DEFEATED_KEY, 0) == 1;
    }

    // Boss'un yenildiğini kaydet
    public void MarkBossAsDefeated(string bossName)
    {
        defeatedBosses[bossName] = true;
        
        if (bossName == "Necromancer")
        {
            PlayerPrefs.SetInt(NECROMANCER_DEFEATED_KEY, 1);
            PlayerPrefs.Save();
        }
    }

    // Boss'un yenilip yenilmediğini kontrol et
    public bool IsBossDefeated(string bossName)
    {
        if (defeatedBosses.ContainsKey(bossName))
            return defeatedBosses[bossName];
        
        return false;
    }

    // Tüm ilerlemeyi sıfırla (test için)
    public void ResetProgress()
    {
        defeatedBosses.Clear();
        PlayerPrefs.DeleteKey(NECROMANCER_DEFEATED_KEY);
        PlayerPrefs.Save();
        LoadProgress();
    }
} 