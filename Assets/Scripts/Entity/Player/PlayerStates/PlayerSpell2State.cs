using UnityEngine;
using System.Collections;

public class PlayerSpell2State : PlayerState
{
    private FireSpell currentFireSpell;
    private const string SPELL2_ANIM_NAME = "PlayerSpell2"; // Animator'daki state ismiyle aynı olmalı
    private const float MIN_CHARGE_TIME = 0.35f; // Minimum şarj süresi
    private const float MAX_CHARGE_TIME = 3.5f;
    private float currentChargeTime;
    private bool hasSpawnedSpell;

    public PlayerSpell2State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        currentChargeTime = 0f;
        hasSpawnedSpell = false;
        
        // Animasyonu başlat
        player.anim.Play(SPELL2_ANIM_NAME);
        
        if (player.spellbookWeapon != null)
        {
            player.spellbookWeapon.PauseAnimation();
        }
    }

    public override void Update()
    {
        base.Update();
        player.SetZeroVelocity();

        currentChargeTime += Time.deltaTime;
        
        // Minimum şarj süresini geçtiyse ve henüz spawn olmadıysa
        if (currentChargeTime >= MIN_CHARGE_TIME && !hasSpawnedSpell)
        {
            SpawnFireSpell();
            hasSpawnedSpell = true;
        }

        // T tuşu bırakıldığında VEYA maksimum süre dolduğunda
        if (!player.playerInput.spell2Input || currentChargeTime >= MAX_CHARGE_TIME)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Oyuncu animasyonunu devam ettir
        player.anim.speed = 1;
        
        // Spellbook animasyonunu idle'a döndür ve devam ettir
        if (player.spellbookWeapon != null)
        {
            player.spellbookWeapon.PlayIdleAnimation();
            player.spellbookWeapon.ResumeAnimation();
        }
        
        // FireSpell'i yok et
        if (currentFireSpell != null)
        {
            GameObject.Destroy(currentFireSpell.gameObject);
            currentFireSpell = null;
        }
    }

    private void SpawnFireSpell()
    {
        if (player.fireSpellPrefab != null && player.fireSpellPoint != null)
        {
            GameObject spellObj = GameObject.Instantiate(player.fireSpellPrefab, 
                player.fireSpellPoint.position, 
                player.fireSpellPoint.rotation);
            
            currentFireSpell = spellObj.GetComponent<FireSpell>();
        }
    }
}
