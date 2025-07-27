using System;
using System.Collections;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;


public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [Header("Controls for lerping the Y damping during player jump/fall")] 
    
    [SerializeField] private CinemachineVirtualCamera[] allVirtualCameras;
    
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -15f;
    
    public bool isLerpingYDamping {get; private set;}
    public bool lerpedFromPlayerFalling  {get;  set;}

    private Coroutine lerpYPanCoroutine;
    private Coroutine panCameraCoroutine;
    private Coroutine shakeCoroutine;

    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;
    private CinemachineConfiner2D cinemachineConfiner;

    private float normYPanAmount;

    private Vector2 startingTrackedObjectOffset;
    private Vector2 originalTrackedObjectOffset; // Shake için orijinal pozisyon

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

       
        FindCinemachineComponents();
        SetupCameraComponents();
    }

    private void FindCinemachineComponents()
    {
        // Virtual Camera'yı bul
        currentCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (currentCamera != null)
        {
            // CinemachineConfiner bileşenini al
            cinemachineConfiner = currentCamera.GetComponent<CinemachineConfiner2D>();
            if (cinemachineConfiner == null)
            {
                Debug.LogWarning("CinemachineConfiner bileşeni bulunamadı! Virtual Camera'ya eklenmemiş olabilir.");
            }
        }
        else
        {
            Debug.LogWarning("CinemachineVirtualCamera bulunamadı!");
        }
    }

    private void SetupCameraComponents()
    {
        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];
                framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        if (framingTransposer != null)
        {
            normYPanAmount = framingTransposer.m_YDamping;
            startingTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
            originalTrackedObjectOffset = startingTrackedObjectOffset; // Shake için kaydet
        }
    }

    public void UpdateSceneBoundary()
    {
        if (cinemachineConfiner == null)
        {
            Debug.LogWarning("CinemachineConfiner null! SceneBoundary güncellenemedi.");
            return;
        }

        // Sahnedeki SceneBoundary'yi bul
        GameObject sceneBoundary = GameObject.FindGameObjectWithTag("SceneBoundary");
        if (sceneBoundary != null)
        {
            // SceneBoundary'nin Collider2D bileşenini al
            Collider2D boundaryCollider = sceneBoundary.GetComponent<Collider2D>();
            if (boundaryCollider != null)
            {
                // CinemachineConfiner'ın BoundingShape2D'sini güncelle
                cinemachineConfiner.m_BoundingShape2D = boundaryCollider;
                Debug.Log($"SceneBoundary güncellendi: {sceneBoundary.name}");
            }
            else
            {
                Debug.LogWarning($"SceneBoundary ({sceneBoundary.name}) Collider2D bileşeni bulunamadı!");
            }
        }
        else
        {
            Debug.LogWarning("SceneBoundary tag'li obje bulunamadı! Bu sahne için SceneBoundary yok olabilir.");
        }
    }

    public void PanCameraOnContact(float _panDistance, float _panTime,
        CustomInspectorObjects.PanDirection _panDirection, bool _panToStartingPos)
    {
        // Önceki bir pan işlemi devam ediyorsa durdur
        if (panCameraCoroutine != null)
        {
            StopCoroutine(panCameraCoroutine);
        }

        panCameraCoroutine = StartCoroutine(PanCamera(_panDistance, _panTime, _panDirection, _panToStartingPos));
    }

     private IEnumerator PanCamera(float _panDistance, float _panTime, CustomInspectorObjects.PanDirection _panDirection, bool _panToStartingPos)
{
    Vector2 endPos = Vector2.zero;
    Vector2 startPos = framingTransposer.m_TrackedObjectOffset; 

    // Easing tipini tanımlamak için bir değişken
    LeanTweenType selectedEaseType;

    if (!_panToStartingPos)
    {
        // ... (Aşağı pan için endPos hesaplamaları aynı kalır)
        Vector2 directionVector = Vector2.zero;
        switch (_panDirection)
        {
            case CustomInspectorObjects.PanDirection.Up:
                directionVector = Vector2.up;
                break;
            case CustomInspectorObjects.PanDirection.Down:
                directionVector = Vector2.down;
                break;
            case CustomInspectorObjects.PanDirection.Right:
                directionVector = Vector2.right;
                break;
            case CustomInspectorObjects.PanDirection.Left:
                directionVector = Vector2.left;
                break;
            default:
                break;
        }

        endPos = startPos + (directionVector * _panDistance);
        selectedEaseType = LeanTweenType.easeInOutQuad; // Aşağı pan için hızlı başlangıç, yavaşlama
    }
    else // _panToStartingPos ise (yani yukarı çıkış)
    {
        endPos = startingTrackedObjectOffset;
        selectedEaseType = LeanTweenType.easeInOutQuad; // Yukarı çıkış için yavaş başla, hızlan, yavaşla (daha yumuşak yerleşim)
        // Ya da denemek için: selectedEaseType = LeanTweenType.easeInQuad;
    }

    bool tweenCompleted = false;

    LeanTween.value(gameObject, startPos, endPos, _panTime)
        .setOnUpdate((Vector2 val) => {
            framingTransposer.m_TrackedObjectOffset = val;
        })
        .setEase(selectedEaseType) // Belirlenen easing tipini kullan
        .setOnComplete(() => {
            tweenCompleted = true;
        });

    while (!tweenCompleted)
    {
        yield return null;
    }
    Debug.Log("Kamera pan işlemi LeanTween ile tamamlandı!");
}

    public void LerpYDamping(bool isPlayerFalling)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        isLerpingYDamping = true;

        float startDampAmount = framingTransposer.m_YDamping;
        float endDampAmount = 0f;
        isLerpingYDamping = false;

        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            lerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
        }
        
        float elapsedTime = 0f;

        while (elapsedTime < fallPanTime)
        {
            elapsedTime += Time.deltaTime;
            
            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / fallPanTime));
            framingTransposer.m_YDamping = lerpedPanAmount;
            
            yield return null;
        }
        
        isLerpingYDamping = false;
        
    }

    /// <summary>
    /// Kamera titreşimi efekti - LeanTween ile hafif shake
    /// </summary>
    /// <param name="intensity">Titreşim yoğunluğu (0.1f - 0.5f arası önerilir)</param>
    /// <param name="duration">Titreşim süresi</param>
    /// <param name="frequency">Titreşim frekansı (saniyede kaç kez)</param>
    public void ShakeCamera(float intensity = 0.2f, float duration = 0.3f, float frequency = 20f)
    {
        // Eğer zaten shake devam ediyorsa, yeni shake'i başlatma
        if (shakeCoroutine != null)
        {
            return;
        }
        
        shakeCoroutine = StartCoroutine(ShakeCameraCoroutine(intensity, duration, frequency));
    }

    private IEnumerator ShakeCameraCoroutine(float intensity, float duration, float frequency)
    {
        if (framingTransposer == null) yield break;

        float elapsedTime = 0f;
        float shakeInterval = 1f / frequency; // Her shake arasındaki süre
        
        Vector2 originalOffset = framingTransposer.m_TrackedObjectOffset;
        
        while (elapsedTime < duration)
        {
            // Rastgele offset hesapla
            Vector2 randomOffset = new Vector2(
                UnityEngine.Random.Range(-intensity, intensity),
                UnityEngine.Random.Range(-intensity, intensity)
            );
            
            // LeanTween ile yumuşak geçiş
            LeanTween.value(gameObject, (Vector2)framingTransposer.m_TrackedObjectOffset, originalOffset + randomOffset, shakeInterval * 0.5f)
                .setOnUpdate((Vector2 val) => {
                    framingTransposer.m_TrackedObjectOffset = val;
                })
                .setEase(LeanTweenType.easeOutQuad);
            
            elapsedTime += shakeInterval;
            yield return new WaitForSeconds(shakeInterval);
        }
        
        // Orijinal pozisyona yumuşak dönüş
        LeanTween.value(gameObject, (Vector2)framingTransposer.m_TrackedObjectOffset, originalOffset, 0.1f)
            .setOnUpdate((Vector2 val) => {
                framingTransposer.m_TrackedObjectOffset = val;
            })
            .setEase(LeanTweenType.easeOutQuad);
        
        shakeCoroutine = null;
    }

    
}
