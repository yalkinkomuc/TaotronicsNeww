using UnityEngine;

public class PlayerSpell2State : PlayerState
{
    private FireSpell currentFireSpell;

    public PlayerSpell2State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
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
        
        // State'ten çıkarken ateşi yok et
        if (currentFireSpell != null)
        {
            GameObject.Destroy(currentFireSpell.gameObject);
            currentFireSpell = null;
        }
    }

    private void SpawnFireSpell()
    {
        GameObject spellObj = GameObject.Instantiate(
            player.fireSpellPrefab, 
            player.fireSpellPoint.position, 
            player.fireSpellPoint.rotation,
            player.fireSpellPoint // Parent olarak spell point'i veriyoruz
        );
        
        currentFireSpell = spellObj.GetComponent<FireSpell>();
    }
}
