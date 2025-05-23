using UnityEngine;

public class PlayerAirPushState : PlayerState
{
    private float airPushManaCost;
    private GameObject airPushPrefab;
    private Transform airPushSpawnPoint;

    public PlayerAirPushState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Get mana cost from player
        airPushManaCost = player.airPushManaCost;
        airPushPrefab = player.airPushPrefab;
        airPushSpawnPoint = player.airPushSpawnPoint;
        
        // Check if player has enough mana
        if (!player.HasEnoughMana(airPushManaCost))
        {
            // Not enough mana, return to idle state
            stateMachine.ChangeState(player.idleState);
            return;
        }
        
        // Use mana
        player.UseMana(airPushManaCost);
        
        // Spawn the air push effect
        if (airPushPrefab != null && airPushSpawnPoint != null)
        {
            GameObject airPushObj = GameObject.Instantiate(airPushPrefab, airPushSpawnPoint.position, Quaternion.identity);
            AirPush airPush = airPushObj.GetComponent<AirPush>();
            
            if (airPush != null)
            {
                // Initialize the air push with player's stats and direction
                airPush.Initialize(player.facingdir, player.stats);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        
        player.SetZeroVelocity();
        
        // If animation is finished, return to idle state
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
