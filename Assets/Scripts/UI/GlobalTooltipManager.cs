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
    
    [Header("Panel Management")]
    [SerializeField] private GameObject panelToHideWhenSelectionOpens;
    
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
    
    public void ShowTooltip(EquipmentData equipment, Vector3 position)
    {
        // Don't show tooltip if equipment selection panel is open
        if (UI_EquipmentSelectionPanel.Instance != null && 
            UI_EquipmentSelectionPanel.Instance.IsPanelOpen())
        {
            Debug.Log("Equipment selection panel is open, not showing tooltip");
            return;
        }
        
        if (equipment == null || tooltipPanel == null) return;
        
        // Check if it's the same equipment and position (prevent unnecessary updates)
        if (isHovering && currentEquipment == equipment && 
            Vector3.Distance(currentPosition, position) < 10f)
        {
            // Same equipment, but update tooltip content in case equipment data changed
            UpdateTooltipContent(equipment, position);
            return;
        }
        
        Debug.Log($"Showing tooltip for: {equipment.itemName}");
        
        // Reset hover state if equipment changed
        if (currentEquipment != equipment)
        {
            isHovering = false;
            Debug.Log("Equipment changed, resetting hover state");
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
        yield return new WaitForSecondsRealtime(delay);
        
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
        
        // Get mouse position
        Vector3 mousePosition = Input.mousePosition;
        
        // Calculate smart offset based on screen position
        Vector3 smartOffset = CalculateSmartOffset(position);
        
        // Set tooltip position using mouse position with smart offset
        tooltipPanel.transform.position = mousePosition + smartOffset;
        
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
                // Special text for secondary weapons
                if (weaponData.weaponType == WeaponType.Spellbook)
                {
                    tooltipDamageText.text = "Scales with Mind";
                }
                else if (weaponData.weaponType == WeaponType.Boomerang)
                {
                    tooltipDamageText.text = "Scales with Might";
                }
                else
                {
                    // Normal damage calculation for other weapons
                    string damageRange = CalculateWeaponDamageRange(weaponData, false);
                    tooltipDamageText.text = $"Hasar: {damageRange}";
                }
            }
            else
            {
                tooltipDamageText.text = "";
            }
        }
    }
    
    /// <summary>
    /// Calculate smart offset based on object's screen position
    /// </summary>
    private Vector3 CalculateSmartOffset(Vector3 objectPosition)
    {
        // Convert object world position to screen position
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, objectPosition);
        
        // Get screen center
        float screenCenterX = Screen.width * 0.5f;
        
        // If object is on the left side of screen, show tooltip on the left
        if (screenPosition.x < screenCenterX)
        {
            return new Vector3(-offset.x, offset.y, 0); // Left side offset
        }
        else
        {
            return new Vector3(offset.x, offset.y, 0); // Right side offset
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
    /// Force reset all hover states - called when equipment selection panel closes
    /// </summary>
    public void ResetAllHoverStates()
    {
        isHovering = false;
        currentEquipment = null;
        
        // Hide tooltip immediately
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
        
        Debug.Log("[GlobalTooltipManager] All hover states reset");
    }
    
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