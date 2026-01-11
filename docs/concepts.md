# Necroknight - Technical Concepts Document

## Overview

This document outlines the core algorithms and systems for Necroknight's open-world enemy spawning, movement, aggro, and combat mechanics.

---

## 1. World Structure

### Map Size

- **1000x1000 tiles** (Unity Tilemap)
- Divided into **regions/biomes** (e.g., 200x200 chunks)

### Region Types

| Region       | Enemies            | Difficulty  |
| ------------ | ------------------ | ----------- |
| Forest       | Gnomes, Goblins    | Easy        |
| Swamp        | Lizards, Slimes    | Easy-Medium |
| Mountains    | Harpies, Ogres     | Medium      |
| Ruins        | Minotaurs, Treants | Hard        |
| Dark Citadel | Bosses             | End-game    |

---

## 2. Enemy Group Spawning Algorithm

### Poisson Disk Sampling

Use **Poisson Disk Sampling** to distribute enemy groups naturally across the map. This ensures groups are spread out with minimum distance between them (no clumping, no perfect grids).

```
Algorithm: Poisson Disk Sampling
─────────────────────────────────
Input:
  - width, height (map size)
  - minDistance (minimum space between group centers)
  - maxAttempts (tries before giving up on a point)

Output:
  - List of spawn points for enemy groups

Steps:
1. Create a grid of cells (cellSize = minDistance / sqrt(2))
2. Pick random initial point, add to active list
3. While active list not empty:
   a. Pick random point from active list
   b. Generate up to maxAttempts candidates around it (distance: minDistance to 2*minDistance)
   c. For each candidate:
      - Check if within bounds
      - Check if no other points within minDistance (use grid for fast lookup)
      - If valid, add to points list and active list
   d. If no valid candidate found, remove point from active list
4. Return all points as group spawn locations
```

### Parameters by Region

| Region    | minDistance | Group Size               |
| --------- | ----------- | ------------------------ |
| Forest    | 30 tiles    | 3-6 Gnomes               |
| Swamp     | 40 tiles    | 2-4 Lizards              |
| Mountains | 50 tiles    | 1-2 Ogres                |
| Ruins     | 60 tiles    | 1 Minotaur + 2-3 minions |

---

## 3. Group Movement Algorithm (Flocking / Boids)

### Overview

Enemy groups use a simplified **Boids algorithm** for natural group movement. Each enemy in a group follows three rules:

### The Three Rules

```
1. SEPARATION - Avoid crowding nearby groupmates
   ─────────────────────────────────────────────
   For each nearby ally within separationRadius:
     separationForce += (myPosition - allyPosition).normalized / distance

2. ALIGNMENT - Steer towards average heading of group
   ─────────────────────────────────────────────────
   avgVelocity = sum(ally.velocity) / allyCount
   alignmentForce = (avgVelocity - myVelocity).normalized

3. COHESION - Steer towards center of group
   ─────────────────────────────────────────
   centerOfMass = sum(ally.position) / allyCount
   cohesionForce = (centerOfMass - myPosition).normalized
```

### Combined Steering

```
finalVelocity = (
    separation * separationWeight +
    alignment  * alignmentWeight +
    cohesion   * cohesionWeight
).normalized * moveSpeed
```

### Recommended Weights

| Creature | Separation | Alignment | Cohesion | Notes        |
| -------- | ---------- | --------- | -------- | ------------ |
| Gnome    | 1.5        | 1.0       | 2.0      | Tight packs  |
| Lizard   | 1.0        | 0.5       | 1.0      | Loose groups |
| Ogre     | 2.0        | 0.2       | 0.5      | Spread out   |

### Random Walk (Idle Wandering)

When not aggroed, groups perform a **random walk**:

```
Algorithm: Group Random Walk
────────────────────────────
1. Group has a "leader" (first spawned or strongest)
2. Every 3-5 seconds (randomized):
   a. Leader picks random direction (0-360°)
   b. Leader picks random distance (5-15 tiles)
   c. Leader moves toward target point
3. Group members follow leader using Boids rules
4. If leader hits region boundary, pick new direction
```

---

## 4. Aggro System

### Aggro Detection

```
Algorithm: Aggro Check (per frame)
──────────────────────────────────
For each enemy group:
  distanceToPlayer = Vector2.Distance(groupCenter, playerPosition)

  if distanceToPlayer < aggroRadius:
    group.state = AGGRO
    group.target = player
  else if distanceToPlayer > deaggroRadius:
    group.state = IDLE
    group.target = null
```

### Aggro Parameters by Creature

| Creature | Aggro Radius | Deaggro Radius | Aggro Behavior  |
| -------- | ------------ | -------------- | --------------- |
| Gnome    | 8 tiles      | 15 tiles       | Swarm player    |
| Lizard   | 10 tiles     | 18 tiles       | Circle and bite |
| Ogre     | 12 tiles     | 20 tiles       | Charge straight |
| Minotaur | 15 tiles     | 25 tiles       | Charge + AOE    |

### Group Aggro Propagation

```
When one enemy in group aggros:
  → Entire group aggros (shared state)
  → Nearby groups of SAME TYPE within 10 tiles also aggro (chain reaction)
  → Groups of DIFFERENT types ignore each other
```

---

## 5. Enemy-Enemy Interaction Rules

### Faction System

```
Factions:
  - WILD (all untamed enemies)
  - PLAYER (Necroknight + tamed minions)

Rules:
  - WILD enemies IGNORE other WILD enemies (no infighting)
  - WILD enemies ATTACK player and PLAYER faction
  - PLAYER faction ATTACKS WILD enemies
  - PLAYER faction IGNORES other PLAYER faction
```

### Implementation

```csharp
// Pseudocode
bool ShouldAttack(Entity self, Entity target) {
    if (self.faction == target.faction) return false;
    if (self.faction == WILD && target.faction == WILD) return false;
    return true;
}
```

---

## 6. Combat System

### Auto-Attack (Passive)

```
On collision/proximity with enemy:
  - Deal damage based on attack stat
  - Attack cooldown (e.g., 0.5s)
  - No player input required
```

### Right-Click: Resurrect / Tame

```
Algorithm: Resurrection
───────────────────────
On Right-Click:
  1. Check for corpses within tameRadius (e.g., 3 tiles)
  2. If corpses found AND soulMeter >= cost:
     a. Consume soul meter
     b. Convert corpse to PLAYER faction
     c. Corpse rises with death animation (reverse)
     d. Add to player's minion list
  3. If no corpses, do nothing (or play fail sound)
```

### Special Attack Meter

```
Soul Meter:
  - Max: 100
  - Gain: +10 per enemy killed
  - Cost: Varies by creature size
    - Small (Gnome, Lizard): 10 souls
    - Medium (Goblin, Harpy): 20 souls
    - Large (Ogre, Minotaur): 40 souls
```

---

## 7. Player UI Elements

### Health Bar

- Top-left corner
- Red bar, white outline
- Shows current HP / max HP

### Soul Meter

- Below health bar
- Purple/green glowing bar
- Fills as you kill enemies

### Minimap

```
Minimap Specs:
  - Bottom-right or top-right corner
  - Size: 150x150 pixels
  - Shows:
    - Player position (white dot, center)
    - Tamed minions (green dots)
    - Enemy groups (red dots)
    - Region boundaries (faint lines)
  - Render: Separate camera, orthographic, culling mask
```

---

## 8. Unity Implementation Tips

### Tilemap Setup

```
Hierarchy:
  Grid
  ├── Tilemap_Ground (base terrain)
  ├── Tilemap_Decoration (trees, rocks)
  ├── Tilemap_Collision (walls, water)
  └── Tilemap_Regions (invisible, for spawn rules)
```

### Chunk Loading (for 1000x1000 map)

```
- Only load/activate chunks near player (e.g., 3x3 chunk radius)
- Pool enemy GameObjects
- Disable AI for off-screen enemies
- Use Unity's Tilemap chunk system or custom solution
```

### Recommended Components

| System      | Unity Feature                                |
| ----------- | -------------------------------------------- |
| Pathfinding | A\* Pathfinding Project (free) or NavMesh 2D |
| Collision   | CircleCollider2D + Rigidbody2D               |
| Spawning    | Object Pooling                               |
| Minimap     | RenderTexture + secondary Camera             |

---

## 9. Algorithm Summary

| System         | Algorithm                    | Purpose                  |
| -------------- | ---------------------------- | ------------------------ |
| Group Spawning | Poisson Disk Sampling        | Natural distribution     |
| Group Movement | Boids (Flocking)             | Realistic group behavior |
| Idle Wandering | Random Walk                  | Organic patrol patterns  |
| Aggro          | Distance check + propagation | Engaging combat triggers |
| Factions       | Tag-based filtering          | Prevent enemy infighting |
| Resurrection   | Area check + resource cost   | Core taming mechanic     |

---

_Build the horde. Conquer the world._
