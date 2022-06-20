using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KineticistElementsExpanded.Components.Properties
{
    public class FeatureRankPlus1Getter : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
            var unitFeatureRank = unit?.Progression.Features.GetRank(Feature);
            int value = unitFeatureRank ?? 0;
            return value + 1;
        }

        public BlueprintFeatureReference Feature;
    }

    public class BuffRankPlus1Getter : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
            var unitFeatureRank = unit?.Buffs.GetBuff(Buff).Rank;
            int value = unitFeatureRank ?? 0;
            return value + 1;
        }

        public BlueprintBuffReference Buff;
    }

    public class ClassLevelGetter : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
            var classlvl = unit?.Progression.GetClassLevel(ClassRef);
            int value = classlvl ?? 0;
            return value;
        }

        public BlueprintCharacterClassReference ClassRef;
    }
}
