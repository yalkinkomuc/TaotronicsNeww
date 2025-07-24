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

    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;

    private Vector2 startingTrackedObjectOffset;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                currentCamera = allVirtualCameras[i];
                framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        normYPanAmount = framingTransposer.m_YDamping;

        startingTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
    }

    public void PanCameraOnContact(float _panDistance, float _panTime,
        CustomInspectorObjects.PanDirection _panDirection, bool _panToStartingPos)
    {
        panCameraCoroutine = StartCoroutine(PanCamera(_panDistance, _panTime, _panDirection, _panToStartingPos));
    }

    private IEnumerator PanCamera(float _panDistance, float _panTime, CustomInspectorObjects.PanDirection _panDirection,
        bool _panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startPos = Vector2.zero;

        if (!_panToStartingPos)
        {
            switch (_panDirection)
            {
                case CustomInspectorObjects.PanDirection.Up:
                    endPos =Vector2.up;
                    break;
                case CustomInspectorObjects.PanDirection.Down:
                    endPos =Vector2.down;
                    break;
                case CustomInspectorObjects.PanDirection.Right:
                    endPos =Vector2.right;
                    break;
                case CustomInspectorObjects.PanDirection.Left:
                    endPos =Vector2.left;
                    break;
                default:
                    break;
            }

            endPos *= _panDistance;
            
            startPos *= startingTrackedObjectOffset;

            endPos += startPos;

        }

        else
        {
            startPos = framingTransposer.m_TrackedObjectOffset;
            endPos = startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while (elapsedTime < _panTime)
        {
            elapsedTime += Time.deltaTime;
            
            Vector3 panLerp = Vector3.Lerp(startPos, endPos, (elapsedTime / _panTime));
            framingTransposer.m_TrackedObjectOffset = panLerp;
            
            yield return null;
        }
       
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

    
}
