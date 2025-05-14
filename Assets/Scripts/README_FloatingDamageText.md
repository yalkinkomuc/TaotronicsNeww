# Floating Damage Text System

This system allows you to show damage numbers (or any text) floating above game objects when they take damage.

## Setup Instructions

### 1. Create the Floating Text Prefab

1. In Unity, create a new GameObject (Create > UI > Text - TextMeshPro)
2. Name it "FloatingTextPrefab"
3. Add the `FloatingText.cs` script to it
4. Configure the TextMeshPro component:
   - Font Size: 8
   - Font Style: Bold
   - Color: White
   - Alignment: Center
   - Wrapping: Disabled
   - Auto Size: Disabled
5. Adjust the FloatingText component settings:
   - Destroy Time: 1.0
   - Offset: 0, 1.5, 0
   - Randomize Intensity: 0.5, 0, 0
   - Move Up Speed: 1.0
6. Drag this to your Project panel to create a prefab

### 2. Set Up the Floating Text Manager

#### Option A: Manual Setup
1. Create an empty GameObject named "FloatingTextManager"
2. Add the `FloatingTextManager.cs` script to it
3. Assign the FloatingTextPrefab to the "Floating Text Prefab" field
4. Optional: Assign your main Canvas to the "Canvas Transform" field
5. Optional: Customize the damage and critical damage colors

#### Option B: Automatic Setup
1. Add the `FloatingTextInitializer.cs` script to a GameObject in your scene (like your GameManager)
2. Assign the FloatingTextPrefab to the "Floating Text Prefab" field
3. The initializer will automatically create the manager if it doesn't exist

## Usage

### Basic Usage
To show damage numbers when hitting a dummy:

```csharp
// Get or calculate the damage value
float damage = 25.5f;

// Display the damage
FloatingTextManager.Instance.ShowDamageText(damage, targetPosition);

// Or for critical hits:
FloatingTextManager.Instance.ShowDamageText(damage, targetPosition, true);
```

### Using Extension Methods
We've included convenient extension methods:

```csharp
// Show damage on a game object
enemyGameObject.ShowDamageText(25.5f);

// Show critical damage
enemyGameObject.ShowDamageText(50.0f, true);

// Show custom text with custom color
enemyGameObject.ShowFloatingText("Healed!", Color.green);
```

## Customization

You can customize:
- Text appearance (size, font, color) in the TextMeshPro component
- Animation behavior (speed, duration) in the FloatingText component
- Default colors in the FloatingTextManager

## Troubleshooting

If damage numbers don't appear:
1. Check the FloatingTextManager exists in the scene
2. Verify the FloatingTextPrefab is assigned to the manager
3. Make sure you have a Canvas in your scene if you're using a Canvas
4. Check the Camera.main reference is valid

## Dependencies
- TextMeshPro package 