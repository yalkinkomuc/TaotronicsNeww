using UnityEngine;
using System.Collections;

public class PlayerSpell2State : PlayerState
{
    private FireSpell currentFireSpell;
    private const string SPELL2_ANIM_NAME = "PlayerSpell2"; // Animator'daki state ismiyle aynı olmalı
    private const float MAX_CHARGE_TIME = 1000f;
    private float currentChargeTime;
    private bool isSpellActive;

    public PlayerSpell2State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        currentChargeTime = 0f;
        isSpellActive = false;
        
        player.anim.Play(SPELL2_ANIM_NAME);
    }

    public override void Update()
    {
        base.Update();
        player.SetZeroVelocity();

        currentChargeTime += Time.deltaTime;
        
        // Spell aktifse mana tüketmeye başla
        if (isSpellActive)
        {
            float manaCost = player.fireSpellManaDrainPerSecond * Time.deltaTime;
            // Mana kontrolü
            if (!player.HasEnoughMana(manaCost))
            {
                CleanupSpell();
                stateMachine.ChangeState(player.idleState);
                return;
            }
            
            // Mana yeterliyse kullan
            player.UseMana(manaCost);
        }
        
        // T tuşu bırakıldığında veya max süre dolduğunda
        if (!player.playerInput.spell2Input || currentChargeTime >= MAX_CHARGE_TIME)
        {
            CleanupSpell();
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        CleanupSpell();
        
        player.anim.speed = 1;
        
        if (player.spellbookWeapon != null)
        {
            player.spellbookWeapon.animator.speed = 1;
        }
        if (player.swordWeapon != null)
        {
            player.swordWeapon.animator.speed = 1;
        }
    }

    private void CleanupSpell()
    {
        if (currentFireSpell != null)
        {
            GameObject.Destroy(currentFireSpell.gameObject);
            currentFireSpell = null;
        }
        isSpellActive = false;
    }

    // Animation event'ten çağrılacak metod - PauseSpell2Animation çağrıldığında fire spell'i spawn et
    public void SpawnFireSpell()
    {
        if (player.fireSpellPrefab != null && player.fireSpellPoint != null && !isSpellActive)
        {
            GameObject spellObj = GameObject.Instantiate(player.fireSpellPrefab, 
                player.fireSpellPoint.position, 
                player.fireSpellPoint.rotation,
                player.transform);
            
            currentFireSpell = spellObj.GetComponent<FireSpell>();
            isSpellActive = true;
            
            // Fire spell'in damage collider'ını aktif et
            if (currentFireSpell != null)
            {
                currentFireSpell.EnableDamage();
            }
        }
    }

    public float GetCurrentChargeTime()
    {
        return currentChargeTime;
    }
}
