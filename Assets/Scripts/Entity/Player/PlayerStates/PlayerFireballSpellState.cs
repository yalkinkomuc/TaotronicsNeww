using UnityEngine;

public class PlayerFireballSpellState : PlayerState
{
    private GameObject fireballPrefab;
    private Transform fireballSpawnPoint;
    
    private bool fireballCast = false;
    private bool triggerCalled = false;
    private SkillType skillType = SkillType.FireballSpell;
    
    public PlayerFireballSpellState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
        // Get references from player
        fireballPrefab = _player.fireballPrefab;
        fireballSpawnPoint = _player.fireballSpawnPoint;
    }
    
    public override void Enter()
    {
        base.Enter();
        fireballCast = false;
        triggerCalled = false;
        
        // Play fireball animation
        player.anim.SetBool("FireballSpell", true);
        
        // Check if skill is ready (includes mana and cooldown check)
        if (!SkillManager.Instance.IsSkillReady(skillType, player.stats.currentMana))
        {
            // Not enough mana or on cooldown, exit state
            stateMachine.ChangeState(player.idleState);
            
            if (player.stats.currentMana < SkillManager.Instance.GetSkillManaCost(skillType))
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.ShowCustomText("Not enough mana!", player.transform.position + Vector3.up, Color.blue);
                }
            }
            else
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.ShowCustomText("Skill on cooldown!", player.transform.position + Vector3.up, Color.yellow);
                }
            }
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        player.anim.SetBool("FireballSpell", false);
    }
    
    public override void Update()
    {
        base.Update();

        if (player.IsGroundDetected())
        {
        player.SetZeroVelocity();
            
        }
  
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
    
    public void CastFireball()
    {
        // Prevent casting multiple fireballs
        if (fireballCast) return;
        
        // Check if we have the prefab
        if (fireballPrefab == null)
        {
            Debug.LogError("Fireball prefab is not assigned!");
            return;
        }
        
        // Try to use the skill (this will check mana and put it on cooldown)
        float manaCost = SkillManager.Instance.GetSkillManaCost(skillType);
        
        // Use mana for the spell
        if (!player.stats.UseMana(manaCost)) return;
        
        // Set skill on cooldown
        SkillManager.Instance.UseSkill(skillType);
        
        fireballCast = true;
        triggerCalled = true;
        
        // Determine spawn point
        Vector3 spawnPosition;
        if (fireballSpawnPoint != null)
        {
            spawnPosition = fireballSpawnPoint.position;
        }
        else
        {
            // Fallback to a position in front of the player
            spawnPosition = player.transform.position + new Vector3(player.facingdir * 1f, 0.5f, 0);
        }
        
        // Instantiate fireball
        GameObject fireball = GameObject.Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
        
        // Initialize fireball with direction and player stats
        FireballSpell fireballScript = fireball.GetComponent<FireballSpell>();
        if (fireballScript != null)
        {
            fireballScript.Initialize(player.facingdir, player.stats as PlayerStats);
        }
    }
}
