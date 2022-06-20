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
    class TemporaryHitPointsUnique : UnitBuffComponentDelegate<TemporaryHitPointsFromAbilityValueData>, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber
    {
        public override void OnActivate()
        {
            base.Owner.Stats.TemporaryHitPoints.RemoveAllModifiers();
            base.Data.Modifier = base.Owner.Stats.TemporaryHitPoints.AddModifier(this.Value.Calculate(base.Context), base.Runtime, this.Descriptor);
        }

        public override void OnDeactivate()
        {
            ModifiableValue.Modifier modifier = base.Data.Modifier;
            if (modifier != null)
            {
                modifier.Remove();
            }
            base.Data.Modifier = null;
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (this.RemoveWhenHitPointsEnd && base.Data.Modifier.AppliedTo == null)
            {
                base.Buff.Remove();
            }
        }

        public ModifierDescriptor Descriptor;
        public ContextValue Value;
        public bool RemoveWhenHitPointsEnd;
    }
}
