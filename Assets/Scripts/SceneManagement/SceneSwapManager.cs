using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
   public static SceneSwapManager instance;

   private static bool loadFromDoor;

   private GameObject Player;
   private Player playerScript;
   private Collider2D playerCollider;
   private Collider2D doorCollider;
   private Vector3 playerSpawnPosition;
   
   
   private DoorTriggerInteraction.DoorToSpawnAt doorToSpawnTo;

   private void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
      
      Player = GameObject.FindWithTag("Player");
      playerScript = Player.GetComponent<Player>();
      playerCollider = Player.GetComponent<Collider2D>();
      
      
   }

   private void OnEnable()
   {
      SceneManager.sceneLoaded += OnSceneLoaded;
   }

   private void OnDisable()
   {
      SceneManager.sceneLoaded -= OnSceneLoaded;
   }

   public static void SwapSceneFromDoorUse(SceneField myScene, DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt)
   {
      loadFromDoor = true;
      instance.StartCoroutine(instance.FadeOutThenChangeScene(myScene, doorToSpawnAt));
   }
   

   private IEnumerator FadeOutThenChangeScene(SceneField myScene,
      DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt = DoorTriggerInteraction.DoorToSpawnAt.None)
   {
      
      SceneFadeManager.instance.StartFadeOut();

      while (SceneFadeManager.instance.isFadingOut)
      {
         yield return null;
      }
      
      doorToSpawnTo = doorToSpawnAt;
      SceneManager.LoadScene(myScene);
   }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
      SceneFadeManager.instance.StartFadeIn();
      playerScript.ShowWeapons();

      if (loadFromDoor)
      {
         FindDoor(doorToSpawnTo);
         Player.transform.position = playerSpawnPosition;
         loadFromDoor = false;
      }
   }

   private void FindDoor(DoorTriggerInteraction.DoorToSpawnAt doorSpawnNumber)
   {
      DoorTriggerInteraction[] doors =  FindObjectsOfType<DoorTriggerInteraction>();

      for(int i = 0; i < doors.Length; i++)
      {
         if (doors[i].CurrentDoorPosition == doorSpawnNumber)
         {
            doorCollider = doors[i].gameObject.GetComponent<Collider2D>();
            
            CalculateSpawnPosition();
            return;
         }
      }
   }

   private void CalculateSpawnPosition()
   {
      float colliderHeight = playerCollider.bounds.extents.y;
      playerSpawnPosition = doorCollider.transform.position + new Vector3(0f, colliderHeight * 0f, 0f);
   }
}
