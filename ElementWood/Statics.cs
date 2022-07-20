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
            @"A phytokineticist adds {g|Encyclopedia:Lore_Nature}Lore Nature{/g} to her list of class skills.";

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

        protected static readonly string FleshofWoodDescription = @"Element: Wood
Type: defense (Su)
Level: —
Burn: 0
Your skin toughens like timber, turning aside some blows. You gain a +1 enhancement bonus to your existing natural armor bonus. By accepting 1 point of burn, you can increase this enhancement bonus by 1. For every 3 levels beyond 2nd, you can accept 1 additional point of burn to further increase this enhancement bonus by 1 (to a maximum of +7 at 17th level).";

    }
}
