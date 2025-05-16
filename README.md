# 🎮 AR Card Game Documentation

## 📋 Overview
The application is a turn-based card game in augmented reality (AR), developed in Unity. Players and computer opponents control different units, with the goal of eliminating all enemy units. The application uses Vuforia for AR marker recognition, supporting simultaneous recognition of up to 3 markers.

## 🔧 Technical Specifications
- **Development**: Unity
- **AR Functionality**: Vuforia AR marker system
- **Models and Animations**: Mixamo
- **Sound Effects**: freesound.org and adapted sounds from League of Legends
- **Game Type**: Turn-based, card strategy

## 🏞️ Game Scenes
- **Main Menu**: Features themed music and a play button to start the game
- **Main Game Scene**: The battlefield where the game takes place

## 📱 User Interface
- **Event Log**: Displayed at the bottom of the screen, tracking all game events
- **Unit Information**: Left panel with data about player units and computer opponent units
- **Battle Status**: Right panel showing health and status (alive/dead) of units
- **Action Window**: Central panel with options to attack or use special abilities
- **Turn Status**: Information at the top of the screen showing which player's turn it is

## 🦸‍♂️ Player Units

### ⚔️ Warrior
**Role**: Front-line damage dealer  
**Health**: 120  
**Attack**: 20  
**Type**: Melee  

**Special Ability**: *Whirlwind*
- Deals 50% of normal damage to ALL enemy units
- Excellent for clearing multiple low-health enemies
- Cooldown: 3 turns

### 🛡️ Knight
**Role**: Tank/Protector  
**Health**: 150 (Highest among player units)  
**Attack**: 15  
**Type**: Tank  

**Special Ability**: *Shield Block*
- Reduces incoming damage by 50% for one turn
- Critical for surviving powerful enemy attacks
- Cooldown: 3 turns

### 🧙‍♂️ Mage
**Role**: Area damage dealer  
**Health**: 70 (Lowest among player units)  
**Attack**: 10  
**Type**: Spellcaster  

**Special Ability**: *Fireball*
- Deals 25 damage to primary target
- Deals 50% splash damage to adjacent enemies
- Excellent for grouped enemies
- Cooldown: 2 turns

### 🏹 Archer
**Role**: Single-target damage dealer  
**Health**: 80  
**Attack**: 15  
**Type**: Ranged  

**Special Ability**: *Aimed Shot*
- Doubles damage to a single target
- Perfect for eliminating high-priority threats
- Cooldown: 2 turns

### 🗡️ Rogue
**Role**: Assassin/Critical striker  
**Health**: 90  
**Attack**: 18  
**Type**: Assassin  

**Special Ability**: *Backstab*
- 80% chance for critical hit
- Critical hits deal 250% of normal damage
- High-risk, high-reward ability
- Cooldown: 2 turns

## 👹 Enemy Units

### 🧙‍♀️ Dark Wizard
**Role**: Support/Damage dealer  
**Health**: 80  
**Attack**: 8 (Basic attack only)  
**Type**: Spellcaster  

**Special Abilities**:
1. *Shadow Bolt*
   - 20 damage to a single target
   - Used against isolated threats

2. *Dark Nova*
   - 12 damage to ALL player units
   - Preferred when multiple players are present

3. *Healing Shadows*
   - Restores 15 health to allies
   - 70% chance to use when allies are below 50% health

**AI Behavior**: Intelligently chooses spells based on battlefield conditions

### 👺 Goblin
**Role**: Fast striker  
**Health**: 60 (Low)  
**Attack**: 10  
**Type**: Melee  

**Special Mechanic**: *Double Strike*
- Attacks twice every turn
- First attack: 100% damage
- Second attack: 50% damage
- Always targets the weakest (lowest HP) player unit

### 👿 Orc
**Role**: Berserker  
**Health**: 100  
**Attack**: 15  
**Type**: Melee  

**Special Mechanic**: *Enrage*
- Triggers when health drops below 50%
- Increases damage by 50% when enraged
- Attacks become faster and more powerful
- Targets randomly, making it unpredictable

### 💀 Skeleton Archer
**Role**: Ranged attacker  
**Health**: 70  
**Attack**: 12  
**Type**: Ranged  

**Special Ability**: *Arrow Volley*
- Used every third turn
- Fires 3 arrows at a single target
- Each arrow deals 40% of normal damage
- Total potential damage: 120% of basic attack
- Prioritizes targeting spellcasters and ranged units

### 🧌 Troll
**Role**: Regenerating tank  
**Health**: 180 (Highest in the game)  
**Attack**: 25 (Highest single hit damage)  
**Type**: Tank  

**Special Mechanics**:
1. *Regeneration*
   - Heals 5 health points every turn
   - Makes it difficult to defeat in extended battles

2. *Slow Attack Pattern*
   - Can only attack every other turn
   - Targets tank and melee units preferentially

## ⚙️ Game Mechanics
The game follows a simple sequential turn-based model:
1. The player selects an action (attack or special ability) and a target
2. Player actions are executed with appropriate animations and sound effects
3. Enemy units automatically choose actions according to their AI rules
4. The turn repeats until all units on one side are eliminated
