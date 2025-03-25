using UnityEngine;

public class Dummy : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayRandomHit()
    {
        int randomHit = Random.Range(0, 3);
        anim.SetInteger("HitIndex", randomHit);
    }
} 