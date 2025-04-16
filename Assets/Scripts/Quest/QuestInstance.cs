using System.Collections;
using UnityEngine;

[System.Serializable]
public class QuestInstance
{
    public QuestData Data;
    public int currentObjectiveIndex = 0;

    public bool isCompleted => currentObjectiveIndex >= Data.objectives.Length;
    
    public QuestObjective CurrentObjective => IsCompleted ? null : Data.objectives[currentObjectiveIndex];


    public bool IsCompleted => Data != null && Data.objectives != null && currentObjectiveIndex >= Data.objectives.Length;


    public void HandleEvent(string eventName, object data)
    {
        if (IsCompleted || Data == null || Data.objectives == null) return;

        var current = CurrentObjective;

        // Eğer geçerli objective tamamlandıysa, sıradaki objective'e geç
        if (current != null && !current.isCompleted)
        {
            // İlk kez çalıştırıldığında Initialize et
            if (!current.isInitialized)
            {
                current.Initialize();
            }
            
            current.HandleEvent(eventName, data);

            // Eğer bu objective tamamlandıysa sıradaki objective'e geç
            if (current.isCompleted)
            {
                currentObjectiveIndex++;
                Debug.Log($"Objective tamamlandı, sıradaki: {currentObjectiveIndex}");

                // Eğer tüm objective'ler tamamlandıysa quest tamamlandı
                if (IsCompleted)
                {
                    Debug.Log($"🎉 Quest tamamlandı: {Data.title}");
                }
                else
                {
                    // Bir sonraki objective için log ekleyelim
                    Debug.Log($"Yeni objective başlatıldı: {CurrentObjective.GetType().Name}");
                }
            }
        }
    }



}
