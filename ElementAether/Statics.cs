using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KineticistElementsExpanded.ElementAether
{
    class Statics
    {
        protected static readonly string ElementalFocusAetherGuid = "6AA8A023-FC1D-4DAD-B6C2-7CC01B7BF48D";
        protected static readonly string ElementalFocusAetherDescription =
            "Kineticists who focus on the element of aether—a rare substance formed when elemental"
            + " energy affects the Ethereal Plane—are called telekineticists. Telekineticists use"
            + " strands of aether to move objects with their minds.";

        protected static readonly string AetherClassSkillsDescription = "An Telekineticist adds"
            + " {g|Encyclopedia:Trickery}Trickery{/g} and {g|Encyclopedia:Knowledge_World}Knowledge"
            + " (World){/g} to her list of class {g|Encyclopedia:Skills}skills{/g}.";

        protected static readonly string TelekineticBlastDescription =
@"Element: aether
Type: simple blast
Level: --
Burn: 0
Blast Type: physical
{g|Encyclopedia:Damage}Damage{/g}: {g|Encyclopedia:Damage_Type}bludgeoning{/g}, piercing, and"
+ @"slashing
You throw a nearby unattended object enfolded in strands of aether at a single foe as a ranged attack";

        protected static readonly string ForceBlastDescription =
@"Element: aether
Type: composite blast
level: —
Burn: 2
Prerequisites: primary element (aether), expanded element (aether)
Blast Type: energy
{g|Encyclopedia:Damage}Damage{/g}: force
You throw a burst of force at a foe. Force blast deals damage as a simple energy blast instead of a composite energy blast.";


        protected static readonly string ForceWardDescription =
@"You constantly surround yourself with a ward of force. You gain a number of temporary hit points equal to your kineticist level.

These temporary hit points regenerate at a rate of 1 per minute. By accepting 1 point of burn as a standard action, you can increase the maximum number of temporary hit points provided by your force ward by half your kineticist level until the next time your burn is removed.
If you use this ability multiple times, the increases stack. For every two times you use this ability, the regeneration effect increases by 1.";


        protected static readonly string TelekineticInvisibilityDescription =
@"You weave strands of aether, bending light and dampening sound; This works as invisibility except that the aetheric bending is easier to notice than normal invisibility, so your bonus on Stealth checks is halved (+10 while moving and +20 while perfectly still).";

        protected static readonly string TelekineticFinesseDescription =
@"A Telekineticist can use {g|Encyclopedia:Trickery}Trickery{/g} at a range of 30 feet.";

        protected static readonly string TelekineticManeuversDescription =
@"A Telekineticist can perform combat maneuvers (trip, disarm, bullrush, pull) at range using your main Kineticist stat. 
If you possess the Telekinetic Finesse wild talent, add Dirty Trick to the list of combat maneuvers you can perform; using Dexterity instead of your main Kineticist stat.";

        protected static readonly string TelekineticManeuversPullDescription =
@"A pull attempts to pull an opponent straight towards you without doing any harm. If your {g|Encyclopedia:Combat_Maneuvers}combat maneuver{/g} is successful, your target is pulled 5 feet. For every 5 by which your {g|Encyclopedia:Attack}attack{/g} exceeds your opponent's {g|Encyclopedia:CMD}CMD{/g}, you can pull the target an additional 5 feet.
An enemy being moved by a pull does not provoke an {g|Encyclopedia:Attack_Of_Opportunity}attack of opportunity{/g} because of the movement. You cannot pull a creature into a square that is occupied by a solid object or obstacle.";

        protected static readonly string TouchsiteReactiveDescription =
@"Your strands of aether surround you, making it virtually impossible for a violent motion to catch you off guard, so long as the motion originates within their range. The character can react to danger before her senses would normally allow her to do so. She cannot be caught {g|Encyclopedia:Flat_Footed}flat-footed{/g}, nor does she lose her {g|Encyclopedia:Dexterity}Dexterity{/g} {g|Encyclopedia:Bonus}bonus{/g} to {g|Encyclopedia:Armor_Class}AC{/g} if the {g|Encyclopedia:Attack}attacker{/g} is invisible. She still loses her Dexterity bonus to AC if immobilized. A character with this ability can still lose her Dexterity bonus to AC if an opponent successfully uses the feint {g|Encyclopedia:CA_Types}action{/g} against her.";

        protected static readonly string SelfTelekinesisDescription =
@"You can use your telekinetic abilities to move yourself. As a move action you can cause yourself to hover over the ground, granting yourself a +3 dodge {g|Encyclopedia: Bonus}bonus{/g} to {g|Encyclopedia:Armor_Class}AC{/g} against {g|Encyclopedia:MeleeAttack}melee attacks{/g} and immunity to ground based effects, such as difficult terrain.";

        protected static readonly string SelfTelekinesisGreaterDescription =
@"You have greater control over you self telekinesis. You can hover over the ground without expending an action, granting yourself a +3 dodge {g|Encyclopedia: Bonus}bonus{/g} to {g|Encyclopedia:Armor_Class}AC{/g} against {g|Encyclopedia:MeleeAttack}melee attacks{/g} and immunity to ground based effects, such as difficult terrain.";

        protected static readonly string SpellDeflectionDescription =
@"You weave strands of aether around yourself in order to deflect magic. Until the beginning of your next turn, gain Spell Resistance equal to 10 + your kineticist level. You can accept 1 point of burn to increase the duration to 10 minutes per kineticist level you possess.";

        protected static readonly string KineticBladeDescription =
@"Element: universal
Type: form infusion
Level: 1
Burn: 1
Associated Blasts: any
{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: none
You form a weapon using your kinetic abilities. You create a nonreach, light or {g|Encyclopedia:Light_Weapon}one-handed{/g} weapon in your hand formed of pure energy or elemental matter. The kinetic blade's shape is purely cosmetic and doesn't affect the {g|Encyclopedia:Damage}damage{/g} dice, {g|Encyclopedia:Critical}critical{/g} threat range, or critical multiplier of the kinetic blade, nor does it grant the kinetic blade any weapon special features.
You can use this form infusion once as part of an {g|Encyclopedia:Attack}attack{/g} {g|Encyclopedia:CA_Types}action{/g}, a charge action, or a full-attack action in order to make {g|Encyclopedia:MeleeAttack}melee attacks{/g} with your kinetic blade. Since it's part of another action (and isn't an action itself), using this wild talent doesn't provoke any additional {g|Encyclopedia:Attack_Of_Opportunity}attacks of opportunity{/g}. The kinetic blade deals your kinetic blast damage on each hit (applying any modifiers to your kinetic blast's damage as normal, but not your {g|Encyclopedia:Strength}Strength{/g} modifier). The blade disappears at the end of your turn. The weapon deals the same {g|Encyclopedia:Damage_Type}damage type{/g} that your kinetic blast deals, and it interacts with {g|Encyclopedia:Armor_Class}Armor Class{/g} and {g|Encyclopedia:Spell_Resistance}spell resistance{/g} as normal for a blast of its type. The kinetic blade doesn't add the damage {g|Encyclopedia:Bonus}bonus{/g} from elemental overflow.";

        protected static readonly string DisintegratingInfusionDescription =
@"Element: aether
Type: substance infusion
Level: 6
Burn: 4,
Associated Blasts: force
{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Fortitude
You can use force to rip your targets apart. Against creatures, your kinetic blast deals double its normal amount of damage, but targets receive a saving throw to reduce the damage to half the blast’s normal amount of damage (for a total of 1/4 of the blast’s increased damage). Any creature reduced to 0 or fewer hit points by the blast is disintegrated, as the spell disintegrate. ";
    }
}
