using System;
using UnityEngine;

public class Trap : MonoBehaviour
{
    private void OnCollisionStay2D(Collision2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();

        if (player != null)
        {
            player.Damage();
        }
    }
}
