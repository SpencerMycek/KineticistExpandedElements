using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Units;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.GameModes.GameModeType;
using static Kingmaker.UnitLogic.Mechanics.Properties.BlueprintUnitProperty;

namespace KineticistElementsExpanded.Components
{
    class RegenTempHpPerMinute : UnitBuffComponentDelegate, ITickEachRound
    {
        private int m_Counter;
        private BlueprintCharacterClass m_Class;
        private BlueprintFeature m_Feature;

        public ModifierDescriptor Descriptor;

        public PropertyValueGetter feature_getter;
        public PropertyValueGetter class_getter;

        public RegenTempHpPerMinute (BlueprintCharacterClass m_Class, BlueprintFeature m_Feature, PropertyValueGetter feature_getter, PropertyValueGetter class_getter)
        {
            this.m_Counter = 0;
            this.m_Class = m_Class;
            this.m_Feature = m_Feature;
            this.feature_getter = feature_getter;
            this.class_getter = class_getter;
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
                var feature_ranks = feature_getter.GetValue(this.Owner);
                var class_lvl = class_getter.GetValue(this.Owner);
                int maxTempHp = class_lvl * feature_ranks;
                int regenAmount = feature_ranks/2;
                var currentTempHp = tempHp.ModifiedValue;

                int value = (currentTempHp + regenAmount) > maxTempHp ? maxTempHp : currentTempHp + regenAmount;
                tempHp.RemoveAllModifiers();
                tempHp.AddModifier(value, base.Runtime, this.Descriptor);
                m_Counter = 1;
                return;
            }
        }
    }
}
