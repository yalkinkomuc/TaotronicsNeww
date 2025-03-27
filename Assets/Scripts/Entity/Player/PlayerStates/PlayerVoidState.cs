using UnityEngine;
using System.Collections;

public class PlayerVoidState : PlayerState
{
    private Enemy targetEnemy;
    private const float detectionRadius = 5f; // Void skill menzili
    private const int slashCount = 3; // Vuruş sayısı
    private const float slashInterval = 0.3f; // Vuruşlar arası süre
     // Kılıç efekti prefabı

    public PlayerVoidState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
        // Prefabı yükle - prefabı Resources klasörüne eklemelisin
        
    }

    public override void Enter()
    {
        base.Enter();
        
        // Önce en yakın düşmanı bul
        targetEnemy = FindClosestEnemy();
        
        if (targetEnemy != null)
        {
            // Düşman bulundu, mana kontrolü yap
            if (!player.HasEnoughMana(player.voidSkillManaCost))
            {
                stateMachine.ChangeState(player.idleState);
                return;
            }
            
            // Mana kullan
            player.UseMana(player.voidSkillManaCost);
            
            // Void skill başlat
            player.StartCoroutine(VoidSkillSequence());
        }
        else
        {
            // Düşman bulunamadı, idle state'e dön
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        // State'ten çıkarken bool'u false yap
        player.anim.SetBool("VoidDisappear", false);
        player.anim.SetBool("VoidReappear", false);
        
        player.ExitGhostMode();
    }

    private Enemy FindClosestEnemy()
    {
        // Belirli mesafedeki tüm düşmanları bul
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, detectionRadius, player.passableEnemiesLayerMask);
        
        Enemy closestEnemy = null;
        float closestDistance = detectionRadius;
        
        foreach (var collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                float distance = Vector2.Distance(player.transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }
        
        return closestEnemy;
    }

    private IEnumerator VoidSkillSequence()
    {
        // 1. Void disappear animasyonu oynat
        player.anim.SetBool("VoidDisappear", true);
        
        // Kaybolma animasyonu için bir süre bekle
        yield return new WaitForSeconds(0.5f);
        
        // Disappear animasyonu bitince bool'u false yap
        player.anim.SetBool("VoidDisappear", false);
        
        // 2. Düşmanın üzerine void slash effect'leri oluştur
        for (int i = 0; i < slashCount; i++)
        {
            // Kılıç efekti oluştur
            GameObject slashEffect = Object.Instantiate(
                player.voidSlashPrefab, 
                targetEnemy.transform.position + new Vector3(0, 0.5f, 0), 
                Quaternion.identity
            );
            
            // Düşmana hasar ver
            float damage = player.stats.baseDamage.GetValue() * 0.8f; // Her slash temel hasarın %80'i
            targetEnemy.GetComponent<CharacterStats>().TakeDamage(damage);
            
            // Kılıç efektini belirli bir süre sonra yok et
            Object.Destroy(slashEffect, 0.5f);
            
            // Sonraki vuruş için bekle
            yield return new WaitForSeconds(slashInterval);
        }
        
        // 3. Oyuncuyu düşmanın karşı tarafında konumlandır
        RepositionPlayerBehindEnemy();
        
        // 4. Reappear animasyonu oynat
        player.anim.SetBool("VoidReappear", true);
        
        // Bir süre bekle 
        yield return new WaitForSeconds(0.5f);
        
        // Reappear animasyonu bitince bool'u false yap
        player.anim.SetBool("VoidReappear", false);
        
        // Idle state'e dön
        stateMachine.ChangeState(player.idleState);
    }
    
    private void RepositionPlayerBehindEnemy()
    {
        // Düşmanın baktığı yönün tam tersine oyuncuyu konumlandır
        Vector2 repositionDirection = new Vector2(-targetEnemy.facingdir, 0).normalized;
        Vector3 newPosition = targetEnemy.transform.position + new Vector3(repositionDirection.x * 5f, 0, 0);
        
        // Player'ın Y pozisyonunu yerden yükselt
        newPosition.y = player.groundCheck.position.y;
        
        // Oyuncuyu yeni pozisyona taşı
        player.transform.position = newPosition;
        
        // Oyuncuyu düşmana baktır
        int newFacingDir = targetEnemy.transform.position.x > player.transform.position.x ? 1 : -1;
        if (player.facingdir != newFacingDir)
            player.Flip();
    }

    public override void Update()
    {
        base.Update();
        
        player.EnterGhostMode();
        player.SetZeroVelocity();
        // Bu state'de sadece coroutine çalıştığı için Update'te ekstra bir şey yapmıyoruz
    }
}
