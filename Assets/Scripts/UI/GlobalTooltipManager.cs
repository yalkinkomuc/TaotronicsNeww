using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Added for GraphicRaycaster and Image
using UnityEngine.EventSystems; // Added for GraphicRaycaster

/// <summary>
/// Global tooltip manager that handles all tooltips in the game
/// Uses a single tooltip panel for better performance
/// </summary>
public class GlobalTooltipManager : MonoBehaviour
{
    public static GlobalTooltipManager Instance { get; private set; }
    
    [Header("Tooltip UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipNameText;
    [SerializeField] private TextMeshProUGUI tooltipDescriptionText;
    [SerializeField] private TextMeshProUGUI tooltipDamageText;
    
    [Header("Tooltip Settings")]
    [SerializeField] private float showDelay = 0.3f;
    [SerializeField] private Vector3 offset = new Vector3(150, 50, 0);
    
    private Coroutine tooltipCoroutine;
    private PlayerStats playerStats;
    private bool isHovering = false;
    private EquipmentData currentEquipment;
    private Vector3 currentPosition;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Get PlayerStats reference
            playerStats = FindFirstObjectByType<PlayerStats>();
            
            // Setup tooltip panel to prevent mouse event conflicts
            SetupTooltipPanel();
            
            // Initially hide tooltip
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupTooltipPanel()
    {
        if (tooltipPanel != null)
        {
            // Disable all mouse event components on tooltip panel
            var graphicRaycaster = tooltipPanel.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = false;
            }
            
            // Disable raycast target on all child images
            var images = tooltipPanel.GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                image.raycastTarget = false;
            }
            
            // Disable raycast target on all child texts
            var texts = tooltipPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                text.raycastTarget = false;
            }
            
            // Disable any button components
            var buttons = tooltipPanel.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                button.interactable = false;
            }
        }
    }
    
    private void OnDestroy()
    {
        // Stop any running coroutines
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
        }
    }
    
    /// <summary>
    /// Show tooltip for equipment data
    /// </summary>
    /// <param name="equipment">Equipment data to show tooltip for</param>
    /// <param name="position">World position to show tooltip near</param>
    public void ShowTooltip(EquipmentData equipment, Vector3 position)
    {
        if (equipment == null || tooltipPanel == null) return;
        
        // Check if it's the same equipment and position (prevent unnecessary updates)
        if (isHovering && currentEquipment == equipment && 
            Vector3.Distance(currentPosition, position) < 10f)
        {
            return; // Already showing tooltip for this equipment
        }
        
        isHovering = true;
        currentEquipment = equipment;
        currentPosition = position;
        
        // Stop any existing coroutine
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
        }
        
        // Start new tooltip coroutine
        tooltipCoroutine = StartCoroutine(ShowTooltipWithDelay(equipment, position, showDelay));
    }
    
    /// <summary>
    /// Hide the tooltip
    /// </summary>
    public void HideTooltip()
    {
        isHovering = false;
        currentEquipment = null;
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        
        // Stop any running coroutines
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
            tooltipCoroutine = null;
        }
    }
    
    /// <summary>
    /// Show tooltip with delay to prevent flickering
    /// </summary>
    private IEnumerator ShowTooltipWithDelay(EquipmentData equipment, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Only show tooltip if still hovering and equipment hasn't changed
        if (isHovering && currentEquipment == equipment && tooltipPanel != null)
        {
            UpdateTooltipContent(equipment, position);
            tooltipPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Update tooltip content and position
    /// </summary>
    private void UpdateTooltipContent(EquipmentData equipment, Vector3 position)
    {
        if (equipment == null || tooltipPanel == null) return;
        
        // Set tooltip position using screen coordinates for better stability
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
        tooltipPanel.transform.position = screenPosition + offset;
        
        // Set tooltip content
        if (tooltipNameText != null)
        {
            tooltipNameText.text = equipment.itemName;
        }
        
        if (tooltipDescriptionText != null)
        {
            tooltipDescriptionText.text = equipment.description;
        }
        
        // Set damage text for weapons
        if (tooltipDamageText != null)
        {
            if (equipment is WeaponData weaponData)
            {
                string damageRange = CalculateWeaponDamageRange(weaponData, false);
                tooltipDamageText.text = $"Hasar: {damageRange}";
            }
            else
            {
                tooltipDamageText.text = "";
            }
        }
    }
    
    /// <summary>
    /// Calculate weapon damage range using WeaponDamageManager (same as BlacksmithUI)
    /// </summary>
    private string CalculateWeaponDamageRange(WeaponData weapon, bool isNextLevel)
    {
        if (weapon == null || playerStats == null) return "0";
        
        if (isNextLevel)
        {
            return WeaponDamageManager.GetWeaponDamageRangeStringNextLevel(weapon.weaponType, playerStats);
        }
        else
        {
            return WeaponDamageManager.GetWeaponDamageRangeString(weapon.weaponType, playerStats);
        }
    }
    
    /// <summary>
    /// Update PlayerStats reference (called when player spawns)
    /// </summary>
    public void UpdatePlayerStats(PlayerStats stats)
    {
        playerStats = stats;
    }
    
    /// <summary>
    /// Check if mouse is over tooltip panel
    /// </summary>
    public bool IsMouseOverTooltip()
    {
        if (tooltipPanel == null || !tooltipPanel.activeInHierarchy) return false;
        
        // Get mouse position in screen coordinates
        Vector3 mousePosition = Input.mousePosition;
        
        // Convert tooltip panel bounds to screen coordinates
        RectTransform rectTransform = tooltipPanel.GetComponent<RectTransform>();
        if (rectTransform == null) return false;
        
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        // Convert corners to screen coordinates
        for (int i = 0; i < 4; i++)
        {
            corners[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[i]);
        }
        
        // Check if mouse is within tooltip bounds
        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        
        return mousePosition.x >= minX && mousePosition.x <= maxX && 
               mousePosition.y >= minY && mousePosition.y <= maxY;
    }
} 