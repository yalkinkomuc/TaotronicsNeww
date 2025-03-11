using System.Collections;
using UnityEngine;

public class PlayerDashState : PlayerState
{
    
    public PlayerDashState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = player.dashDuration;
        
        
        player.DashCoroutineController();
        
        
        
    }

    public override void Exit()
    {
        base.Exit();
        
        player.SetVelocity(0,rb.linearVelocity.y);
        player.ExitGhostMode();
    }

    public override void Update()
    {
        base.Update();
        
        player.SetVelocity(player.dashSpeed*player.dashDirection,0);
        
        player.EnterGhostMode();

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.idleState);
            
        }
        
        
    }

    public IEnumerator SpawnDashEffects()
    {
       
        for (int i = 0; i < player.effectCount; i++)
        {
            GameObject effect = Object.Instantiate(player.dashEffectPrefab,player.anim.transform.position,Quaternion.identity); 
            
           

            if (player.dashDirection == -1)
            {
                effect.transform.rotation = Quaternion.Euler(0,180,0);
               
            }
            
            Object.Destroy(effect, 0.5f); // Efekti kısa bir süre sonra yok et
            
            yield return new WaitForSeconds(player.spawnInterval);
        }
    }
}
