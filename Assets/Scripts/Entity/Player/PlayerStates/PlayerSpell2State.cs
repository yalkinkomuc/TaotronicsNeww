using UnityEngine;
using System.Collections;

public class PlayerSpell2State : PlayerState
{
    private FireSpell currentFireSpell;
    private const string SPELL2_ANIM_NAME = "PlayerSpell2"; // Animator'daki state ismiyle aynı olmalı
    private const float MIN_CHARGE_TIME = 0.2f; // Minimum şarj süresi
    private const float MAX_CHARGE_TIME = 1000f;
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
        
        player.anim.Play(SPELL2_ANIM_NAME);
    }

    public override void Update()
    {
        base.Update();
        player.SetZeroVelocity();

        // Mana kontrolü
        if (!player.HasEnoughMana(player.spell2ManaDrainPerSecond * Time.deltaTime))
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        currentChargeTime += Time.deltaTime;

        // Minimum şarj süresine ulaştıysak ve henüz spawn etmediysek
        if (currentChargeTime >= MIN_CHARGE_TIME && !hasSpawnedSpell)
        {
            hasSpawnedSpell = true;
            player.StartCoroutine(DelayedSpawnFireSpell());
        }
        
        // T tuşu bırakıldığında veya max süre dolduğunda
        if (!player.playerInput.spell2Input || currentChargeTime >= MAX_CHARGE_TIME)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        player.anim.speed = 1;
        
        if (player.spellbookWeapon != null)
        {
            player.spellbookWeapon.animator.speed = 1;
        }
        if (player.swordWeapon != null)
        {
            player.swordWeapon.animator.speed = 1;
        }
        
        if (currentFireSpell != null)
        {
            GameObject.Destroy(currentFireSpell.gameObject);
            currentFireSpell = null;
        }
    }

    private IEnumerator DelayedSpawnFireSpell()
    {
        yield return new WaitForSeconds(0.1f); // Çok kısa bir delay
        SpawnFireSpell();
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
