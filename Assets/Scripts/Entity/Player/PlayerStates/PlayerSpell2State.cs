using UnityEngine;
using System.Collections;

public class PlayerSpell2State : PlayerState
{
    private FireSpell currentFireSpell;
    private const string SPELL2_ANIM_NAME = "PlayerSpell2"; // Animator'daki state ismiyle aynı olmalı
   // private const string SPELL2_ARMOR_ANIM_NAME = "Armor_Spell2";
    private const float MAX_CHARGE_TIME = 1000f;
    private float currentChargeTime;
    private bool isSpellActive;

    public PlayerSpell2State(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // SkillManager kontrolü - Fire Spell açık mı?
        if (SkillManager.Instance != null)
        {
            // Beceri açık değilse state'den çık
            if (!SkillManager.Instance.IsSkillUnlocked("FireSpell"))
            {
                Debug.Log("Fire Spell is not unlocked!");
                stateMachine.ChangeState(player.idleState);
                return;
            }
            
            // Mana kontrolü
            float manaCost = SkillManager.Instance.GetSkillManaCost(SkillType.FireSpell);
            if (player.stats.currentMana < manaCost)
            {
                Debug.Log($"Not enough mana for Fire Spell! Required: {manaCost}, Current: {player.stats.currentMana}");
                stateMachine.ChangeState(player.idleState);
                return;
            }
        }
        else
        {
            // Eski yöntem - SkillManager yoksa basit mana kontrolü
            if (player.stats.currentMana < player.fireSpellManaDrainPerSecond)
            {
                Debug.Log($"Not enough mana for Fire Spell! Required: {player.fireSpellManaDrainPerSecond}, Current: {player.stats.currentMana}");
                stateMachine.ChangeState(player.idleState);
                return;
            }
        }
        
        currentChargeTime = 0f;
        isSpellActive = false;
        
        player.anim.Play(SPELL2_ANIM_NAME);
    }

    public override void Update()
    {
        base.Update();
        player.SetZeroVelocity();

        currentChargeTime += Time.deltaTime;
        
        // T tuşu basılı tutulduğunda mana tüketmeye başla
        if (UserInput.IsSpell2BeingPressed && !isSpellActive)
        {
            isSpellActive = true;
        }
        
        // Spell aktifse mana tüketmeye devam et
        if (isSpellActive)
        {
            float manaCost = player.fireSpellManaDrainPerSecond * Time.deltaTime;
            // Mana kontrolü
            if (!player.HasEnoughMana(manaCost))
            {
                CleanupSpell();
                stateMachine.ChangeState(player.idleState);
                return;
            }
            
            // Mana yeterliyse kullan
            player.UseMana(manaCost);
        }
        
        // T tuşu bırakıldığında veya max süre dolduğunda
        if (UserInput.WasSpell2Released || currentChargeTime >= MAX_CHARGE_TIME)
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
        if (player.basicArmor != null && player.basicArmor.animator != null)
        {
            player.basicArmor.animator.speed = 1;
        }
        
        if (player.spellbookWeapon?.animator != null)
        {
            player.spellbookWeapon.animator.speed = 1;
        }
        if (player.swordWeapon?.animator != null)
        {
            player.swordWeapon.animator.speed = 1;
        }

        if (player.burningSword?.animator != null)
        {
            player.burningSword.animator.speed = 1;
        }

        if (player.hammer?.animator != null)
        {
            player.hammer.animator.speed = 1;
        }
        
    }

    private void CleanupSpell()
    {
        Debug.Log($"CleanupSpell() çağrıldı - isSpellActive: {isSpellActive}, currentFireSpell null mu?: {currentFireSpell == null}");
        
        if (currentFireSpell != null)
        {
            Debug.Log($"currentFireSpell bulundu: {currentFireSpell.name}");
            GameObject.Destroy(currentFireSpell.gameObject);
            currentFireSpell = null;
            Debug.Log("currentFireSpell null yapıldı");
        }
        else
        {
            Debug.Log("currentFireSpell zaten null - sahnedeki fire spell'leri arıyorum...");
            
            // Sahnedeki tüm FireSpell'leri bul ve sil
            FireSpell[] fireSpells = Object.FindObjectsOfType<FireSpell>();
            Debug.Log($"Sahnede {fireSpells.Length} adet FireSpell bulundu");
            
            foreach (FireSpell spell in fireSpells)
            {
                if (spell != null && spell.gameObject != null)
                {
                    Debug.Log($"Fire spell siliniyor: {spell.name}");
                    GameObject.Destroy(spell.gameObject);
                }
            }
        }
        
        isSpellActive = false;
        Debug.Log($"isSpellActive false yapıldı: {isSpellActive}");
    }

    // Animation event'ten çağrılacak metod - PauseSpell2Animation çağrıldığında fire spell'i spawn et
    public void SpawnFireSpell()
    {
        Debug.Log($"SpawnFireSpell() çağrıldı - isSpellActive: {isSpellActive}, currentFireSpell null mu?: {currentFireSpell == null}");
        
        if (player.fireSpellPrefab != null && player.fireSpellPoint != null)
        {
            // SkillManager ile skill kullanımını işaretle
            if (SkillManager.Instance != null)
            {
                // Fire Spell'i kullanıldığı olarak işaretle (cooldown başlat)
                SkillManager.Instance.UseSkill(SkillType.FireSpell);
            }
            
            GameObject spellObj = GameObject.Instantiate(player.fireSpellPrefab, 
                player.fireSpellPoint.position, 
                player.fireSpellPoint.rotation
                );
            
            currentFireSpell = spellObj.GetComponent<FireSpell>();
            isSpellActive = true;
            
            Debug.Log($"Fire spell oluşturuldu - isSpellActive: {isSpellActive}, currentFireSpell null mu?: {currentFireSpell == null}");
            
            // Fire spell'in damage collider'ını aktif et
            if (currentFireSpell != null)
            {
                currentFireSpell.EnableDamage();
            }
        }
    }

    public float GetCurrentChargeTime()
    {
        return currentChargeTime;
    }
}
