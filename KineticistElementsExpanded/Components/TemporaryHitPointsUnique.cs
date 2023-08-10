using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KineticistElementsExpanded.Components
{
    class TemporaryHitPointsUnique : UnitBuffComponentDelegate<TemporaryHitPointsFromAbilityValueData>
    {
        public override void OnActivate()
        {
            int value = Value.Calculate(base.Context);
            base.Owner.Stats.TemporaryHitPoints.RemoveAllModifiers();
            base.Data.Modifier = base.Owner.Stats.TemporaryHitPoints.AddModifier(value, base.Runtime, Descriptor);
        }

        public override void OnDeactivate()
        {
            base.Data.Modifier?.Remove();
            base.Data.Modifier = null;
        }

        public ModifierDescriptor Descriptor;
        public ContextValue Value;
        public bool RemoveWhenHitPointsEnd;
    }
}
