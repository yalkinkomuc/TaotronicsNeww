using UnityEngine;
using System.Collections;

public class PlayerSpell2State : PlayerState
{
    private FireSpell currentFireSpell;
    private const string SPELL2_ANIM_NAME = "PlayerSpell2"; // Animator'daki state ismiyle aynı olmalı
    public const float MIN_CHARGE_TIME = 0.2f; // Minimum şarj süresi
    private const float MAX_CHARGE_TIME = 1000f;
    private float currentChargeTime;
    private bool hasSpawnedSpell;
    private bool isSpellActive;

    public PlayerSpell2State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        currentChargeTime = 0f;
        hasSpawnedSpell = false;
        isSpellActive = false;
        
        player.anim.Play(SPELL2_ANIM_NAME);
    }

    public override void Update()
    {
        base.Update();
        player.SetZeroVelocity();

        currentChargeTime += Time.deltaTime;

        // Minimum şarj süresine ulaştıysak ve henüz spawn etmediysek
        if (currentChargeTime >= MIN_CHARGE_TIME && !hasSpawnedSpell)
        {
            hasSpawnedSpell = true;
            isSpellActive = true;
            player.StartCoroutine(DelayedSpawnFireSpell());
        }
        
        // Minimum şarj süresinden sonra mana tüketmeye başla
        if (currentChargeTime >= MIN_CHARGE_TIME && isSpellActive)
        {
            float manaCost = player.spell2ManaDrainPerSecond * Time.deltaTime;
            // Mana kontrolü
            if (!player.HasEnoughMana(manaCost))
            {
                Debug.Log($"Not enough mana to sustain fire spell! Required: {manaCost}, Current: {player.stats.currentMana}");
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

    private IEnumerator DelayedSpawnFireSpell()
    {
        yield return new WaitForSeconds(0.1f); // Çok kısa bir delay
        SpawnFireSpell();
    }

    private void SpawnFireSpell()
    {
        if (player.fireSpellPrefab != null && player.fireSpellPoint != null && isSpellActive)
        {
            GameObject spellObj = GameObject.Instantiate(player.fireSpellPrefab, 
                player.fireSpellPoint.position, 
                player.fireSpellPoint.rotation);
            
            currentFireSpell = spellObj.GetComponent<FireSpell>();
        }
    }

    public float GetCurrentChargeTime()
    {
        return currentChargeTime;
    }
}
