using UnityEngine;

public class PlayerSpell1State : PlayerState
{
    private Vector3[] spawnPositions;
    private bool positionsCalculated = false;

    public PlayerSpell1State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
        spawnPositions = new Vector3[3];
    }

    public override void Enter()
    {
        base.Enter();
        CalculateSpawnPositions();
        positionsCalculated = true;
    }

    private void CalculateSpawnPositions()
    {
        float xOffset = 1f * player.facingdir;
        float startX = player.transform.position.x + xOffset;
        float spawnY = player.transform.position.y + 0.3f;

        for (int i = 0; i < 3; i++)
        {
            spawnPositions[i] = new Vector3(
                startX + (player.spellSpacing * i * player.facingdir),
                spawnY,
                player.transform.position.z
            );
        }
    }

    public override void Exit()
    {
        base.Exit();
        positionsCalculated = false;
    }

    public override void Update()
    {
        base.Update();

        player.SetZeroVelocity();

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public Vector3[] GetSpawnPositions()
    {
        return spawnPositions;
    }
}
