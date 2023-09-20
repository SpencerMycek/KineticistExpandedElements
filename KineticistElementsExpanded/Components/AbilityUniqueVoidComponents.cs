using JetBrains.Annotations;
using CodexLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.FactLogic;
using Owlcat.Runtime.Core.Utils;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Class.Kineticist;

namespace KineticistElementsExpanded.Components
{
    public class AbilityUniqueGraviticBoost : AbilityUniqueBoost
    {

        public AbilityUniqueGraviticBoost(params BlueprintAbilityReference[] list)
        {
            this.m_AbilityList = list;
            this.ModifiyDice = true;
            this.AddBonus = false;
        }

        /*
         // Not Needed, keeping for reference
        [HarmonyPatch(typeof(PromoteSpellDices), nameof(PromoteSpellDices.OnEventAboutToTrigger))]
        class DragonBoostPatch
        {
            public static ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference> AbilityList
            {
                get
                {
                    return m_AbilityList;
                }
            }

            static void Postfix(RuleCalculateDamage evt, PromoteSpellDices __instance)
            {
                if (__instance.Owner.Descriptor.Buffs.GetBuff(boost) == null && __instance.Owner.Descriptor.Buffs.GetBuff(boost_greater) == null)
                {
                    return;
                }

                MechanicsContext context = evt.Reason.Context;
                AbilityExecutionContext abilityExecutionContext;
                if ((abilityExecutionContext = (context as AbilityExecutionContext)) != null) {
                    AbilityData ability = abilityExecutionContext.Ability;
                    if (AbilityList.Contains(ability.Blueprint))
                    {
                        IEnumerable<BlueprintAbility> source = AbilityList;
                        AbilityData convertedFrom = ability.ConvertedFrom;
                        if (!source.Contains((convertedFrom != null) ? convertedFrom.Blueprint : null))
                        {
                            return;
                        }
                    }
                } else
                {
                    AbilityData ability2 = evt.Reason.Ability;
                    BlueprintAbility blueprintAbility;
                    if ((blueprintAbility = ((ability2 != null) ? ability2.Blueprint : null)) == null)
                    {
                        MechanicsContext parentContext = context.ParentContext;
                        blueprintAbility = (((parentContext != null) ? parentContext.AssociatedBlueprint : null) as BlueprintAbility);
                    }
                    BlueprintAbility blueprintAbility2 = blueprintAbility;
                    if (blueprintAbility2 == null)
                    {
                        return;
                    }
                    if (!AbilityList.Contains(blueprintAbility2) && !AbilityList.Contains(blueprintAbility2.Parent))
                    {
                        return;
                    }
                }
                
                foreach (BaseDamage item in evt.DamageBundle)
                {
                    var dice = item.Dice.ModifiedValue.Dice;
                    var rolls = item.Dice.ModifiedValue.Rolls;
                    if (conversion_map.Keys.Contains(dice))
                    {
                        item.Dice.Modify(new DiceFormula(rolls, conversion_map[dice]), __instance.Fact);
                    }
                }
            }
            static Dictionary<DiceType, DiceType> conversion_map = new Dictionary<DiceType, DiceType>()
        {
            { DiceType.D6, DiceType.D8 },
            { DiceType.D8, DiceType.D10 },
            { DiceType.D10, DiceType.D12 },
        };
            static BlueprintBuff boost = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(KineticistTree.Instance.Boost_Gravitic.BoostActivatable.Get().m_Buff.Guid);
            static BlueprintBuff boost_greater = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(KineticistTree.Instance.Boost_GraviticGreater.BoostActivatable.Get().m_Buff.Guid);

            static BlueprintAbilityReference[] m_AbilityList = Array.ConvertAll(KineticistTree.Instance.GetAll(basic: true, composite: true, onlyPhysical: true).ToArray(), x => x.BaseAbility);
        }
        */
    }

    public class AbilityUniqueAethericBoost : AbilityUniqueBoost
    {

        public AbilityUniqueAethericBoost(params BlueprintAbilityReference[] list)
        {
            this.m_AbilityList = list;
            this.ModifiyDice = false;
            this.AddBonus = true;
            this.bonus = 1;
        }

    }

    public class AbilityUniqueBoost : UnitFactComponentDelegate, IRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookSubscriber, ISubscriber
    {

        public AbilityUniqueBoost(params BlueprintAbilityReference[] list)
        {
            this.m_AbilityList = list;
        }

        public ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference> AbilityList
        {
            get
            {
                return this.m_AbilityList;
            }
        }

        public void ApplyAboutToTrigger(RuleCalculateDamage evt, [NotNull] MechanicsContext context)
        {
            try
            {
                AbilityExecutionContext abilityExecutionContext;

                // Make sure we only triggered on allowed abilities
                if ((abilityExecutionContext = (context as AbilityExecutionContext)) != null)
                {
                    AbilityData ability = abilityExecutionContext.Ability;
                    if (!this.AbilityList.Contains(ability.Blueprint))
                    {
                        IEnumerable<BlueprintAbility> source = this.AbilityList;
                        AbilityData convertedFrom = ability.ConvertedFrom;
                        if (!source.Contains((convertedFrom != null) ? convertedFrom.Blueprint : null))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    AbilityData ability2 = evt.Reason.Ability;
                    BlueprintAbility blueprintAbility;
                    if ((blueprintAbility = ((ability2 != null) ? ability2.Blueprint : null)) == null)
                    {
                        MechanicsContext parentContext = context.ParentContext;
                        blueprintAbility = (((parentContext != null) ? parentContext.AssociatedBlueprint : null) as BlueprintAbility);
                    }
                    BlueprintAbility blueprintAbility2 = blueprintAbility;
                    if (blueprintAbility2 == null)
                    {
                        return;
                    }
                    if (!this.AbilityList.Contains(blueprintAbility2) && !this.AbilityList.Contains(blueprintAbility2.Parent))
                    {
                        return;
                    }
                }

                foreach (BaseDamage item in evt.DamageBundle)
                {
                    if (ModifiyDice)
                    {
                        var dice = item.Dice.ModifiedValue.Dice;
                        item.Dice.Modify(new DiceFormula(item.Dice.ModifiedValue.Rolls, conversion_map[dice]), base.Fact);
                    }
                    if (AddBonus)
                    {
                        item.AddModifier(new Modifier(1 * item.Dice.ModifiedValue.Rolls, ModifierDescriptor.None));
                    }
                }

            }
            catch (Exception ex)
            {
                Helper.PrintNotification($"[AethericBoost] Exception: {ex.Message}");
            }
        }

        public void ApplyDidTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {
        }

        #region Triggers

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        #endregion

        public BlueprintAbilityReference[] m_AbilityList;
        private Dictionary<DiceType, DiceType> conversion_map = new Dictionary<DiceType, DiceType>()
        {
            { DiceType.D6, DiceType.D8 },
            { DiceType.D8, DiceType.D10 },
            { DiceType.D10, DiceType.D12 },
        };


        public bool ModifiyDice;
        public bool AddBonus;

        [ConditionalShow("AddBonus")]
        public int bonus = 1;
    }


    public class AbilityUniqueNegativeAdmixture : AbilityUniqueAdmixture
    {

        public AbilityUniqueNegativeAdmixture(params BlueprintAbilityReference[] list)
        {
            this.m_AbilityList = list;
            this.Type = Helper.CreateDamageTypeDescription(DamageEnergyType.NegativeEnergy);

        }
    }

    public class AbilityUniquePositiveAdmixture : AbilityUniqueAdmixture
    {

        public AbilityUniquePositiveAdmixture(params BlueprintAbilityReference[] list)
        {
            this.m_AbilityList = list;
            this.Type = Helper.CreateDamageTypeDescription(DamageEnergyType.PositiveEnergy);
        }
    }

    public class AbilityUniqueAdmixture : UnitFactComponentDelegate, IRulebookHandler<RulePrepareDamage>, IInitiatorRulebookHandler<RulePrepareDamage>, IInitiatorRulebookSubscriber, ISubscriber
    {

        public AbilityUniqueAdmixture(params BlueprintAbilityReference[] list)
        {
            this.m_AbilityList = list;
        }

        public ReferenceArrayProxy<BlueprintAbility, BlueprintAbilityReference> AbilityList
        {
            get
            {
                return this.m_AbilityList;
            }
        }

        public void ApplyAboutToTrigger(RulePrepareDamage evt, [NotNull] MechanicsContext context)
        {
            try
            {
                AbilityExecutionContext abilityExecutionContext;

                // Make sure we only triggered on allowed abilities
                if ((abilityExecutionContext = (context as AbilityExecutionContext)) != null)
                {
                    AbilityData ability = abilityExecutionContext.Ability;
                    if (!this.AbilityList.Contains(ability.Blueprint))
                    {
                        IEnumerable<BlueprintAbility> source = this.AbilityList;
                        AbilityData convertedFrom = ability.ConvertedFrom;
                        if (!source.Contains((convertedFrom != null) ? convertedFrom.Blueprint : null))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    AbilityData ability2 = evt.Reason.Ability;
                    BlueprintAbility blueprintAbility;
                    if ((blueprintAbility = ((ability2 != null) ? ability2.Blueprint : null)) == null)
                    {
                        MechanicsContext parentContext = context.ParentContext;
                        blueprintAbility = (((parentContext != null) ? parentContext.AssociatedBlueprint : null) as BlueprintAbility);
                    }
                    BlueprintAbility blueprintAbility2 = blueprintAbility;
                    if (blueprintAbility2 == null)
                    {
                        return;
                    }
                    if (!this.AbilityList.Contains(blueprintAbility2) && !this.AbilityList.Contains(blueprintAbility2.Parent))
                    {
                        return;
                    }
                }

                var baseDamage = new DamageDescription
                {
                    TypeDescription = Type,
                    Dice = new DiceFormula(Value.DiceCountValue.Calculate(base.Context), Value.DiceType),
                    Bonus = Value.BonusValue.Calculate(base.Context),
                    SourceFact = base.Fact
                }.CreateDamage();
                baseDamage.CriticalModifier = evt.DamageBundle.First?.CriticalModifier;

                evt.Add(baseDamage);

            }
            catch (Exception ex)
            {
                Helper.PrintNotification($"[AbilityUniqueAdmixture] Exception: {ex.Message}");
                Main.Print($"[AbilityUniqueAdmixture] Exception: {ex.Message}");
            }
        }

        public void ApplyDidTrigger(RulePrepareDamage evt, [NotNull] MechanicsContext context)
        {
        }

        #region Triggers

        public void OnEventAboutToTrigger(RulePrepareDamage evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RulePrepareDamage evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        #endregion

        public BaseDamage ChangeType(BaseDamage damage)
        {
            if (damage.Type == Type.Type)
            {
                switch (damage.Type)
                {
                    case DamageType.Energy:
                        break;
                    case DamageType.Force:
                    case DamageType.Direct:
                    case DamageType.Untyped:
                        return damage;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if ((damage as EnergyDamage)?.EnergyType == Type.Energy)
                {
                    return damage;
                }
            }

            BaseDamage baseDamage = Type.CreateDamage(new ModifiableDiceFormula(damage.Dice), damage.Bonus);
            baseDamage.CopyFrom(damage);
            return baseDamage;
        }

        public BlueprintAbilityReference[] m_AbilityList;
        public DamageTypeDescription Type;
        public ContextDiceValue Value;
    }
}
