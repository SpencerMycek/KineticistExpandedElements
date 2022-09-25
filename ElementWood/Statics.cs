using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KineticistElementsExpanded.ElementWood
{
    class Statics
    {
        protected static readonly string ElementalFocusWoodDescription =
            @"Kineticists who focus on the concept of wood as an element are known as phytokineticists. Phytokineticists share a strong bond with the Fey World and channel the power of primordial life. As there is no “Elemental Plane of Wood,” the phytokineticist draws upon pockets of vital energy that form when the Elemental Planes grind against the borders of the Fey World.";
        protected static readonly string WoodClassSkillsDescription =
            @"A phytokineticist adds {g|Encyclopedia:Lore_Nature}Lore Nature{/g} and {g|Encyclopedia:Knowledge_World}Knowledge (World){/g} to her list of class skills.";

        protected static readonly string PositiveBlastDescription = @"Element: Wood
Type: simple blast
Level: —
Burn: 0
Blast Type: energy
{g|Encyclopedia:Damage}Damage{/g}: {g|Encyclopedia:Damage_Type}positive energy{/g}
You blast your foe with positive energy.";

        protected static readonly string WoodBlastDescription = @"Element: Wood
Type: simple blast
Level: —
Burn: 0
Blast Type: physical
{g|Encyclopedia:Damage}Damage{/g}: {g|Encyclopedia:Damage_Type}bludgeoning{/g}, piercing, and slashing
You lash out with boughs, vines, or a deluge of stinging blooms.";

        protected static readonly string VerdantBlastDescription = @"Element: Wood
Type: composite blast
Level: —
Burn: 2
Prerequisite(s): primary element(wood), expanded element(wood)
Blast Type: physical
{g|Encyclopedia:Damage}Damage{/g}: {g|Encyclopedia:Damage_Type}bludgeoning{/g}, piercing, slashing, and positive
You blast your foes with fanciful wild plant growth overflowing with positive energy from the First World. Verdant Blast’s damage counts as positive energy only when it would be beneficial to you.";

        protected static readonly string AutumnBlastDescription = @"Element: Earth and Wood
Type: composite blast
Level: —
Burn: 2
Prerequisite(s): earth blast, wood blast
Blast Type: physical
{g|Encyclopedia:Damage}Damage{/g}: {g|Encyclopedia:Damage_Type}bludgeoning{/g}, piercing, and slashing
A burst of fallen leaves and earthy decay batters a single foe.";

        protected static readonly string SpringBlastDescription = @"Element: Air and Wood
Type: composite blast
Level: —
Burn: 2
Prerequisite(s): air blast, wood blast
Blast Type: physical
{g|Encyclopedia:Damage}Damage{/g}: {g|Encyclopedia:Damage_Type}bludgeoning{/g}, piercing, and slashing
Sharp blossoms and pummeling seeds buffet your foe.";

        protected static readonly string WinterBlastDescription = @"Element: Water and Wood
Type: composite blast
Level: —
Burn: 2
Prerequisite(s): cold blast, wood blast
Blast Type: physical
{g|Encyclopedia:Damage}Damage{/g}: half cold and {g|Encyclopedia:Damage_Type}bludgeoning{/g}, piercing, and slashing 
You unleash a blast of frigid frost and jagged, bare branches at your target.";

        protected static readonly string SummerBlastDescription = @"Element: Fire and Wood
Type: composite blast
Level: —
Burn: 2
Prerequisite(s): fire blast, wood blast
Blast Type: physical
{g|Encyclopedia:Damage}Damage{/g}: half fire and {g|Encyclopedia:Damage_Type}bludgeoning{/g}, piercing, and slashing 
A burning blast of heat and sun-dried foliage tears at your foe.";

        protected static readonly string PositiveAdmixtureDescription = @"Element: Wood
Type: composite blast
Level: —
Burn: 2
Prerequisite(s): positive blast, any other simple energy blast
Blast Type: energy
{g|Encyclopedia:Damage}Damage{/g}: see text
Choose another energy simple blast you know. Positive admixture’s damage is half positive energy, and half the chosen blast’s type.";


        protected static readonly string KineticBladeDescription =
@"Element: universal
Type: form infusion
Level: 1
Burn: 1
Associated Blasts: any
{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: none
You form a weapon using your kinetic abilities. You create a nonreach, light or {g|Encyclopedia:Light_Weapon}one-handed{/g} weapon in your hand formed of pure energy or elemental matter. The kinetic blade's shape is purely cosmetic and doesn't affect the {g|Encyclopedia:Damage}damage{/g} dice, {g|Encyclopedia:Critical}critical{/g} threat range, or critical multiplier of the kinetic blade, nor does it grant the kinetic blade any weapon special features.
You can use this form infusion once as part of an {g|Encyclopedia:Attack}attack{/g} {g|Encyclopedia:CA_Types}action{/g}, a charge action, or a full-attack action in order to make {g|Encyclopedia:MeleeAttack}melee attacks{/g} with your kinetic blade. Since it's part of another action (and isn't an action itself), using this wild talent doesn't provoke any additional {g|Encyclopedia:Attack_Of_Opportunity}attacks of opportunity{/g}. The kinetic blade deals your kinetic blast damage on each hit (applying any modifiers to your kinetic blast's damage as normal, but not your {g|Encyclopedia:Strength}Strength{/g} modifier). The blade disappears at the end of your turn. The weapon deals the same {g|Encyclopedia:Damage_Type}damage type{/g} that your kinetic blast deals, and it interacts with {g|Encyclopedia:Armor_Class}Armor Class{/g} and {g|Encyclopedia:Spell_Resistance}spell resistance{/g} as normal for a blast of its type. The kinetic blade doesn't add the damage {g|Encyclopedia:Bonus}bonus{/g} from elemental overflow.";


        protected static readonly string FleshofWoodDescription = @"Element: Wood
Type: defense (Su)
Level: —
Burn: 0
Your skin toughens like timber, turning aside some blows. You gain a +1 enhancement bonus to your existing natural armor bonus. By accepting 1 point of burn, you can increase this enhancement bonus by 1. For every 3 levels beyond 2nd, you can accept 1 additional point of burn to further increase this enhancement bonus by 1 (to a maximum of +7 at 17th level).";

        protected static readonly string wood_wild_talent_name = "Kineticist Bonus Feat — Wood";
        protected static readonly string wood_wild_talent_description = "A character that selects this talent gains a bonus {g|Encyclopedia:Feat}feat{/g}. These feats must be taken from the following list: Spell Penetration, Greater Spell Penetration, Precise Shot, Trip, Greater Trip";

        protected static readonly string WoodHealerDescription = @"Element: Wood
Type: utility
Level: 1
Burn: 1
Prerequisites: positive blast
This functions as kinetic healer, but you must base the amount of healing on your positive blast, not wood blast. This wild talent counts as kinetic healer for prerequisites for selecting other kineticist wild talents.";

        protected static readonly string WoodlandStepDescription = @"Element: Wood
Type: utility
Level: 1
Burn: 0 
You can move through any sort difficult terrain at your normal {g|Encyclopedia:Speed}speed{/g} and without taking {g|Encyclopedia:Damage}damage{/g} or suffering any other impairment.";

        protected static readonly string ThornFleshDescription = @"Element: Wood
Type: utility
Level: 3
Burn: 1
Prerequisite(s): flesh of wood
You cover your body in barbed thorns that injure foes who strike you. Until the next time your burn is removed, while your flesh of wood is active, any creature that strikes you with an {g|Encyclopedia:UnarmedAttack}unarmed{/g} strike or {g|Encyclopedia:NaturalAttack}natural weapon{/g} or that grapples you takes {g|Encyclopedia:Dice}1d6{/g} points of {g|Encyclopedia:Damage_Type}piercing damage{/g}.";

        protected static readonly string HerbalAntivenomDescription = @"Element: Wood
Type: utility
Level: 8
Burn: 0
Your body can produce the herbal remedies necessary to counter almost any poison. You gain a +5 alchemical bonus on saving throws against poison as if always under the effect of antitoxin. 

In addition your knowledge in healing gives you a +5 bonus to Lore: Religion";

        protected static readonly string WildGrowthDescription = @"Element: Wood
Type: utility
Level: 8
Burn: 0
You cause tall grass, weeds, and other plants to wrap around creatures in the area of effect or those that enter the area. Creatures that fail their Reflex {g|Encyclopedia:Saving_Throw}save{/g} gain the entangled condition. Creatures that succeed at their save can move as normal, but those that remain in the area must save again each {g|Encyclopedia:Combat_Round}round{/g}. Creatures that move into the area must save immediately.
Entangled creatures can break free by making a {g|Encyclopedia:Combat_Maneuvers}combat maneuver{/g} {g|Encyclopedia:Check}check{/g}, {g|Encyclopedia:Athletics}Athletics check{/g}, or {g|Encyclopedia:Mobility}Mobility check{/g} against the {g|Encyclopedia:DC}DC{/g} of this ability every round.
The entire area of effect is considered difficult terrain while the effect lasts.";

        protected static readonly string ForestSiegeDescription = @"Element: Wood
Type: utility
Level: 9
Burn: 1
You can transform available plant life into a besieging army. This ability affects one plant for every three Kineticist Levels. 
After spending the burn to activity this ability, until the next time your burn is removed. You can spend a Standard action to cause nearby plants to act like heavy catapults, throwing rocks at a chosen target, dealing 6d6 Bludgeoning damage if they hit.";

        protected static readonly string WoodSoldiersDescription = @"Element: Wood
Type: utility
Level: 8
Burn: 1
Your presence animates surrounding plant life and causes it to fight by your side. You summon four wooden golems that last until the next time you recover burn. If you have the Spring, Summer, Autumn, or Winter blasts, your golems will gain unique upgrades.

Autumn Blast: Your Wood Soldiers gain resistance to physical damage
Spring Blast: Your Wood Soldiers gain a fly speed, which provides a +3 to AC and immunity to ground based effects
Summer Blast: Your Wood Soldiers gain resistance to fire, and deal 1d6 extra fire damage on all attacks
Winder Blast: Your Wood Soldiers gain resistance to cold,  and deal 1d6 extra cold damage on all attacks";

        protected static readonly string SporeInfusionDescription = @"Element: Wood
Type: substance infusion
Level: 5
Burn: 2
{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Fortitude negates
Associated Blasts: autumn, spring, summer, verdant, winter, wood
Creatures that take damage from your blast are infected with spores. If the target fails its Fortitude save, it takes 1d6 points of damage per round for 10 rounds as plants and fungi grow out of its body. At the end of that time, the target is exposed to the pulsing puffs disease. This infusion is a disease effect

    Pulsing Puffs: Blast—injury; save Fort DC 18; onset 1 minute; frequency 1/day; effect 1d6 Dex damage; cure 2 consecutive saves.";

        protected static readonly string ToxicInfusionDescription = @"Element: Wood
Type: substance infusion
Level: 4
Burn: 3
{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Fortitude negates
Associated Blasts: autumn, spring, summer, verdant, winter, wood
The plants in your blast are mildly toxic. All creatures that take piercing or slashing damage from your blast are sickened for 1 round.";

        protected static readonly string GreaterToxicInfusionDescription = @"Element: Wood
Type: substance infusion
Level: 7
Burn: 3
Prerequisite(s): toxic infusion
{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Fortitude negates
Associated Blasts: autumn, spring, summer, verdant, winter, wood
Your plant toxin is more virulent. All creatures that take piercing or slashing damage from your blast are exposed to your poison.

    Blast— injury; save Fort; frequency 1/round for 6 rounds; effect 1d2 Con damage; cure 2 consecutive saves.";

    }
}
