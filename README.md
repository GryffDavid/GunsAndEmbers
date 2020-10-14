# Guns and Embers
This Readme is essentially a design outline of the game and a post-mortem of sorts too. However, I definitely haven't given up on the idea or the implementation and I still stand by the concept. I would love to finish this game given enough time and money... and an artist. 

Although the game is not complete, the core mechanics and most of the features have been implemented in some form. No final art has been produced and everything is just placeholder, programmer art. I'm very much a designer and programmer and definitely not an artist. I am however happy with the [particle effects](https://github.com/GryffDavid/MultithreadedParticles) and the programmatically generated effects such as the [lightning](https://github.com/GryffDavid/StylizedLightning), [smoke](https://github.com/GryffDavid/VectorSmoke) and [grass](https://github.com/GryffDavid/2DDynamicGrass), as well as some of the sound effects I made too.

## Table of Contents

- [Mechanics and Gameplay](#Mechanics-and-Gameplay)
- [Gifs, Videos, Screenshots](#Gameplay)
- [What is missing](#What-is-missing)
- [What I Learned](#What-I-learned)
- [Design Successes](#Design-Successes)
- [Design Problems](#Design-Problems)
- [Features not Implemented](#Features-Not-Implemented,-Otherwise-Known-as-Ideas)
- [Background](#Background)
- [Features implemented](#Features-implemented)
	- [Turrets](#Turrets)
      - [Grenade Turrets](#Grenade-Turrets)
      - [Heavy Projectile Turrets](#Heavy-Projectile-Turrets)
      - [Light Projectile Turrets](#Light-Projectile-Turrets)
      - [Beam Turrets](#Beam-Turrets)
	- [Traps](#Traps)
		- [Offensive](#Offensive-Traps)
		- [Defensive](#Defensive-Traps)
	- [Invaders](#Invaders)
		- [Airborne](#Airborne)
		- [Heavy Ranged](#Heavy-Ranged)
		- [Light Ranged](#Light-Ranged)
		- [Melee](#Melee)
- [Code Links](#Code-Links)

## Mechanics and Gameplay
- This is a non-traditional tower defence game. 
- Players control a tower to which they can attach turrets to repel incoming invaders. 
- Traps can be placed on the terrain in front of the tower. 
- The turrets work in a manner very similar to artillery games such as Worms or Scorched Earth.
- Players choose a loadout from an assortment of traps and turrets before beginning the level. 
- Invaders of various types approach the tower in escalating waves. This lead to quite fast-paced, escalating gameplay as traps need to be replaced between bouts of artillery fire. 
- Quick decisions about trap placement are essential and certain traps and turrets can be used together strategically to devastate incoming invaders. Such as careful use of a freeze turret and a flame trap to lock an invader in place in the flames, dealing multiple rounds of high damage-over-time. 
- Tactical placement of the traps can be thought of the same as the trap placement mechanics in Orcs Must Die and its sequel. 
- Placement of the turrets into the slots on the tower is also important. Placing a heavy artillery turret in the top-most slot will increase it's range just based on physics alone - the projectile will have more time to move in the X axis as it falls along the Y axis due to gravity. So placement of turrets on the tower involves the player making a tactical decision. 
- There is also a certain element of emergent gameplay that I like and discovered. How two traps interact in an interesting way or how a trap and turret interact. While programming, I was often subsequently surprised by some bit of emergent gameplay that I didn't plan for that turned out to be really cool. So I started to embrace that by making sure that traps interact in a way that the player would perhaps expect them to or at least in a way that makes sense so the player when observing the interaction says, "Oh, of course. That makes sense. I didn't think of that" instead of "That's dumb. Why didn't that work?!". Such as a flamethrower trap having the flame blocked by a wall trap, which could then be done intentionally to trap invaders between the wall and flamethrower with nowhere to escape.

## Gameplay
![Animation of Gameplay](https://github.com/GryffDavid/READMEImages/blob/master/TowerDefense/Gameplay1.gif)
![Animation of Gameplay 2](https://github.com/GryffDavid/READMEImages/blob/master/TowerDefense/Gameplay2.gif)

This gameplay video shows some of the key mechanics and some of the cool features that I'm happy with.
- The artillery arcing projectiles onto the terrain
- The [Gas Grenades](#GasGrenadeTurret) bouncing across the terrain and unleashing a cloud of toxic gas that does damage over time to invaders that pass through it
- Invaders moving up and attacking the wall traps
- The [DropShip](#DropShip) that flies in from the right and drops off 2 battering rams (Green placeholder graphics)
- [Invaders](#Soldier) using pathfinding to navigate back through the traps to man the battering ram, taking it forward to attack the tower
- The [Healing Drones](#HealerDrone) which target invaders that have taken damage, floating nearby until the invaders are fully healed. Waiting unti, more invaders are damaged or moving onto another hurt invader. They also will not move too close to the tower and will not heal any invader too close to the tower, making them a challenge to hit. Also I'm really happy with their motion.
- The [Harpoon Cannon invaders](#HarpoonCannon) which shoot harpoons out that attach to a turret whereupon the invader pulls the rope taut and starts to reverse, eventually destroying the turret.
- [Trap](#Traps) placement works quite well with 2 bugs that need addressing
- The shield on the tower actually works very well and I'm happy with the damage absorption and shield regeneration mechanics. Although the visuals are missing in the video, the invader projectiles can be seen stopping in mid-air and the shield bar on the UI being knocked down.

[Just Gameplay (YouTube)](https://youtu.be/00MV9-NqXG8?t=27)

[Setup and gameplay. No menus (YouTube)](https://youtu.be/00MV9-NqXG8)

[Menus, Setup and Gameplay (YouTube)](https://youtu.be/xH4tC8bOpSg)

[Buggy, but mechanically solid gameplay from 2017 (YouTube)](https://youtu.be/wTQDjgLDA-c?t=4)

## What is missing
- Balance.
- Lots of optimization. I have optimized some things, but there is so much that still needs to be done. But I am/was being careful about not prematurely optimizing, instead trying to flesh out mechanics and get the gameplay solid before worrying about optimizing and getting the code to production level. Optimizing on the fly felt pointless because so much of what I'd written was going to be rewritten because I'd learned something new and had a far better approach or because I wasn't sure if a mechanic was going to stick around.
- There is no tutorial. 
- There is a story, but it's not very good and I only have an incredibly loose outline. I was really much more focused on gameplay and mechanics, not too bothered about story. 
- A proper structure to the levels and invaders.
- Different terrain features/terrains. 
- Different towers.
- Big invaders. For the most part the invaders are mostly small, which works quite well, but there should also be that "Wow" moment for a player when something big and unexpected turns up such as a tripod with a laser cannon for a face. Something the size of the tower that feels threatening and feels like it suddenly takes priority.

## What I learned
- Learned a lot about design from a practical point of view.
- Implementing 2.5D taught me a lot. I had to solve a *lot* of problems, the biggest of which being the lighting system implementation. Turns out there aren't a whole lot of 2.5D lighting examples to go off of (At least, at the time) so I ended up designing the whole thing from the ground up. 
- The arc of the projectiles being predicted is essential because it means there isn't as much randomness about where the projectile is going to land. 
- Physics. 
[Insert Screenshots of how much I progressed over the course of 2 years]

## Design Successes
- The gameplay is genuinely fun which is extremely important. Arcing a huge exploding projectile through the air and having it land squarely into a clump on oncoming invaders is extremely satisfying. 
- The emergent gameplay is a success that I wasn't expecting, but I embrace wholeheartedly.
- The "mood" system and AI is pretty cool too. 
- The lighting systems is really great and I definitely would not have been able to do that in just 2 dimensions.

## Design Problems
- Players picking a loadout before knowing what enemies are approaching is probably the biggest problem, but it's easily overcome with a couple small changes, but I would really need to base this decision on at least a handful of levels being complete with a solid variety of weapons. The solution would be to let the player just unlock the weapons along the progression of the game and let them just pick from the entire list as opposed to choosing a loadout before the level starts.
- The 2.5D may not have been the best choice, but I don't regret it. If I were to start the whole project over again I would still go with 2.5D. It certainly complicated things, but I do like the extra variety and mechanics it brings as opposed to the initial design which was just 2D. It does mean that the invaders can't be *too* intelligent though because otherwise they'd just walk around each trap by moving along the Z depth. They have to be a bit dumb which could be immersion breaking for the player. 
- The 2.5D also complicates the heavy projectile arcing, although I have found that my solution of predicting where approximately it's going to land and then adjusting the Z depth based on invader density is quite good.
- The invaders bunching up against the tower is perhaps a problem that needs to be re-thought. It forces the player to fire turrets almost directly downward which is irritating and not satisfying at all.

## Features Not Implemented, Otherwise Known as Ideas
- Ability to place a flamethrower trap facing "backwards" as to fire left instead of right. This would allow the player to trap invaders between a wall and a flamethrower and torch the invaders when they're bunched up against the wall.

## Background
The project was initially started in July 2013, about a year into my using C# and XNA. The budget for this project has always been $0 and I'm the only person that has worked on it. I worked on it fairly consistently until about late-2017 when I set it aside due to burnout after only really taking public holidays off for 4 years straight - no weekends. I worked on [another project](https://github.com/GryffDavid/ArenaPlatformer) after that until September 2018 when my mother got sick. I took care of her until her passing in February 2020. I didn't completely stop during that time, but my work was sporadic and inefficient.

## Features implemented
- ❌ Feature is only partially implemented/Only some code exists
- ✅ Feature is implemented and currently working
- ❎ Feature has been implemented successfully in the past, but does not currently work
### Turrets
#### Grenade Turrets
**StickyMineTurret** ❎ Fire a bunch of mines that stick onto the terrain and can't be interacted with by the invaders. The player can then choose to detonate all of the mines at the same time when they feel is appropriate.

**GasGrenadeTurret** ✅ Fires a grenade out onto the terrain. After a little bit of time it explodes into a gas cloud that causes damage-over-time to any invader that walks through it.

**GrenadeTurret** ✅ Fires a grenade out onto the terrain, the grenade explodes after a time.

### Heavy Projectile Turrets
**CannonTurret** ✅ Just a simple cannon that fires a large, heavy projectile. Fairly good range. Does damage to surrounding invaders when it hits the ground and creates an explosion.

**BoomerangTurret** ❎ Very similar to the cannon turret, except that the projectile will arc back towards the tower. This lets the projectiles land on the opposite side of a defensive wall

**ClusterTurret** ✅ Fires a large round out over the terrain. It explodes in mid-air, dropping a bunch of smaller projectiles that fall to the ground and explode on impact

**FelCannonTurret** ❎ A large ball of energy that shoots out lightning bolts that chain from one invader to the next upon impact with the ground

**FlameThrowerTurret** ❎ Very short range, but devastating. Mostly used in the bottom most slot. Works very well on invaders that have bypassed the shield already.

**GlueTurret** ❌ Medium range, fires a sticky glue onto the terrain that slows down invaders as they walk into it. 

**HarpoonTurret** ❌ This turret is meant to be used to retrieve powerups, boxes or crates that land on the terrain. Fire the harpoon out, when it's attached to the item, right click and yank back the crate to collect the item. Can also be used on vehicles or normal invaders to do damage and possibly mush them into the side of the tower.


### Light Projectile Turrets
**LightningTurret** ✅ Fires a lightning bolt. Instantly damaging the invader it hits, but also conducting out towards nearby invaders and creating a damage-over-time effect.

**MachineGunTurret** ✅ Simple. Infinite range, but not very accurate. Quite weak in terms of damage output. Overheats if used too much.

**ShotgunTurret** ✅ Burst of light projectiles with infinite range. Does more damage than the machine gun per-bullet.

**SniperTurret** ❌ 


### Beam Turrets
**BeamTurret** ✅ 

**FreezeTurret** ❎ A single burst beam that freezes an invader in place for a time when hit. Works similar to the ice trap, but it can be a guaranteed hit and a specific invader can be targetted as opposed to their only being a chance of an invader walking through an ice trap.

**PersistentBeamTurret** ❎ A high powered energy beam that does damage when contacting an invader. Mouse button needs to be held down to keep the beam active.


---
### Traps
Traps are placed directly onto the terrain in front of the tower. Some traps are triggered when an invader gets close to, or steps onto the trap. Others are triggered directly by the player and others are not triggered at all (Such as the Wall).
### Offensive Traps
**TriggerTrap** ❎

**BarrelTrap** ❎ A barrel placed onto the terrain that does nothing special when left alone, but will explode when the player shoots it with a turret, an explosion happens nearby (Whether from an invader or a player action)

**FireTrap** ✅ Simple, just a fire that does damage over time when an invader walks through it. Has 5 charges and is extinguished after those 5 are exhausted.

**FlameThrowerTrap** ✅ Pops up out of the ground, blocking the invaders path and spraying fire onto them doing damage over time. Is manually triggered in the current version, but the intention is to have it automatically trigger when approached.

**LandMineTrap** ✅

**PopFragTrap**❌

**SawBladeTrap** ✅

**SpikesTrap** ✅ 



### Defensive Traps
**CatapultTrap** ❎ When an invader steps onto this they are catapulted back towards the right of the screen, setting them back. Works well when combined with a wall trap as the invader is catapulted back over the wall and then has to break through the wall again.

**GlueTrap** ❎ 

**IceTrap** ✅ Freezes an invader in place when touched. Not as targetted as the Freeze Turret as it relies on chance of the invader walking into it whereas the Freeze Turret can be used on a specific invader that the player chooses.

**Wall** ✅ Pretty self explanatory. Just an obstacle that needs to be destroyed for the invader to progress. Has an interesting effect in that it also blocks the player from shooting the invaders on the other side of the wall with any sort of light projectile. To damage the invaders attacking a wall, the player needs to use a heavy, arcing projectile or other means such as gas or fire.


---

### Invaders
### Airborne
**DropShip** ✅ This airbone invader just flies in from the right to a point on screen where it then opens up the doors and drops out a collection of invaders. 

**Gunship** ✅ Flies in from the right, stops at a range from the tower whereupon the bays open and a series of missiles are fired directly at the tower. The implementation isn't good yet though because it does way too much, the sprite it far too big and it never runs out of ammo.

**HealerDrone** ✅ I'm very happy with the implementation of these so far. The drones spawn to the right, off-screen and then fly in. When an invader is hurt, not too close to the tower and within range the healer drone will fly up and start following the invader, hitting it with a healing beam that reverses damage the player has done. 

### Heavy Ranged
**Archer** ✅ 

**FireElemental** ✅ 

**FlameJetTrooper** ✅ 

**HarpoonCannon** ✅ This works really well and I'm very happy with the implementation, even though the visuals are terrible. This invader fires a harpoon attached to a rope. When the harpoon anchors to a turret, the rope is retracted until it is pulled taut and then the invader reverses to a point that the turret can no longer hold on and it's ripped from the socket. It's devastating and it pulls the players focus and becomes top priority as soon as the harpoon is anchored.

**JumpMan** ✅ Basically a soldier/rifleman, but not hindered by defensive traps. Using a jetpack of sorts to jump over walls/traps.

**StationaryCannon** ✅ 

**Tank** ❎ 

**WaterElemental** ❌ 


### Light Ranged
**Healer** ❌ 

**RifleMan** ✅ Similar to the soldier, except armed with a gun. Moves up to a range from the tower and starts shooting with light projectiles.

**Sapper** ❌ 


### Melee
**BatteringRam** ✅ This invader requires two other invaders to operate. It can be spawned off-screen to the right at which point 2 soldier invaders are chosen to operate it and they start to move back towards the right of the screen. They walk off screen, collect the battering ram and then escort it towards the tower to do damage. It can also be dropped onto the terrain by a dropship at which point 2 invaders will retreat to operate it.

**Slime** ✅ 

**Soldier** ✅ Just a simple melee soldier. Squishy. Basically cannon fodder. Has an interesting effect in this game in that they absorb shots for anything standing behind them.

**Spider** ✅ 

**SuicideBomber** ✅ Does exactly what it says on the box. Fast moving, moves up close to the tower and detonates itself. Can move through the shield.

## Code Links
