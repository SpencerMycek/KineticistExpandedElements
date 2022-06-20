using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Units;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KineticistElementsExpanded.Components
{
    class RegenTempHpPerMinute : UnitBuffComponentDelegate, ITickEachRound
    {
        private int m_Counter;
        private BlueprintCharacterClass m_Class;
        private BlueprintFeature m_Feature;

        public ModifierDescriptor Descriptor;

        public RegenTempHpPerMinute (BlueprintCharacterClass m_Class, BlueprintFeature m_Feature)
        {
            this.m_Counter = 0;
            this.m_Class = m_Class;
            this.m_Feature = m_Feature;
            Descriptor = ModifierDescriptor.UntypedStackable;
        }

        public void OnNewRound()
        {
            if (m_Counter % 10 != 0)
            {
                m_Counter += 1;
                return;
            }
            else
            {
                var tempHp = this.Owner.Stats.TemporaryHitPoints;
                var feature_ranks = this.Owner.Progression.Features.GetRank(m_Feature);
                var class_lvl = this.Owner.Progression.GetClassLevel(m_Class);
                int maxTempHp = (class_lvl*(feature_ranks+1))/2;
                int regenAmount = (feature_ranks/2)+1;
                var currentTempHp = tempHp.ModifiedValue;
                if (currentTempHp < maxTempHp)
                {
                    tempHp.RemoveAllModifiers();
                    tempHp.AddModifier(currentTempHp + regenAmount, base.Runtime, this.Descriptor);
                }
                m_Counter = 1;
                return;
            }
        }
    }
}
