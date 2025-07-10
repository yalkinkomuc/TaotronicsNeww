using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MainMenu_UI : MonoBehaviour
{
   [Header("Clear Save Confirmation")]
   [SerializeField] private GameObject confirmationPanel;
   [SerializeField] private UnityEngine.UI.Button confirmButton;
   [SerializeField] private UnityEngine.UI.Button cancelButton;

   private void Start()
   {
      // Confirmation panel'i başlangıçta kapalı tut
      if (confirmationPanel != null)
      {
         confirmationPanel.SetActive(false);
      }
      
      // Button event'lerini ayarla
      if (confirmButton != null)
         confirmButton.onClick.AddListener(ConfirmClearSave);
      if (cancelButton != null)
         cancelButton.onClick.AddListener(CancelClearSave);
   }

   public void PlayGame()
   {
      UnityEngine.SceneManagement.SceneManager.LoadScene(1);
   }
   
   public void ExitGame()
   {
      Application.Quit();
   }
   
   // Clear Save butonuna basıldığında çağrılır
   public void OnClearSaveButtonClicked()
   {
      if (confirmationPanel != null)
      {
         confirmationPanel.SetActive(true);
      }
      else
      {
         // Eğer confirmation panel yoksa direkt sor
         if (UnityEngine.Application.isEditor)
         {
            // Editor'da UnityEditor.EditorUtility.DisplayDialog kullan
            #if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("Save Verilerini Sil", 
                "Tüm oyuncu verilerini silmek istediğinizden emin misiniz? Bu işlem geri alınamaz!", 
                "Evet, Sil", "İptal"))
            {
               ClearAllSaveData();
            }
            #endif
         }
         else
         {
            // Build'de direkt sil (confirmation panel olmadığı için)
            Debug.LogWarning("Confirmation panel atanmadı, save data siliniyor...");
            ClearAllSaveData();
         }
      }
   }
   
   // Kullanıcı silmeyi onayladığında
   private void ConfirmClearSave()
   {
      ClearAllSaveData();
      if (confirmationPanel != null)
         confirmationPanel.SetActive(false);
   }
   
   // Kullanıcı iptal ettiğinde
   private void CancelClearSave()
   {
      if (confirmationPanel != null)
         confirmationPanel.SetActive(false);
   }
   
   // Tüm save verilerini temizle
   private void ClearAllSaveData()
   {
      Debug.Log("Tüm save verileri temizleniyor...");
      
      // PlayerPrefs'teki tüm oyuncu verilerini sil
      ClearPlayerData();
      ClearQuestData();
      ClearInventoryData();
      ClearWeaponData();
      ClearSkillData();
      ClearChestData();
      ClearGameProgressData();
      ClearMiscData();
      
      // Tüm değişiklikleri kaydet
      PlayerPrefs.Save();
      
      Debug.Log("Tüm save verileri başarıyla temizlendi!");
   }
   
   private void ClearPlayerData()
   {
      // Checkpoint verileri
      PlayerPrefs.DeleteKey("CheckpointActivated");
      PlayerPrefs.DeleteKey("CheckpointX");
      PlayerPrefs.DeleteKey("CheckpointY");
      PlayerPrefs.DeleteKey("CheckpointSceneIndex");
      
      // Oyuncu stat verileri
      PlayerPrefs.DeleteKey("PlayerLevel");
      PlayerPrefs.DeleteKey("PlayerMaxHealth");
      PlayerPrefs.DeleteKey("PlayerMaxMana");
      PlayerPrefs.DeleteKey("PlayerBaseDamage");
      PlayerPrefs.DeleteKey("PlayerSkillPoints");
      PlayerPrefs.DeleteKey("PlayerVitality");
      PlayerPrefs.DeleteKey("PlayerMight");
      PlayerPrefs.DeleteKey("PlayerMind");
      PlayerPrefs.DeleteKey("PlayerDefense");
      PlayerPrefs.DeleteKey("PlayerLuck");
      PlayerPrefs.DeleteKey("PlayerExperience");
      PlayerPrefs.DeleteKey("PlayerExperienceToNextLevel");
      PlayerPrefs.DeleteKey("PlayerGold");
      
      // Silah durumları
      PlayerPrefs.DeleteKey("HasBoomerang");
      PlayerPrefs.DeleteKey("HasSpellbook");
      PlayerPrefs.DeleteKey("HasSword");
   }
   
   private void ClearQuestData()
   {
      PlayerPrefs.DeleteKey("ActiveQuestIDs");
      PlayerPrefs.DeleteKey("QuestObjectiveIndices");
      PlayerPrefs.DeleteKey("CompletedQuests");
      
      // Quest objective verilerini temizlemek için QuestSaveSystem kullan
      // Not: Bu kısım için available quest listesine ihtiyaç var, 
      // eğer erişim varsa QuestSaveSystem.ClearAllQuestData() çağrılabilir
   }
   
   private void ClearInventoryData()
   {
      PlayerPrefs.DeleteKey("PlayerInventory");
      PlayerPrefs.DeleteKey("CollectedItems");
   }
   
   private void ClearWeaponData()
   {
      // Silah seviye verilerini temizle
      // Tüm weapon ID'leri için döngü yapabiliriz ya da prefix kullanabiliriz
      string[] weaponIDs = {"Sword", "Boomerang", "Spellbook"}; // Bilinen silah ID'leri
      
      foreach (string weaponID in weaponIDs)
      {
         PlayerPrefs.DeleteKey($"Weapon_{weaponID}_Level");
      }
   }
   
   private void ClearSkillData()
   {
      PlayerPrefs.DeleteKey("PlayerShards");
      PlayerPrefs.DeleteKey("PlayerSkills");
   }
   
   private void ClearChestData()
   {
      PlayerPrefs.DeleteKey("ChestData");
   }
   
   private void ClearGameProgressData()
   {
      PlayerPrefs.DeleteKey("NecromancerDefeated");
   }
   
   private void ClearMiscData()
   {
      PlayerPrefs.DeleteKey("ReadDialogues");
      PlayerPrefs.DeleteKey("LastSelectedTab");
   }
}
