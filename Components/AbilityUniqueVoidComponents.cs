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

namespace KineticistElementsExpanded.Components
{
    public class AbilityUniqueGraviticBoost : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, IInitiatorRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDrainEnergy>
    {

        public AbilityUniqueGraviticBoost(params BlueprintAbilityReference[] list)
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

        public void ApplyAboutToTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {
            try
            {
                RuleDealDamage ruleDealDamage = evt as RuleDealDamage;
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

                BaseDamage damage = ruleDealDamage.DamageBundle.First;
                damage.ReplaceDice(new DiceFormula(damage.Dice.Rolls, DiceType.D8));

            }
            catch (Exception ex)
            {
                Helper.PrintNotification($"[AddForceBlastNullifyDamage] Exception: {ex.Message}");
            }
        }

        public void ApplyDidTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {
        }

        #region Triggers

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventAboutToTrigger(RuleDealStatDamage evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventAboutToTrigger(RuleDrainEnergy evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDealStatDamage evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDrainEnergy evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        #endregion

        public BlueprintAbilityReference[] m_AbilityList;
    }

    public class AbilityUniqueNegativeAdmixture : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, IInitiatorRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDrainEnergy>
    {

        public AbilityUniqueNegativeAdmixture(params BlueprintAbilityReference[] list)
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

        public void ApplyAboutToTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {
            try
            {
                RuleDealDamage ruleDealDamage = evt as RuleDealDamage;
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

                BaseDamage damage = ruleDealDamage.DamageBundle.First;
                damage.ReplaceDice(new DiceFormula(damage.Dice.Rolls / 2, damage.Dice.Dice));

                DamageTypeDescription typeDescription = damage.CreateTypeDescription();
                typeDescription.Type = DamageType.Energy;
                typeDescription.Physical = null;
                typeDescription.Energy = DamageEnergyType.NegativeEnergy;

                BaseDamage newDamage = typeDescription.CreateDamage(damage.Dice, damage.Bonus);
                ruleDealDamage.Add(newDamage);

            }
            catch (Exception ex)
            {
                Helper.PrintNotification($"[AddForceBlastNullifyDamage] Exception: {ex.Message}");
            }
        }

        public void ApplyDidTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {
        }

        #region Triggers

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventAboutToTrigger(RuleDealStatDamage evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventAboutToTrigger(RuleDrainEnergy evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDealStatDamage evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDrainEnergy evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        #endregion

        public BlueprintAbilityReference[] m_AbilityList;
    }

    public class AbilityUniquePositiveAdmixture : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, IInitiatorRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDrainEnergy>
    {

        public AbilityUniquePositiveAdmixture(params BlueprintAbilityReference[] list)
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

        public void ApplyAboutToTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {
            try
            {
                RuleDealDamage ruleDealDamage = evt as RuleDealDamage;
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

                BaseDamage damage = ruleDealDamage.DamageBundle.First;
                damage.ReplaceDice(new DiceFormula(damage.Dice.Rolls / 2, damage.Dice.Dice));

                DamageTypeDescription typeDescription = damage.CreateTypeDescription();
                typeDescription.Type = DamageType.Energy;
                typeDescription.Physical = null;
                typeDescription.Energy = DamageEnergyType.PositiveEnergy;

                BaseDamage newDamage = typeDescription.CreateDamage(damage.Dice, damage.Bonus);
                ruleDealDamage.Add(newDamage);

            }
            catch (Exception ex)
            {
                Helper.PrintNotification($"[AddForceBlastNullifyDamage] Exception: {ex.Message}");
            }
        }

        public void ApplyDidTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {
        }

        #region Triggers

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventAboutToTrigger(RuleDealStatDamage evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventAboutToTrigger(RuleDrainEnergy evt)
        {
            this.ApplyAboutToTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDealStatDamage evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        public void OnEventDidTrigger(RuleDrainEnergy evt)
        {
            this.ApplyDidTrigger(evt, base.Context);
        }

        #endregion

        public BlueprintAbilityReference[] m_AbilityList;
    }

}
