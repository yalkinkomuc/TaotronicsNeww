using System;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxEffectMultiplier;

    [SerializeField] private bool infiniteHorizontal;
    [SerializeField] private bool infiniteVertical;
    
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    private float textureUnitSizeX;
    private float textureUnitSizeY;


    private void Awake()
    {
        // Kamera referansını al
        FindCamera();
    }

    private void Start()
    {
        // Start'ta da kamera referansını kontrol et
        if (cameraTransform == null)
        {
            FindCamera();
        }
        
        if (cameraTransform != null)
        {
            lastCameraPosition = cameraTransform.position;

            Sprite sprite = GetComponent<SpriteRenderer>().sprite;
            Texture2D texture = sprite.texture;

            textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
            textureUnitSizeY = texture.height / sprite.pixelsPerUnit;
        }
    }
    
    private void FindCamera()
    {
        // Önce Camera.main'i dene
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            Debug.Log($"ParallaxBackground: Camera.main bulundu - {Camera.main.name}");
            return;
        }
        
        // Camera.main yoksa sahne içindeki kamerayı bul (WaterCamera hariç)
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        if (cameras.Length > 0)
        {
            // WaterCamera'yı filtrele
            foreach (Camera cam in cameras)
            {
                if (cam.name.Contains("Water") || cam.name.Contains("water"))
                {
                    continue; // WaterCamera'yı atla
                }
                
                // İlk geçerli kamerayı kullan
                cameraTransform = cam.transform;
                Debug.Log($"ParallaxBackground: Geçerli kamera bulundu - {cam.name} (WaterCamera filtrelendi, toplam {cameras.Length} kamera)");
                return;
            }
            
            // Eğer hiç geçerli kamera bulunamazsa ilkini kullan
            cameraTransform = cameras[0].transform;
            Debug.Log($"ParallaxBackground: Son çare olarak ilk kamera kullanıldı - {cameras[0].name}");
            return;
        }
        
        Debug.LogWarning("ParallaxBackground: Kamera bulunamadı!");
    }

    private void LateUpdate()
    {
        // Her frame'de kamera referansını kontrol et
        if (cameraTransform == null)
        {
            FindCamera();
            if (cameraTransform == null) return;
        }

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x , deltaMovement.y * parallaxEffectMultiplier.y);
        lastCameraPosition = cameraTransform.position;

        if (infiniteHorizontal)
        {
            // Kameranın sol tarafındaki sınıra göre spawn et
            float cameraLeftEdge = cameraTransform.position.x - (textureUnitSizeX * 0.5f);
            float spriteLeftEdge = transform.position.x - (textureUnitSizeX * 0.5f);
            
            // Eğer sprite kameranın sol tarafından çok uzaklaştıysa
            if (spriteLeftEdge < cameraLeftEdge - textureUnitSizeX)
            {
                // Sprite'ı kameranın sağ tarafına taşı
                transform.position = new Vector3(transform.position.x + textureUnitSizeX, transform.position.y);
            }
            // Eğer sprite kameranın sağ tarafından çok uzaklaştıysa
            else if (spriteLeftEdge > cameraLeftEdge + textureUnitSizeX)
            {
                // Sprite'ı kameranın sol tarafına taşı
                transform.position = new Vector3(transform.position.x - textureUnitSizeX, transform.position.y);
            }
        }

        if (infiniteVertical)
        {
            // Kameranın alt tarafındaki sınıra göre spawn et
            float cameraBottomEdge = cameraTransform.position.y - (textureUnitSizeY * 0.5f);
            float spriteBottomEdge = transform.position.y - (textureUnitSizeY * 0.5f);
            
            // Eğer sprite kameranın alt tarafından çok uzaklaştıysa
            if (spriteBottomEdge < cameraBottomEdge - textureUnitSizeY)
            {
                // Sprite'ı kameranın üst tarafına taşı
                transform.position = new Vector3(transform.position.x, transform.position.y + textureUnitSizeY);
            }
            // Eğer sprite kameranın üst tarafından çok uzaklaştıysa
            else if (spriteBottomEdge > cameraBottomEdge + textureUnitSizeY)
            {
                // Sprite'ı kameranın alt tarafına taşı
                transform.position = new Vector3(transform.position.x, transform.position.y - textureUnitSizeY);
            }
        }
       
    }
}


