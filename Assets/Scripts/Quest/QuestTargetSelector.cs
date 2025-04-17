using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Bu bileşeni scene'deki düşmanları quest'e eklemek için kullanın
public class QuestTargetSelector : MonoBehaviour
{
    [Tooltip("Bu quest hedeflerinin ekleneceği quest")]
    public QuestData targetQuest;
    
    [FormerlySerializedAs("targetEnemies")]
    [Tooltip("Bu sahnedeki düşmanları bu listeye sürükleyin")]
    public List<GameObject> enemiesInScene = new List<GameObject>();
    
    // Sadece Unity Editor'da kullanılacak
    [ContextMenu("Add To Kill Objective")]
    private void AddToKillObjective()
    {
        #if UNITY_EDITOR
        if (targetQuest == null)
        {
            Debug.LogError("Lütfen önce bir Quest seçin!");
            return;
        }
        
        // Önce quest'te bir TargetedKillObjective var mı bak
        TargetedKillObjective killObjective = null;
        
        foreach (var objective in targetQuest.objectives)
        {
            if (objective is TargetedKillObjective targetedKill)
            {
                killObjective = targetedKill;
                break;
            }
        }
        
        // Eğer yoksa, yeni bir tane oluştur
        if (killObjective == null)
        {
            Debug.LogError("Bu quest'te bir TargetedKillObjective bulunamadı! Önce bir tane ekleyin.");
            return;
        }
        
        // Düşman ID'lerini topla
        List<string> enemyTypes = new List<string>();
        
        foreach (var enemy in enemiesInScene)
        {
            if (enemy == null) continue;
            
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null && !string.IsNullOrEmpty(enemyComponent.enemyType))
            {
                string enemyType = enemyComponent.enemyType;
                
                if (!enemyTypes.Contains(enemyType))
                {
                    enemyTypes.Add(enemyType);
                }
            }
            else
            {
                Debug.LogWarning($"'{enemy.name}' bir Enemy bileşeni içermiyor veya enemyType değeri boş!");
            }
        }
        
        // Objective'e ekle
        SerializedObject serializedObject = new SerializedObject(killObjective);
        SerializedProperty targetTypesProp = serializedObject.FindProperty("targetEnemyTypes");
        
        // Mevcut tipleri sil
        targetTypesProp.ClearArray();
        
        // Yeni tipleri ekle
        for (int i = 0; i < enemyTypes.Count; i++)
        {
            targetTypesProp.InsertArrayElementAtIndex(i);
            SerializedProperty element = targetTypesProp.GetArrayElementAtIndex(i);
            element.stringValue = enemyTypes[i];
        }
        
        // Değişiklikleri kaydet
        serializedObject.ApplyModifiedProperties();
        
        // Asset'i kaydet
        EditorUtility.SetDirty(killObjective);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Düşman tipleri quest'e eklendi! ({enemyTypes.Count} tip)");
        #endif
    }
    
    // Bu script sadece Editor'da kullanılır, Runtime'da etkin değildir
    private void Awake()
    {
        enabled = false;
    }
} 