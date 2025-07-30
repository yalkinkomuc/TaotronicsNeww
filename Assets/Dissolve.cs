using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Dissolve : MonoBehaviour
{
   [SerializeField] private float _dissolveTime = 0.75f;
   
   private SpriteRenderer[] _spriteRenderers;
   private Material[] _materials;
   
   private int _dissolveAmount = Shader.PropertyToID("_DissolveAmount");
   private int _verticalDissolveAmount = Shader.PropertyToID("_VerticalDissolveAmount");

   private void Start()
   {
      _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

      _materials = new Material[_spriteRenderers.Length];
      for (int i = 0; i < _spriteRenderers.Length; i++)
      {
         _materials[i] = _spriteRenderers[i].material;
      }
   }

   private void Update()
   {
      if (Keyboard.current.gKey.wasPressedThisFrame)
      {
         StartCoroutine(Vanish(true, false));
      }

      if (Keyboard.current.hKey.wasPressedThisFrame)
      {
         StartCoroutine(Appear(true, false));
      }
   }

   public void Vanish()
   {
      StartCoroutine(Vanish(true, false));
   }

   public void Reappear()
   {
      StartCoroutine(Appear(true, false));
   }

   private IEnumerator Vanish(bool useDissolve , bool useVerticalDissolve)
   {
      float elapsedTime = 0;
      while (elapsedTime < _dissolveTime)
      {
         elapsedTime += Time.deltaTime;

         float lerpedDissolve = Mathf.Lerp(0, 1.1f, (elapsedTime / _dissolveTime));
         float lerpedVerticalDissolve = Mathf.Lerp(0, 1.1f, (elapsedTime / _dissolveTime));


         for (int i = 0; i < _materials.Length; i++)
         {
            if(useDissolve)
             _materials[i].SetFloat(_dissolveAmount, lerpedDissolve);
            
            if(useVerticalDissolve)
             _materials[i].SetFloat(_verticalDissolveAmount, lerpedVerticalDissolve);
         }
         yield return null;
      }
   }
   
   private IEnumerator Appear(bool useDissolve , bool useVerticalDissolve)
   {
      float elapsedTime = 0;
      while (elapsedTime < _dissolveTime)
      {
         elapsedTime += Time.deltaTime;

         float lerpedDissolve = Mathf.Lerp(1.1f, 0, (elapsedTime / _dissolveTime));
         float lerpedVerticalDissolve = Mathf.Lerp(1.1f,0f, (elapsedTime / _dissolveTime));


         for (int i = 0; i < _materials.Length; i++)
         {
            if(useDissolve)
               _materials[i].SetFloat(_dissolveAmount, lerpedDissolve);
            
            if(useVerticalDissolve)
               _materials[i].SetFloat(_verticalDissolveAmount, lerpedVerticalDissolve);
         }
         yield return null;
      }
   }
}
