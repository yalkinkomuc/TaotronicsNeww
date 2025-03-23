using UnityEngine;
using System.Collections;

public class PlayerSpell2State : PlayerState
{
    private FireSpell currentFireSpell;
    private const string SPELL2_ANIM_NAME = "PlayerSpell2"; // Animator'daki state ismiyle aynı olmalı
    private const float SPAWN_DELAY = 0.5f;

    public PlayerSpell2State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Animasyonu başlat
        player.anim.Play(SPELL2_ANIM_NAME);
        
        if (player.spellbookWeapon != null)
        {
            player.spellbookWeapon.PauseAnimation();
        }

        // Gecikmeli spawn
        player.StartCoroutine(DelayedSpawnFireSpell());
    }

    private IEnumerator DelayedSpawnFireSpell()
    {
        yield return new WaitForSeconds(SPAWN_DELAY);
        SpawnFireSpell();
    }

    public override void Update()
    {
        base.Update();
        player.SetZeroVelocity();

        // T tuşu bırakıldığında state'i bitir
        if (!player.playerInput.spell2Input)
        {
            stateMachine.ChangeState(player.idleState);
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
