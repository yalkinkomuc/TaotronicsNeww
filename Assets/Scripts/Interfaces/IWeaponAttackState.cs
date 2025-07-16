public interface IWeaponAttackState
{
    /// <summary>
    /// Gets the current combo counter for this weapon attack
    /// </summary>
    int GetComboCounter();
    
    /// <summary>
    /// Gets the weapon type for this attack state
    /// </summary>
    WeaponType GetWeaponType();
    
    /// <summary>
    /// Gets the damage multiplier for the specified combo index
    /// </summary>
    /// <param name="comboIndex">The combo index (0, 1, 2, etc.)</param>
    float GetDamageMultiplier(int comboIndex);
    
    /// <summary>
    /// Gets the knockback multiplier for this weapon type
    /// </summary>
    float GetKnockbackMultiplier(int comboIndex);
    
    /// <summary>
    /// Gets the combo window duration for this weapon
    /// </summary>
    float GetComboWindow();
} 