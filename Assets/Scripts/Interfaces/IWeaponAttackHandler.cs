using UnityEngine;

public interface IWeaponAttackHandler
{
    /// <summary>
    /// Handles the attack logic for a specific weapon type
    /// </summary>
    /// <param name="player">The player performing the attack</param>
    /// <param name="attackPosition">The position where the attack should be performed</param>
    void HandleAttack(Player player, Vector2 attackPosition);
    
    /// <summary>
    /// Gets the weapon type this handler is responsible for
    /// </summary>
    WeaponType GetWeaponType();
} 