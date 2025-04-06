using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestObjectiveItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private Image checkmarkImage;
    
    public void Setup(QuestObjective objective)
    {
        if (objectiveText != null)
        {
            objectiveText.text = objective.isCompleted ? 
                $"{objective.description} (Tamamlandı)" : 
                objective.description;
        }
        
        if (checkmarkImage != null)
        {
            checkmarkImage.enabled = objective.isCompleted;
        }
    }
} 