using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Player player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            
            // Player'ı da DontDestroyOnLoad yap
            if (player != null)
            {
                DontDestroyOnLoad(player.gameObject);
            }
            else
            {
                Debug.LogError("PlayerManager: Player referansı atanmamış!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        ClampPositionToArena();
    }

    void ClampPositionToArena()
    {
        if (BossArenaManager.ArenaCollider != null)
        {
            Vector3 newPos = transform.position;
            
            // X pozisyonunu sınırla
            newPos.x = Mathf.Clamp(newPos.x, 
                BossArenaManager.ArenaCollider.bounds.min.x + 1f, 
                BossArenaManager.ArenaCollider.bounds.max.x - 1f);
            
            // Y pozisyonunu sınırla
            newPos.y = Mathf.Clamp(newPos.y, 
                BossArenaManager.ArenaCollider.bounds.min.y + 1f, 
                BossArenaManager.ArenaCollider.bounds.max.y - 1f);
            
            transform.position = newPos;
        }
    }
}
