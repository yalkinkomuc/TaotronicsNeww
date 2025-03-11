using UnityEngine;

public class VFXAnimTrigger : MonoBehaviour
{
    // Animation Event ile çağrılacak
    public void CompleteVFX()
    {
        VFXManager.Instance.OnVFXComplete(gameObject);
    }
} 