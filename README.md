# I3E_TheHeist_Project

## Group Information

**Group Name:** Exhibit Three  
**Group Members:** Alex, Kayden, Sheryn  
**Game Name:** The Heist
**Github Respository:** https://github.com/Alex4576/I3E_TheHeist_Project

## Game Overview

The Heist is a crime-prevention museum game in which the player takes on the role of a museum overseer. The player must complete a crime-prevention quiz before entering the main museum gallery, where suspicious NPC behaviours and security threats must be identified.

The gameplay incorporates interactive objects, CCTV systems and AI-controlled NPCs, including visitors, a robber and a hacker.

## Application Controls

| Control | Function |
|---|---|
| W/A/S/D | Move player front, left, back and right |
| E | Interact |
| F | Access CCTV |

## Gameplay Objective

The objective of the game is to prevent the museum heist and break the time loop by identifying and catching the thieves and hackers before they can successfully carry out their plan.

To complete the game, players must:

1. Enter the museum and make their way through the museum corridor.
2. Enter the Quiz Room and discover that the lift to Level 2 is locked.
3. Complete the Crime Prevention Quiz to unlock the lift.
4. Take the lift to Level 2 and enter the museum gallery.
5. Observe the NPCs and identify suspicious behaviour while protecting the museum exhibits.
6. Use the CCTV system to scan for suspicious activity and repair cameras that have been disabled by the Hacker.
7. Identify and catch the correct suspects while avoiding falsely accusing innocent visitors.
8. Complete all 3 waves of the heist.
9. Catch all the Thieves and the Hacker to prevent the heist and successfully break the time loop.

## Limitations / Known Bugs

- NPC path selection can occasionally result in similar or repeated roaming routes because destinations are randomly generated.
- Quiz progress is not saved if the application is closed.
- The quiz can only be exited using ESC while on the homepage; once the questions begin, the player must complete the quiz.
- NPC animations use a limited set of animation clips, primarily idle and walking.
- Some interactions require the player to aim directly at the object's collider for the raycast to detect it.
- Catching an innocent NPC results in a time penalty, but the NPC remains in the scene and can potentially be caught again.
- CCTV scan results provide an indication of suspicious activity rather than identifying the exact thief, requiring the player to observe the NPCs themselves.

## AI Implementation

### 1. NPC Visitor

#### Purpose

The NPC Visitor represents ordinary museum visitors who are not involved in the heist. They move naturally around the museum to populate the environment and make it more difficult for the player to immediately distinguish suspicious NPCs from innocent visitors.

#### Implementation

The NPC Visitor uses Unity's NavMesh system through a `NavMeshAgent`. A random position is generated within the specified `walkRadius`, after which `NavMesh.SamplePosition()` is used to locate a valid position on the baked NavMesh.

The visitor moves towards this position using `SetDestination()`. Once the visitor reaches its destination, it waits for the specified `waitTime` before selecting another random destination.

The visitor's movement speed is obtained from the NavMeshAgent's velocity and passed to the Animator through the `Speed` float parameter.

#### FSM Diagram

![NPC Visitor FSM Diagram](ReadMeImages/VisitorFSM.png)

### 2. NPC Hacker

#### Purpose

The NPC Hacker represents one of the suspects involved in the museum heist. Its main purpose is to locate and disable active CCTV cameras, reducing the effectiveness of the museum's security system.

The Hacker also attempts to avoid the player when approached, making it more difficult for the player to catch it.

#### Implementation

The NPC Hacker uses Unity's NavMesh system through a `NavMeshAgent` to navigate around the museum. The Hacker alternates between `Scouting` and `Hunting` modes. During `Scouting`, it moves randomly around the museum and may occasionally stop and look around to appear more natural. After a random period of time, it enters `Hunting` mode and begins searching for nearby active CCTV cameras.

The Hacker uses `FindNearestActiveCCTV()` to search for the closest active CCTV within its `detectionRange`. Once a CCTV is found, the Hacker enters the `GoingToCCTV` state and uses `SetDestination()` to move towards it. When it reaches the specified `hackRange`, it stops and enters the `Hacking` state. After the `hackDuration` is completed, `DisableCamera()` is called on the targeted CCTV, disabling the camera and triggering its associated effects.

If the player approaches the Hacker while it is travelling towards a CCTV, the `Hacker` enters the `Evading` state. A direction away from the player is calculated and a valid position is found using `NavMesh.SamplePosition()`. The Hacker then moves away at an increased `evadeSpeed` before eventually returning to `Scouting` mode.

The Hacker's movement speed is passed to the Animator through the Speed float parameter, while the `IsHacking` Boolean parameter controls its hacking animation. When the player successfully catches the Hacker, it enters the `Caught` state, stops moving, updates the gameplay objective and is removed from the scene.

#### FSM Diagram

![NPC Hacker FSM Diagram](ReadMeImages/HackerFSM.png)

### 3. NPC Thief

#### Purpose

The NPC Hacker represents one of the suspects involved in the museum heist. Its main purpose is to locate and disable active CCTV cameras, reducing the effectiveness of the museum's security system. The Hacker also attempts to avoid the player when approached, making it more difficult for the player to catch it.

#### Implementation

The NPC Thief uses Unity's NavMesh system through a `NavMeshAgent` to navigate around the museum. The Thief begins in the `Roaming` state and searches for the nearest available `StealableItem` within its specified `detectionRange`. If an available item is found, the Thief enters the `GoingToItem` state and uses `SetDestination()` to move towards the targeted exhibit.

Once the Thief reaches the specified `stealRange`, it stops moving and enters the `Stealing` state. A stealTimer is used to create a delay before the stealing attempt is completed. The Thief then checks for active CCTV cameras within the `cameraCheckRadius`. If an active CCTV is nearby, the Thief has a lower chance of successfully stealing the item. If no active CCTV is nearby, the stealing attempt succeeds. After the attempt, the Thief returns to the `Roaming` state and searches for another available item.

The Thief also interacts with the CCTV scanning system through `OnDetectedByScan()`, allowing the CCTV system to provide the player with information when suspicious activity is detected.

The Thief's movement speed is obtained from the `NavMeshAgent` velocity and passed to the Animator through the `Speed` float parameter. This allows the NPC to switch between its idle and walking animations depending on its movement.

When the player correctly identifies and catches the Thief, it enters the `Caught` state and stops moving. Any items previously stolen by the Thief are restored to the museum through `Restore()`, the gameplay objective is updated and the Thief is removed from the scene.

#### FSM Diagram

![NPC Thief FSM Diagram](ReadMeImages/ThiefFSM.png)

### 4. NPC CCTV

#### Purpose

The NPC CCTV represents the museum's security surveillance system. Its purpose is to help the player detect suspicious activity and make it more difficult for the Thief to successfully steal museum exhibits. The CCTV can also be disabled by the Hacker, requiring the player to repair it before it can be used again.

#### Implementation

The CCTV operates using two main states: `Active` and `Disabled`. When Active, the player can interact with the CCTV to activate a security scan. During the scan, the player's main camera is temporarily disabled and the CCTV camera is enabled, allowing the player to view the gallery from the CCTV's perspective. A CCTV interface displays the scanning status while the scan is taking place.

The scan runs for the specified `scanDuration` and searches for ThiefAI objects within the CCTV's `scanRadius`. The distance between the CCTV and each Thief is calculated using `Vector3.Distance()`. If a Thief is within range, the CCTV records that suspicious activity has been detected and calls `OnDetectedByScan()` on the Thief. After the scan finishes, the CCTV displays the scan result before returning the player to the normal camera view.

After each scan, a `scanCooldown` prevents the player from immediately using the CCTV again. Player interaction prompts are also temporarily disabled while viewing the CCTV camera and restored when the scan is complete.

The CCTV can be disabled when the Hacker successfully calls `DisableCamera()`. When disabled, spark and smoke VFX are instantiated to visually indicate that the camera has been hacked. A disabled CCTV cannot perform a scan. The player can interact with it to call `RepairCamera()`, which returns the CCTV to its Active state and removes the associated spark and smoke effects.

#### FSM Diagram

![NPC CCTV FSM Diagram](ReadMeImages/CCTVFSM.png)

## Puzzle Answers

### Crime Prevention Quiz

**Question 1:** Why is stealing harmful?  
**Answer:** C. It can cause financial and emotional loss to the victim.

**Question 2:** What is the best way to help prevent theft?  
**Answer:** C. Stay alert and report suspicious activity.

**Question 3:** Which item may be impossible to truly replace after being stolen?  
**Answer:** C. A historical artefact or family heirloom.

**Question 4:** If you notice suspicious behaviour in a museum, what should you do?  
**Answer:** D. Inform museum staff or security.

**Question 5:** Which behaviour is most suspicious?  
**Answer:** C. Repeatedly checking security cameras and restricted areas.

## Gallery Gameplay

| Wave | Suspects |
|---|---|
| Wave 1 | 2 Thieves |
| Wave 2 | 1 Thief, 1 Hacker |
| Wave 3 | 2 Thieves, 2 Hackers |

## External Assets and Credits

### 3D Assets

**Potted Plant**  
`potted_plant-1` by attben - Sketchfab  
License: Creative Commons Attribution (CC BY) (https://sketchfab.com/3d-models/potted-plant-1-9c6eccd1a8eb434981317bf3e66ec2bb)

**Brinjal**  
`Eggplant - Material Study` by buttr_toes - Sketchfab  
License: Creative Commons Attribution (CC BY) (https://sketchfab.com/3d-models/eggplant-material-study-5d35a6a8e39b4c0a89b53be0bfb57ecb)

### Audio

**Kiosk**  
Click 01_Minimal UI Sounds by cabled_mess - Freesound  
License: Creative Commons 0 (https://freesound.org/people/cabled_mess/sounds/370962/)

**Lift Gate**  
Electronic sliding gate by ThabzMalik - Freesound  
License: Creative Commons 0 (https://freesound.org/people/ThabzMalik/sounds/767060/)

### Unity Asset Store

- Skybox - Skybox Series Free (https://assetstore.unity.com/packages/p/skybox-series-free-103633)
- Yughues Free Ground Materials (https://assetstore.unity.com/packages/2d/textures-materials/nature/yughues-free-ground-materials-13001)
- Conifers (https://assetstore.unity.com/packages/3d/vegetation/trees/conifers-botd-142076)
- Outdoor Ground Textures (https://assetstore.unity.com/packages/2d/textures-materials/floors/outdoor-ground-textures-12555)
- Low Poly Character Pack (https://assetstore.unity.com/packages/3d/characters/humanoids/low-poly-character-pack-357288)