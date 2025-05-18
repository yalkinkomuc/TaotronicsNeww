using UnityEngine;
using System.Collections.Generic;

public class PlayerSpell1State : PlayerState
{
    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private Vector3[] calculatedPositions;

    public PlayerSpell1State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
        calculatedPositions = new Vector3[3];
    }

    public override void Enter()
    {
        base.Enter();
        
        // SkillManager üzerinden mana kontrolü
        if (SkillManager.Instance != null)
        {
            float manaCost = SkillManager.Instance.GetSkillManaCost(SkillType.IceShard);
            
            // Yeterli mana yoksa state'i değiştir
            if (player.stats.currentMana < manaCost)
            {
                Debug.Log($"Not enough mana for Ice Shard! Required: {manaCost}, Current: {player.stats.currentMana}");
                stateMachine.ChangeState(player.idleState);
                return;
            }
            
            // Manayı kullan
            player.UseMana(manaCost);
        }
        else
        {
            // Eski yöntem - Mana kullanımı kontrolü
            if (!player.HasEnoughMana(player.spell1ManaCost))
            {
                Debug.Log($"Not enough mana for Spell1! Required: {player.spell1ManaCost}, Current: {player.stats.currentMana}");
                stateMachine.ChangeState(player.idleState);
                return;
            }
            
            // Manayı kullan
            player.UseMana(player.spell1ManaCost);
        }
        
        // Önce pozisyonları hesapla
        CalculateSpawnPositions();
        CheckValidSpawnPositions();
        
        // Eğer hiçbir geçerli buz parçası oluşturma pozisyonu yoksa, state'e girme
        if (validSpawnPositions.Count == 0)
        {
            Debug.Log("No valid positions for ice shards! Cancelling spell.");
            stateMachine.ChangeState(player.idleState);
            return;
        }
    }

    private void CalculateSpawnPositions()
    {
        float xOffset = 1f * player.facingdir;
        float startX = player.transform.position.x + xOffset;
        float spawnY = player.transform.position.y + 0.3f;

        for (int i = 0; i < 3; i++)
        {
            calculatedPositions[i] = new Vector3(
                startX + (player.spellSpacing * i * player.facingdir),
                spawnY,
                player.transform.position.z
            );
        }
    }

    private void CheckValidSpawnPositions()
    {
        validSpawnPositions.Clear();
        
        foreach (Vector3 position in calculatedPositions)
        {
            // Check if there's ground below this position
            bool hasGround = Physics2D.Raycast(
                position, 
                Vector2.down, 
                10f, 
                player.whatIsGround
            ).collider != null;
            
            // Only add positions that have ground beneath them
            if (hasGround)
            {
                validSpawnPositions.Add(position);
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

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public Vector3[] GetSpawnPositions()
    {
        return validSpawnPositions.ToArray();
    }
}
