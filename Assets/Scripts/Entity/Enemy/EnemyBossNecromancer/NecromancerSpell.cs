using System;
using UnityEngine;

public class NecromancerSpell : MonoBehaviour
{
   

    private CapsuleCollider2D capsuleCollider2D;
    private Animator anim;

    private void Awake()
    {
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        capsuleCollider2D.enabled = false; // Başlangıçta collider kapalı
    }

    
    public void ActivateCollider()
    {
       capsuleCollider2D.enabled = true;
    }

    
    public void DeactivateCollider()
    {
        capsuleCollider2D.enabled = false;
    }

    // Animasyon event ile çağrılacak metod
    public void DestroySpell()
    {
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player tag'i bulundu!");
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                //Debug.Log("Player component'i bulundu, hasar veriliyor");
                player.Damage(); 
            }
        }
    }
}
