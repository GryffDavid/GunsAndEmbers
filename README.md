# Guns and Embers
This Readme is essentially a design outline of the game. Although the game is not complete, most of the features have been implemented. No final art has been produced and everything is just placeholder, programmer art. I'm very much a designer and programmer and definitely not an artist. I am however happy with the [particle effects](https://github.com/GryffDavid/MultithreadedParticles) and the programmatically generated effects such as the [lightning](https://github.com/GryffDavid/StylizedLightning), [smoke](https://github.com/GryffDavid/VectorSmoke) and [grass](https://github.com/GryffDavid/2DDynamicGrass).

The project was initially started in 2013, about a year into my using C# and XNA. I worked on it fairly consistently until about late-2017 when I set it aside due to burnout after only really taking Christmas days off for 4 years straight.

## Table of Contents

- [Mechanics and Gameplay](#Mechanics-and-Gameplay)
- [Gifs, Videos, Screenshots](#Gameplay)
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
- [What is missing](#What-is-missing)
- [What I Learned](#What-I-learned)
- [Code Links](#Code-Links)

## Mechanics and Gameplay
This is a non-traditional tower defence game. Players control a tower to which they can attach turrets to repel incoming invaders. Traps can be placed on the terrain in front of the tower. The turrets work in a manner very similar to artillery games such as Worms or Scorched Earth. Players choose a loadout from an assortment of traps and turrets before beginning the level. Invaders of various types approach the tower in escalating waves. This lead to quite fast-paced, escalating gameplay as traps need to be replaced between bouts of artillery fire. Quick decisions about trap placement are essential and certain traps and turrets can be used together strategically to devastate incoming invaders. Such as careful use of a freeze turret and a flame trap to lock an invader in place in the flames, dealing multiple rounds of high damage-over-time. Tactical placement of the traps can be thought of the same as the trap placement mechanics in Orcs Must Die and its sequel. Placement of the turrets into the slots on the tower is also important. Placing a heavy artillery turret in the top-most slot will increase it's range just based on physics alone - the projectile will have more time to move in the X axis as it falls along the Y axis due to gravity. So placement of turrets on the tower involves the player making a tactical decision.

## Gameplay

## Features implemented
Still working on this, so I'm not totally sure if the keying is accurate as of October 2020.
- ❌ Feature is not implemented
- ✅ Feature is implemented and currently working
- ❎ Feature has been implemented successfully in the past, but does not currently work
### Turrets

#### Grenade Turrets
##### StickyMineTurret ❌
##### GasGrenadeTurret ❎
##### GrenadeTurret ❎

### Heavy Projectile Turrets
**CannonTurret** ✅ Just a simple cannon that fires a large, heavy projectile. Fairly good range. Does damage to surrounding invaders when it hits the ground and creates an explosion.

**BoomerangTurret** ❎ Very similar to the cannon turret, except that the projectile will arc back towards the tower. This lets the projectiles land on the opposite side of a defensive wall

**ClusterTurret** ❎ 

**FelCannonTurret** ❎ 

**FlameThrowerTurret** ❎ Very short range, but devastating. Mostly used in the bottom most slot.

**GlueTurret** ❌ Medium range, fires a sticky glue onto the terrain that slows down invaders as they walk into it. 

**GrappleTurret** ❌

**HarpoonTurret** ❌


### Light Projectile Turrets
**LightningTurret** ✅ Fires a lightning bolt. Instantly damaging the invader it hits, but also conducting out towards nearby invaders and creating a damage-over-time effect.

**MachineGunTurret** ✅ Simple. Infinite range, but not very accurate. Quite weak in terms of damage output. Overheats if used too much.

**ShotgunTurret** ✅ Burst of light projectiles with infinite range. Does more damage than the machine gun per-bullet.

**SniperTurret** ❌


### Beam Turrets
**BeamTurret** ✅

**FreezeTurret** ❎

**PersistentBeamTurret** ❎


---
### Traps
Traps are placed directly onto the terrain in front of the tower. Some traps are triggered when an invader gets close to, or steps onto the trap. Others are triggered directly by the player and others are not triggered at all (Such as the Wall).
### Offensive Traps
**TriggerTrap** ❎

**BarrelTrap** ❎

**FireTrap** ✅

**FlameThrowerTrap** ✅

**LandMineTrap** ✅

**PopFragTrap**❌

**SawBladeTrap** ✅

**SpikesTrap** ✅ 



### Defensive Traps
**CatapultTrap** ❎

**GlueTrap** ❎

**IceTrap** ✅ 

**Wall** ✅ 


---

### Invaders
### Airborne
**DropShip** ✅ 

**Gunship** ✅ 

**HealerDrone** ✅ 


### Heavy Ranged
**Archer** ✅ 

**FireElemental** ✅ 

**FlameJetTrooper** ✅ 

**Harpooncannon** ✅ 

**JumpMan** ✅ 

**StationaryCannon** ✅ 

**Tank** ❎ 

**WaterElemental** ❌ 


### Light Ranged
**Healer** ❌ 

**RifleMan** ✅ 

**Sapper** ❌ 


### Melee
**BatteringRam** ✅ This invader requires two other invaders to operate. It can be spawned off-screen to the right at which point 2 soldier invaders are chosen to operate it and they start to move back towards the right of the screen. They walk off screen, collect the battering ram and then escort it towards the tower to do damage. It can also be dropped onto the terrain by a dropship at which point 2 invaders will retreat to operate it.

**Slime** ✅ 

**Soldier** ✅ 

**Spider** ✅ 

**SuicideBomber** ✅ Does exactly what it says on the box. Fast moving, moves up close to the tower and detonates itself. Can move through the shield.


## What is missing
Tutorial. Any sort of story. A proper structure to the levels and invaders.

## What I learned
Learned a lot about design from a practical point of view.

## Code Links
