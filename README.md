# Kineticist Elements Expanded

Mod for Pathfinder Wrath of the Righteous


## Credits and Acknowledgement

Thanks to [Truinto][1] author of [Dark Codex][2] for providing help, advice, and some methods so I could gain my own footing in C# and modding P:WoTR

Thanks to the entire OwlCat community discord and the modding community there, for answering my plethora of strange questions and pushing me in the right directions

## Description

This mod adds expanded element choices for the Kineticist class. Currently this mod adds only the "Aether" element from the Tabletop version of Pathfinder.
More elements may be coming in the future.

Added elements means: More blast types, More infusions, wild talents, Elemental Defense, etc.

This mod originally started as a personal project to add my favorite element, Aether, to the game. But I have expanded the scope to encompass any other elements others might want to add to the game.

If you have any suggestions for elements to add to the game, please reach out to me, write an issue for the repo, etc.

Thank you for reading!

I also do not have the skills currently to add custom effects to the game, so any element added will borrow the effect from other kineticist elements.
I shall try to match the theme/element of the ability as best as possible to make it visually appealing

## Content

| Element | Feature | Description | Status |
| :------ | :------ | :---------: | ------ |
| Aether | Element Choice | The main aether element, adds selections for base kineticist (and archetypes) and kinetic knight | Done |
| Aether | Basic Blast and Composite | Telekinetic and Force Blast - Including all vanilla infusions and Kinetic Blade | Done |
| Aether | Foe Throw | Infusion for Telekinetic Blast - Allows the kineticst to lift an enemy and throw them at another enemy | Done |
| Aether | Many Throw | Infusion for the Telekinetic Blast - Allows the kineticist to lift a multitude of objects and throw them at an equal number of targets | Done |
| Aether | Disintegrating Infusion | Infusion for Force Blast - Functions similarly to the disintegration spell | Done |
| Aether | Force Hook Infusion | Infusion for Force Blast - Allows the kineticist to drag themself to the target of their kinetic blast | Done |
| Aether | Aetheric Boost Composite | A composite blast that can be used on any other basic or composite blast | Not Done |
| Aether | Force Ward | Aether Elemental Defense - Adds a regenerating temporary hp buffer to the kineticist | Done |
| Aether | Expanded Defense and Existing Wild Talents | Wild talents in the vanilla game can now have Aether as a prerequisite | Done |
| Aether | Touchsite, Reactive | Wild Talent that provides the benefits of Uncanny Dodge | Done |
| Aether | Telekinetic Finesse | Wild Talent that allows the kineticist to perform Trickery at range, like Ranged Legerdemain from Arcane Trickster | Done |
| Aether | Telekinetic Maneuvers | Wild Talent that allows the kineticist to perform combat maneuvers at range | Done |
| Aether | Self Telekinesis | Wild Talent chain that allows to kineticist to lift themselves over the battlefield (flight) | Done |
| Aether | Spell Deflection | Wild Talent that allows the kineticist to form a barrier around themselves that provide resistance to spells (Spell Resistance) | Done |
| Aether | Suffocation | Wild Talent that allows the kineticist to choke a target from a distance | Not Done |

## Compatibility

This mod should be compatible with most other mods, as it only adds options to character choices.
Some mods might not integrate perfectly, but they should work together properly

If you notice any issues, please contact me or open up an issue on this repository

## Recent Changes

- Many Throw Infusion Added
- Force Hook Infusion Added
- Foe Throw Infusion Added
- Change: Telekinetic Blasts now use Battering Blast projectiles (Looks more "Force-y")
- Change: The display name of the extended Spell Deflection variant was changed to better distinguish itself

- Fix Wild Talents can now be picked with DarkCodex's ExtraWildTalents feat, if that mod is installed
- Force Blast now does Composite Energy Damage, instead of simple
- Many Foe and Foe Throw now deal the correct amount of damage
- Telekinetic Finesse now adds 1/2 kineticist level to Trickery

## Known Issues

### Disintigration Infusion
This is a very strange infusion, both doubling damage, and halving it. Unfortunately, in it's current state, it does not benefit from metakinesis, elemental overflow, or form infusions.
It does work correctly, when used with Extended Range and basic Force blast.


[1]: https://github.com/Truinto
[2]: https://github.com/Truinto/DarkCodex
