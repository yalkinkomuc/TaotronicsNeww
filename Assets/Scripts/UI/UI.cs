using System;
using UnityEngine;

public class UI : BaseUIPanel
{
    
    private static UI instance;
    
    public GameObject[] uiPanels;


    private void Awake()
    {
        if (instance == null )
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Debug.LogWarning("More than one instance of UI");
            Destroy(gameObject);
        }
    }
}
