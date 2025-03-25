using UnityEngine;

public class Dummy : MonoBehaviour
{
    private Animator anim;
    private readonly int hitIndexHash = Animator.StringToHash("HitIndex");

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayRandomHit()
    {
        int randomHit = Random.Range(0, 3);
        anim.SetInteger(hitIndexHash, randomHit);
    }

    // Animation Event ile çağrılacak
    public void ResetHitIndex()
    {
        anim.SetInteger(hitIndexHash, -1); // veya 0, ama -1 daha güvenli
    }
} 