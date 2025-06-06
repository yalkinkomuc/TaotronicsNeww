using UnityEngine;

public class BaseUIPanel : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.AddPanel(gameObject);
        }
    }

    protected virtual void OnDisable()
    {
        if (UIInputBlocker.instance != null)
        {
            UIInputBlocker.instance.RemovePanel(gameObject);
        }
    }
} 