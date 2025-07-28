using System.Collections;
using UnityEngine;

public class PlayerElectricDashState : PlayerState
{
    private GameObject electricDashPrefab;
    private Transform electricDashSpawnPoint;
    private float electricDashSpeed;
    private float electricDashDistance;
    private float electricDashDuration;
    
    // Smooth dash için yeni değişkenler
    private float currentDashSpeed;
    private float accelerationRate = 80f; // Daha hızlı hızlanma
    private float decelerationRate = 80f; // Daha hızlı yavaşlama
    private bool isAccelerating = true; 
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float dashProgress = 0f;
    
    public PlayerElectricDashState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) 
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Setup dash parameters
        electricDashSpeed = player.electricDashSpeed;
        electricDashDuration = player.electricDashDuration;
        electricDashDistance = player.electricDashDistance;
        electricDashPrefab = player.electricDashPrefab;
        electricDashSpawnPoint = player.electricDashSpawnPoint;
        
        // Smooth dash için başlangıç değerleri
        currentDashSpeed = 0f;
        isAccelerating = true;
        dashProgress = 0f;
        
        // Başlangıç ve hedef pozisyonları hesapla
        startPosition = player.transform.position;
        Vector2 direction = new Vector2(player.dashDirection, 0);
        targetPosition = startPosition + direction * electricDashDistance;
        
        // Engelleri kontrol et
        RaycastHit2D hit = Physics2D.Raycast(
            startPosition, 
            direction, 
            electricDashDistance, 
            player.whatIsGround
        );
        
        if (hit.collider != null)
        {
            targetPosition = hit.point - direction * 0.5f;
        }
        
        // Set state timer
        stateTimer = electricDashDuration;
        
        // Ghost mode for invulnerability
        player.EnterGhostMode();
        
        // Spawn visual effects
        SpawnElectricDashEffects();
        
        // Use mana
        if (SkillManager.Instance != null)
        {
            float manaCost = SkillManager.Instance.GetSkillManaCost(SkillType.ElectricDash);
            player.UseMana(manaCost);
        }
        
        // Start the dash coroutine
        player.StartCoroutine(DoElectricDash());
    }

    public override void Exit()
    {
        base.Exit();
        
        // Stop velocity and exit ghost mode
        player.SetVelocity(0, rb.linearVelocity.y);
        player.ExitGhostMode();
        
        // Clear any enemies hit during this dash
        player.ClearHitEntities();
    }

    public override void Update()
    {
        base.Update();
        
        // Smooth dash movement
        UpdateSmoothDashMovement();
        
        // Check if dash timer expired
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
    
    private void UpdateSmoothDashMovement()
    {
        // Dash progress'i hesapla
        dashProgress += Time.deltaTime / electricDashDuration;
        
        // Acceleration ve deceleration
        if (isAccelerating)
        {
            currentDashSpeed += accelerationRate * Time.deltaTime;
            if (currentDashSpeed >= electricDashSpeed)
            {
                currentDashSpeed = electricDashSpeed;
                isAccelerating = false;
            }
        }
        else
        {
            // Dash'in son %25'inde yavaşlamaya başla
            if (dashProgress > 0.75f)
            {
                currentDashSpeed -= decelerationRate * Time.deltaTime;
                if (currentDashSpeed < 0)
                    currentDashSpeed = 0;
            }
        }
        
        // Velocity'yi uygula
        float direction = player.dashDirection;
        player.SetVelocity(currentDashSpeed * direction, 0);
    }

    private IEnumerator DoElectricDash()
    {
        // Spawn electric effect at the midpoint of dash
        if (electricDashPrefab != null)
        {
            Vector2 midPoint = Vector2.Lerp(startPosition, targetPosition, 0.5f);
            GameObject electricEffect = Object.Instantiate(
                electricDashPrefab, 
                midPoint, 
                Quaternion.identity
            );
            
            // Set effect direction based on player direction
            if (player.dashDirection == -1)
            {
                electricEffect.transform.localScale = new Vector3(
                    -electricEffect.transform.localScale.x,
                    electricEffect.transform.localScale.y,
                    electricEffect.transform.localScale.z
                );
            }
        }
        
        // Spawn shockwave effect at the start position
        if (player.shockwavePrefab != null)
        {
            GameObject shockwaveEffect = Object.Instantiate(
                player.shockwavePrefab,
                startPosition,
                Quaternion.identity
            );
            
            // Call the shockwave effect
            ShockWaveManager shockwaveManager = shockwaveEffect.GetComponent<ShockWaveManager>();
            if (shockwaveManager != null)
            {
                shockwaveManager.CallShockWave();
            }
        }
        
        // Small delay for effect to appear before teleporting
        yield return new WaitForSeconds(0.05f);
        
        // Teleport to destination
        player.transform.position = targetPosition;
        
        // Small delay before ending the dash
        yield return new WaitForSeconds(0.1f);
        
        // End the dash
        stateTimer = -1;
    }

    private void SpawnElectricDashEffects()
    {
        // Similar to regular dash effects but with electric visuals
        for (int i = 0; i < player.effectCount; i++)
        {
            GameObject effect = Object.Instantiate(
                player.dashEffectPrefab, 
                player.transform.position, 
                Quaternion.identity
            );
            
            // Apply electric color (yellow tint)
            SpriteRenderer renderer = effect.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = new Color(1f, 1f, 0.5f, renderer.color.a);
            }
            
            // Flip if needed
            if (player.dashDirection == -1)
            {
                effect.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            
            Object.Destroy(effect, 0.5f);
        }
    }
}
