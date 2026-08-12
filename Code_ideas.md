# Horror Department: Final Project Presentation

## Slide 1: Introduction and Motivation

**Title: The Horror Department: A Narrative Puzzle Experience**

- **Domain and core concept:** *Horror Department* is a top-down 2D horror game in which survival depends on logical problem-solving through circuit puzzles rather than combat.
- **Why puzzle-horror:** While action-horror often relies on direct confrontation, psychological puzzle-horror combines narrative, exploration, and problem-solving. Solving puzzles while being pursued during a blackout raises cognitive load, creating a more tense and immersive experience.
- **Motivation:** The game presents progression as repairing broken systems. The Electric Board restores power and control, while Memory Shards reconstruct the protagonist's fragmented past and narrative.

## Slide 2: Technologies Used

**Title: Development Stack and Tools**

- **Unity 6.3:** Builds the environment, physics, scene flow, and core game loop.
- **C#:** Implements gameplay scripts, singletons, quests, persistence, and AI logic.
- **Universal Render Pipeline (URP):** Provides optimized rendering and atmospheric 2D lighting, including the flashlight in dark hospital corridors.
- **Unity NavMesh:** Adapted for 2D pathfinding so the monster can patrol and pursue the player around obstacles.
- **Input System:** Handles responsive player movement and interactions.
- **TextMesh Pro and Unity Timeline/Playables:** Support scalable UI text, dialogue, and cinematic cutscenes.
- **JSON serialization:** Saves and loads player progress, inventory, quests, and game states across sessions.

## Slide 3: Core Use Cases

**Title: Player Interactions and Mechanics**

- **Exploration and discovery:** The player navigates the eerie HospitalScene and distorted NightmareScene, using the flashlight to uncover paths and clues.
- **Survival and evasion:** During blackout events, the monster spawns. The player runs, hides, and can shine the flashlight on the monster to temporarily stun it and escape.
- **Puzzle solving:**
  - **Electric Circuit Board:** Rotate wire segments to form a continuous electrical path and restore power.
  - **Nightmare Jigsaw:** Drag and place Memory Shard pieces to reveal hidden parts of the story.
- **Narrative progression:** Interact with the Doctor, Receptionist, and Mr. Graves to complete sequential quests and trigger story cutscenes.
- **System management:** Collect inventory items and rely on auto-save checkpoints before dangerous encounters.

## Slide 4: Design and Architecture

**Title: World Design and UI Layout**

- **Level architecture:**
  - **MainMenuScene and LoadingScene:** The entry point to the game.
  - **HospitalScene:** The main hub for NPCs, locked doors, exploration, and blackout encounters.
  - **NightmareScene and NightmarePuzzle:** Psychological-horror scenes containing the jigsaw mechanics.
  - **Cutscene scenes:** Dedicated Timeline-driven scenes that control narrative pacing.
- **UI design and wireframing:**
  - **Immersive HUD:** A minimal health bar and damage vignette communicate player danger without distracting from the world.
  - **Contextual hints:** Interaction prompts appear only when the player is near an object.
  - **Puzzle overlay:** A focused 5 x 5 Electric Board interface temporarily hides the game world during circuit solving.

## Slide 5: Implementation and Design Choices

**Title: Development Achievements and Scope**

- **Completed technical implementations:**
  - **Monster AI:** Uses NavMesh patrol and chase behavior, a detection radius, a coroutine-based loss-of-interest timer, blackout-based spawning, and flashlight stun behavior.
  - **Electric Board traversal:** Uses breadth-first search (BFS) to verify whether electricity travels from the source wire to the target wire.
  - **Persistent game state:** The GameState singleton manages quests, inventory, health, checkpoints, and JSON-based saving.
- **Design choices and scope:**
  - **No healing mechanic:** Damage persists until checkpoint respawn, sustaining tension.
  - **Focused scope:** The project prioritizes polished early chapters and complete core systems over a larger unfinished game world.

## Slide 6: Game Screens

**Title: Visualizing the Horror**

Insert gameplay screenshots for the following scenes:

- **Main Menu:** A dark title screen that establishes the psychological-horror tone.
- **Hospital Exploration:** A top-down view of the dark hospital, illuminated by the player's flashlight.
- **Monster Encounter:** A chase scene showing the monster's proximity and damage vignette.
- **Electric Board Puzzle:** The 5 x 5 wire-grid puzzle interface.
- **Nightmare Jigsaw:** Fragmented Memory Shard pieces awaiting assembly.

## Slide 7: Summary, Challenges, and Future Plans

**Title: Conclusion and Next Steps**

- **Summary:** *Horror Department* combines AI-driven evasion, atmospheric exploration, narrative quests, and logic-based puzzles. It demonstrates that repairing circuits can be as tense and rewarding as direct combat.
- **Technical challenges:**
  - Adapting Unity's 3D NavMesh for 2D required careful handling of colliders and z-axis constraints.
  - Maintaining persistent game state across scenes required strict singleton and UI-reference management.
- **Future plans:**
  - **Expanded puzzles:** Add sequence locks, audio-based puzzles, and other logic mechanics.
  - **New AI threats:** Introduce monsters that respond differently to light and sound.
  - **Narrative completion:** Expand later story chapters and the inventory-crafting system.
