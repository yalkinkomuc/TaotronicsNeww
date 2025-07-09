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

            Destroy(gameObject);
        }
        
        
        
    }

    private void Start()
    {
        foreach (var panels in uiPanels)
        {
            panels.SetActive(false);
        }
    }
}
