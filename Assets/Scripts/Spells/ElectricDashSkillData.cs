using UnityEngine;

[CreateAssetMenu(fileName = "ElectricDashSkill", menuName = "Data/Skills/ElectricDash")]
public class ElectricDashSkillData : SkillData
{
    [Header("Electric Dash Properties")]
    public float dashDistance = 6f;
    public float dashSpeed = 15f;
    public float damageMultiplier = 1.5f;
    
    private void OnEnable()
    {
        // Set default values
        if (string.IsNullOrEmpty(skillID))
            skillID = "electric_dash";
            
        if (string.IsNullOrEmpty(skillName))
            skillName = "Electric Dash";
            
        if (string.IsNullOrEmpty(description))
            description = "Dash forward with electric energy, damaging enemies in your path.";
            
        if (cooldown <= 0)
            cooldown = 8f;
            
        if (manaCost <= 0)
            manaCost = 30f;
            
        if (shardCost <= 0)
            shardCost = 70;
    }
} 