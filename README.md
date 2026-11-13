# My Snowy Friend - Game Design Document (GDD)

## Overview

**My Snowy Friend** is a cozy 3D puzzle game where the player controls a snowman who discovers an abandoned kitten inside a sled during a snowy winter morning. Determined to return the kitten to its family, the snowman begins a journey down the mountain while slowly melting along the way.

The game focuses on grid-based puzzle solving, environmental interaction, and emotional storytelling through gameplay rather than dialogue.

---

# Core Information

| Category        | Details                                       |
| --------------- | --------------------------------------------- |
| Genre           | 3D Puzzle                                     |
| Perspective     | Top-down / Isometric-like                     |
| Art Style       | Cozy stylized winter diorama                  |
| Target Platform | PC                                            |
| Core Theme      | Sacrifice, kindness, companionship            |
| Inspiration     | Captain Toad: Treasure Tracker, Bad Ice-Cream |

---

# Story

## Premise

During a cold winter dawn, a snowman walks through the forest alongside his snowman siblings when he hears a faint sound nearby. Curious, he investigates and discovers a small kitten trembling from the cold inside an abandoned sled.

Without hesitation, the snowman offers the kitten his scarf and decides to return it to its owners, whom he believes live at the foot of the mountain.

Together they begin a journey filled with obstacles and puzzles. However, the snowman faces a tragic reality: with every step toward warmer areas, he slowly melts away.

At the end of the journey, using the last of his strength, the snowman gives the sled one final push. The kitten slides safely into the arms of its family while, in the distance, the snowman silently melts into the snow.

---

# Gameplay

## Core Objective

The player must escort and transport the kitten's sled safely to the end of each level.

Puzzle solving revolves around positioning, movement planning, environmental interactions, and manipulating the sled through different mechanics.

---

# Core Mechanics

## Grid-Based Movement

The player moves on a fixed grid system.

### Movement Rules

* Movement is allowed in four directions:

  * Forward
  * Backward
  * Left
  * Right
* Diagonal movement is not allowed.
* All level elements are aligned to the same grid system.
* Movement should feel precise, readable, and predictable.

This restriction simplifies puzzle readability and encourages strategic movement planning.

---

# Player States

## Full Snowman Form

The default player form.

### Characteristics

* Can move normally.
* Cannot pass through low structures such as frames.
* Can transport and interact with the sled.

---

## Head-Only Form

Occurs after touching a Hot Platform.

### Characteristics

* Small enough to pass through low frames.
* More vulnerable.
* Touching another Hot Platform while already in this state results in death.

---

# Level Design

## Philosophy

Each level should introduce:

* A new mechanic
* A new twist on a previous mechanic
* A new interaction combination

The game should teach naturally through level design without explicit tutorials.

---

## Level Structure Rules

All levels must follow these rules:

* Maintain the same dimensional depth.
* Level length may vary within an established average range.
* The snowman always starts at:

  * `x = 1`
* The sled always starts at:

  * `x = 3`
* The player must remain inside the rectangular level boundaries.
* The sled must always remain reachable by the player.
* Mechanics should feel intuitive and self-explanatory.
* Levels must contain:

  * Minimum: 2 sections
  * Maximum: 3 sections

---

# Collectibles

Some levels may contain optional collectibles.

### Status

Currently undecided.

Possible purposes:

* Completion percentage
* Unlockables
* Cosmetic rewards
* Extra story details

---

# Game Elements

## Ice 🧊

### Description

The kitten's sled can slide linearly across ice surfaces.

### Behavior

* Continues moving until colliding with:

  * Walls
  * Obstacles
  * Other mechanics
* Used for positioning puzzles and momentum interactions.

---

## Frame ⛩️

### Description

A low structure that blocks larger entities.

### Behavior

* The full snowman cannot pass through.
* The sled cannot pass through.
* The head-only form can pass underneath.

---

## Hot Platform 🔥

### Description

Melts the snowman's body.

### Behavior

* Transforms the player into head-only form.
* If already in head-only form:

  * The player dies.

### Design Purpose

* Form-switching puzzles
* Path restrictions
* Sacrifice-based progression

---

## Snow Platform ❄️

### Description

Restores the snowman's body.

### Behavior

* Returns the player to full snowman form.

### Design Purpose

* State recovery
* Alternating traversal routes

---

## Jump Platform ⬆️

### Description

Launches the player or sled forward.

### Behavior

* Launches targets up to 4 tiles forward.
* Can jump over:

  * Walls
  * Small structures
* Direction can be modified using switches.

### Material Variants

* Wood
* Stone

### Design Purpose

* Traversal puzzles
* Chain reactions
* Multi-object positioning

---

## Gongs 🎯

### Description

Mechanical triggers activated when struck.

### Behavior

* Activate or deactivate wooden mechanisms.
* Can be triggered by fast-moving sliding objects.

### Material Affinity

* Wood mechanisms only

### Design Purpose

* Timing puzzles
* Indirect interactions
* Remote activation

---

## Pressure Plates ⬛

### Description

Mechanisms activated by weight.

### Behavior

* Remain active while an object stays on top.

### Material Affinity

* Stone mechanisms only

### Design Purpose

* Multi-step puzzles
* Object management
* Persistent state changes

---

## Doors 🚪

### Description

Openable and closable obstacles.

### Behavior

* Controlled via switches or mechanisms.

### Material Variants

* Wood
* Stone

---

# Camera

## Camera Style

The camera remains distant and heavily zoomed out to create the feeling of observing a small winter diorama.

The perspective should feel almost orthographic while still retaining slight perspective depth.

---

## Camera Behavior

### Follow System

* Smoothly follows the player horizontally throughout the level.
* Anticipates player movement direction.

### Dynamic Rotation

* Slightly rotates when necessary to improve visibility.

### Idle Behavior

If the player remains still:

* The camera slowly shifts toward the direction the player is facing.
* Gradually zooms slightly closer after extended inactivity.

### Movement Constraints

* Minimal movement on vertical axes.
* Stable framing prioritized for puzzle readability.

---

# Visual Direction

## Art Style

* Cozy stylized 3D visuals
* Soft winter lighting
* Miniature diorama aesthetic
* Clean readable geometry
* Strong silhouette readability for mechanics

---

## Environment Themes

Potential environments include:

* Snowy forests
* Frozen lakes
* Wooden bridges
* Mountain paths
* Small abandoned structures
* Warm village outskirts

---

# Emotional Design

The emotional experience should evolve throughout the game:

| Beginning | Middle  | Ending      |
| --------- | ------- | ----------- |
| Cozy      | Hopeful | Melancholic |

The player should gradually become aware that the snowman will not survive the journey.

The story should rely heavily on:

* Animation
* Environmental storytelling
* Music
* Gameplay consequences

Rather than dialogue-heavy exposition.

---

# Puzzle Design Goals

## Key Principles

* Clarity over complexity
* Mechanics introduced progressively
* Every mechanic must combine naturally with previous ones
* Minimal UI
* Environmental readability
* Short but meaningful levels

---

# Technical Notes

## Movement System

* Tile/grid-based logic
* Fixed-distance movement per input
* Smooth interpolation between tiles

## Camera System

* Dynamic follow target
* Slight predictive movement
* Controlled orthographic-like perspective

## Puzzle Logic

* State-based interactions
* Mechanism activation system
* Object tagging by material type
