# Electric Dash Skill

## Overview
Electric Dash is a new skill that allows the player to perform a quick teleport-like dash forward while dealing electric damage to enemies in the path. The skill reuses the dash animation but has unique electric visual effects and mechanics.

## Implementation Details

### Files Created/Modified:
1. `Spells/ElectricDash.cs` - Main skill behavior script
2. `Spells/ElectricDashSkillData.cs` - Skill data scriptable object
3. `Entity/Player/PlayerStates/PlayerElectricDashState.cs` - Player state for handling the electric dash
4. `Entity/Player/Player.cs` - Added properties and state handling for electric dash
5. `Spells/SkillManager.cs` - Added Electric Dash to skill types and default skills
6. `Input/PCInput.cs` - Added Electric Dash input (C key)
7. `Interfaces/IPlayerInput.cs` - Updated interface with electric dash input
8. `Stats/CharacterStats.cs` - Added Electric as a damage type and electricResistance property
9. `VFX/EntityFX.cs` - Added electric effect for visual feedback

### Key Features:
- **Mind Attribute Scaling**: Damage scales with the player's Mind attribute
- **Spellbook Damage Bonus**: Increases damage when using spellbook weapon
- **Electric Resistance**: Enemy electric resistance reduces damage
- **Teleport Effect**: Player quickly dashes to target location
- **Electric Visual FX**: Yellow electric effect applied to enemies
- **Skill Manager Integration**: Managed through SkillManager with cooldown/mana cost

## Setup Instructions

### In Unity Editor:
1. Create the ElectricDash prefab as described in the prefab text file
2. Create a material for electric effects (bright yellow/blue color)
3. Assign the material to EntityFX's electricMat field

### In Player GameObject:
1. Add the following properties in the Inspector:
   - Electric Dash Speed: 15
   - Electric Dash Duration: 0.3
   - Electric Dash Distance: 6
   - Electric Dash Prefab: (reference to the created prefab)
   - Electric Dash Spawn Point: Create a new Transform child for this

### In SkillManager:
The code already adds Electric Dash to the default skills. Make sure it's properly initialized.

## Usage
Press the C key to activate the Electric Dash when it's off cooldown and you have enough mana.

## Technical Notes
- The skill is implemented as a state in the player's state machine
- It uses the same animation as regular dash ("Dash" animation state)
- The damage calculation includes mind attribute and spellbook damage scaling
- Enemies hit receive electrical damage with special visual effects
- Cooldown and mana cost are managed through the SkillManager 