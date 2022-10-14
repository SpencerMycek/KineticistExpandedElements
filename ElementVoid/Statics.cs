using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KineticistElementsExpanded.ElementVoid
{
    class Statics
    {
        protected static readonly string ElementalFocusVoidDescription =
            @"Like aether, void forms where elemental energy meets another material, in this case the substance of the Negative Energy Plane. Kineticists who command this strange force are referred to as chaokineticists.";

        protected static readonly string VoidClassSkillsDescription =
            @"An Chaokineticist adds {g|Encyclopedia:Knowledge_World}Knowledge (World){/g} and {g|Encyclopedia:Mobility}Mobility{/g} to her list of class {g|Encyclopedia:Skills}skills{/g}.";

        protected static readonly string GravityBlastDescription =
            @"Element: Void\nType: simple blast\nLevel: —\nBurn: 0\nBlast Type: physical\n{g|Encyclopedia:Damage}Damage{/g}: {g|Encyclopedia:Damage_Type}bludgeoning{/g}\nYou manipulate gravity to distort and buffet your foe’s body.";

        protected static readonly string NegativeBlastDescription =
            @"Element: Void\nType: simple blast\nLevel: —\nBurn: 0\nBlast Type: physical\n{g|Encyclopedia:Damage}Damage{/g}: negative energy\nYou blast your foe with negative energy.";

        protected static readonly string EmptinessDescription = @"Element: Void\nType: defense\nLevel: —\nBurn: 0\n{g|Encyclopedia:Damage}Damage{/g}: negative energy\nYour body becomes an empty husk, fueled by an internal void. You gain negative energy resistance 2, a 5% chance to ignore critical hits and sneak attacks, and a +1 bonus on Will saves against emotion effects. By accepting 1 point of burn, you can increase the resistance to negative energy by 2, the chance to ignore critical hits and sneak attacks by 5%, and the bonus on Will saves against emotion effects by 1 until the next time your burn is removed";

        protected static readonly string CorpsePuppetDescription = @"Element: Void\nType: utility\nLevel: 4\nBurn: 1\nYou draw upon negative energy to animate a nearby corpse. This ability functions as Animate Dead";

        protected static readonly string CurseBreakerDescription = @"Element: Void\nType: utility\nLevel: 4\nBurn: 1\nPrerequisite: emptiness\nThe power of void protects you from curses and allows you to remove or suppress them. You gain a +4 bonus on saving throws against curses and hexes. You can accept 1 point of burn as a standard action to attempt a caster level check to remove a curse on yourself or another as per remove curse.";

        protected static readonly string GravityControlDescription = @"Element: Void\nType: utility\nLevel: 3\nBurn: 0\nPrerequisite: emptiness\nYou use your gravitic abilities to move yourself, as per flame jet.";

        protected static readonly string GravityControlGreaterDescription = @"Element: Void\nType: utility\nLevel: 3\nBurn: 0\nPrerequisite: emptiness\nYou use your gravitic abilities to move yourself, as per greater flame jet.";

        protected static readonly string VoidHealerDescription = @"Element: Void\nType: utility\nLevel: 1\nBurn: 1\nThis functions as kinetic healer, except it heals undead (and others healed by negative energy).";


        protected static readonly string UndeadGripDescription = @"Element: Void\nType: utility\nLevel: 3\nBurn: 1\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Will negates\nSpell Resistance: yes\nThis functions as hold monster, except it only works on undead creatures.";

        protected static readonly string KineticBladeDescription =
@"Element: universal
Type: form infusion
Level: 1
Burn: 1
Associated Blasts: any
{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: none
You form a weapon using your kinetic abilities. You create a nonreach, light or {g|Encyclopedia:Light_Weapon}one-handed{/g} weapon in your hand formed of pure energy or elemental matter. The kinetic blade's shape is purely cosmetic and doesn't affect the {g|Encyclopedia:Damage}damage{/g} dice, {g|Encyclopedia:Critical}critical{/g} threat range, or critical multiplier of the kinetic blade, nor does it grant the kinetic blade any weapon special features.
You can use this form infusion once as part of an {g|Encyclopedia:Attack}attack{/g} {g|Encyclopedia:CA_Types}action{/g}, a charge action, or a full-attack action in order to make {g|Encyclopedia:MeleeAttack}melee attacks{/g} with your kinetic blade. Since it's part of another action (and isn't an action itself), using this wild talent doesn't provoke any additional {g|Encyclopedia:Attack_Of_Opportunity}attacks of opportunity{/g}. The kinetic blade deals your kinetic blast damage on each hit (applying any modifiers to your kinetic blast's damage as normal, but not your {g|Encyclopedia:Strength}Strength{/g} modifier). The blade disappears at the end of your turn. The weapon deals the same {g|Encyclopedia:Damage_Type}damage type{/g} that your kinetic blast deals, and it interacts with {g|Encyclopedia:Armor_Class}Armor Class{/g} and {g|Encyclopedia:Spell_Resistance}spell resistance{/g} as normal for a blast of its type. The kinetic blade doesn't add the damage {g|Encyclopedia:Bonus}bonus{/g} from elemental overflow.";

        protected static readonly string DampeningInfusionDescription = @"Element: void\nType: substance infusion\nLevel: 1\nBurn: 1\nAssociated Blasts: negative, void\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Will negates\nYour kinetic blast swirls with darkness, making it harder for your foes to see. This otherwise functions as dazzling infusion.";

        protected static readonly string EnervatingInfusionDescription = @"Element: void\nType: substance infusion\nLevel: 7\nBurn: 4\nAssociated Blasts: negative, void\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Fortitude negates\nYour kinetic blast drains life force. Foes that take damage from your infused blast also take 1 temporary negative level. Negative levels from this infusion fade after 24 hours and never become permanent.";

        protected static readonly string void_wild_talent_name = "Kineticist Bonus Feat — Void";
        protected static readonly string void_wild_talent_description = "A character that selects this talent gains a bonus {g|Encyclopedia:Feat}feat{/g}. These feats must be taken from the following list: Spell Penetration, Greater Spell Penetration, Precise Shot, Trip, Greater Trip";

        protected static readonly string PullingInfusionDescription = @"Element: void\nType: substance infusion\nLevel: 1\nBurn: 1\nAssociated Blasts: gravity, void\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: none\nYour kinetic blast pulls foes toward you. Attempt a drag combat maneuver check against each target damaged by your infused blast (the blast always drags the foe closer to you), using your Constitution modifier instead of your Strength modifier to determine your CMB.";

        protected static readonly string SingularityInfusionDescription = @"Element: void\nType: form infusion\nLevel: 4\nBurn: 3\nAssociated Blasts: gravity, negative, void\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Reflex half\nYou create a growing singularity. Choose a grid intersection within 30 feet. All creatures and objects in a 5-foot-radius burst centered on the intersection take 1/4 your blast’s normal amount of damage (or half damage for a negative blast). On your next turn, the singularity deals damage in a 10-foot-radius burst, and on your turn after that, it deals damage in a 15-foot-radius burst. The DC is Dexterity-based.";

        protected static readonly string TurningInfusionDescription = @"Element: void, wood\nType: substance infusion\nLevel: 4\nBurn: 3\nAssociated Blasts: positive, negative\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Will negates\nUndead that take damage from a turning blast must succeed at a Will save or flee for 1 round.";

        protected static readonly string UnnervingInfusionDescription = @"Element: void\nType: substance infusion\nLevel: 3\nBurn: 2\nAssociated Blasts: negative, void\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Will negates\nYour kinetic blast sends the fear of oblivion into your foes. Whenever an infused blast deals negative energy damage to a living foe, it is shaken for 1 round. ";

        protected static readonly string VampiricInfusionDescription = @"Element: void\nType: substance infusion\nLevel: 5\nBurn: 3\nPrerequisite: Kinetic Healer\nAssociated Blasts: negative, void\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: none\nYour kinetic blast can drain your foes’ vitality to replenish your own. If your blast hits (or the enemy fails its saving throw against a blast without an attack roll), you're next activation of the Kinetic Healer Wild Talent is free.";

        protected static readonly string WeighingInfusionDescription = @"Element: void\nType: substance infusion\nLevel: 2\nBurn: 2\nAssociated Blasts: gravity, void\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Reflex negates\nThis infusion functions as entangling infusion, except it entangles and immobilizes a foe by increasing its weight, rather than surrounding it in elemental matter.";

        protected static readonly string VoidBlastDescription = @"Element: void\nType: composite blast\nLevel: —\nBurn: 2\nPrerequisite(s): primary element (void), expanded element (void)\nBlast Type: physical\n{g|Encyclopedia:Damage}Damage{/g}: half bludgeoning, half negative energy\nYou call forth the power of the void to annihilate your foe.";

        protected static readonly string GraviticBoostDescription = @"Element: void\nType: composite blast\nLevel: —\nBurn: 2\nPrerequisite(s): gravity blast, any other physical simple blast\nBlast Type: special\n{g|Encyclopedia:Damage}Damage{/g}: see text\nYou infuse a simple physical blast with added gravity, increasing its damage dice from d6s to d8s; it otherwise deals damage as per the simple blast.";

        protected static readonly string NegativeAdmixtureDescription = @"Element: void\nType: composite blast\nLevel: —\nBurn: 2\nPrerequisite(s): negative blast, any other energy simple blast\nBlast Type: special\n{g|Encyclopedia:Damage}Damage{/g}: see text\nYou infuse a simple physical Choose another simple energy blast you know. Negative admixture’s damage is half negative energy, and half the chosen blast’s type. with added gravity, increasing its damage dice from d6s to d8s; it otherwise deals damage as per the simple blast.";

    }
}
