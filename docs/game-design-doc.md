# Necroknight - Game Design Document

## Overview

**Genre:** 2D Top-Down Auto-Battler / Horde Survival  
**Theme:** Dark Fantasy / Necromancy  
**Inspiration:** Vampire Survivors, Wild Tamer, Right Click to Necromance

---

## Core Concept

You control a Necroknight puck moving through a top-down world. Walk into enemies to auto-attack them. When enemies die, they join your undead army. Build a massive horde, survive waves, and upgrade your minions.

---

## Controls

- **WASD / Joystick** - Move your Necroknight
- **No attack button** - Combat is automatic on contact

---

## Core Loop

1. Move around the map
2. Bump into enemies → auto-attack triggers
3. Defeated enemies become YOUR minions
4. Your minions follow you and auto-attack too
5. Survive waves → earn XP & gold
6. Upgrade minions → repeat with bigger horde

---

## Tameable Enemies

| Creature | Type   | Trait              |
| -------- | ------ | ------------------ |
| Gnome    | Small  | Fast attack speed  |
| Lizard   | Small  | Poison bite        |
| Goblin   | Small  | Swarm bonus        |
| Harpy    | Flying | Attacks from above |
| Ogre     | Large  | High HP, slow      |
| Minotaur | Large  | Charges enemies    |
| Treant   | Large  | AOE slam           |
| Slime    | Small  | Splits on death    |

_More creatures unlock as you progress!_

---

## RPG Upgrade System

### Minion Upgrades (per creature type)

- **HP** - Increases survivability
- **Attack** - More damage per hit
- **Speed** - Faster movement
- **Special** - Enhances unique trait

### Necroknight Upgrades

- **Tame Radius** - Auto-tame from further away
- **Horde Cap** - Command more minions
- **Aura** - Passive buffs to nearby minions
- **Soul Magnet** - Collect XP from further

---

## Progression

- **Waves** - Survive timed enemy waves
- **XP** - Level up mid-run for temporary buffs
- **Gold** - Permanent upgrades between runs
- **Unlock** - New creature types & areas

---

## Win/Lose

- **Win:** Survive all waves / defeat the boss
- **Lose:** Necroknight HP reaches zero

---

## Art Style

Pixel art, top-down view. Cute but dark creature designs. Satisfying soul particles when taming. Simple, clean UI.

---

_Tame. Grow. Survive._
