using UnityEngine;

public class SceneManagement : MonoBehaviour
{

    [SerializeField] private Transform playerSpawnPoint;
    private Player player;
    
    void Start()
    {

        player = PlayerManager.instance.player.GetComponent<Player>();
        player.transform.position = playerSpawnPoint.position;
        player.transform.rotation = Quaternion.identity;
        
        player.gameObject.SetActive(true);
    }

   
}
