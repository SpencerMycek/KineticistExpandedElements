using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
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
    class AbilityUniqueDisintegrateInfusion : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, IInitiatorRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDrainEnergy>
    {

        public AbilityUniqueDisintegrateInfusion(params BlueprintAbilityReference[] list)
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

        private void RunAction(UnitEntityData target, MechanicsContext context)
        {
            TimeSpan gameTime = Game.Instance.TimeController.GameTime;
            if (gameTime != this.m_LastFrameTime)
            {
                this.m_LastFrameTime = gameTime;
                this.m_AffectedThisFrame.Clear();
            }
            if (!this.m_AffectedThisFrame.Add(target))
            {
                return;
            }
            using (context.GetDataScope(target))
            {
                this.Actions.Run();
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

                RuleSavingThrow fort_save = fortSaveRule(context, evt.Target);
                context.TriggerRule(fort_save);
                if (fort_save.IsPassed)
                {
                    ruleDealDamage.HalfBecauseSavingThrow = true;
                }
                else
                {
                    foreach (BaseDamage item in ruleDealDamage.DamageBundle)
                    {
                        var rolls = item.Dice.ModifiedValue.Rolls;
                        var dice = item.Dice.ModifiedValue.Dice;

                        item.Dice.Modify(new DiceFormula(rolls * 2, dice), base.Fact);
                    }
                }

            }
            catch (Exception ex)
            {
                Main.PrintException(ex);
            }
        }

        public void ApplyDidTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {

            this.RunAction(evt.Target, context);
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

        private RuleSavingThrow fortSaveRule(MechanicsContext context, UnitEntityData unit)
        {
            RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(unit, SavingThrowType.Fortitude, context.Params.DC);
            ruleSavingThrow.Reason = context;
            ruleSavingThrow.Buff = null;
            ruleSavingThrow.PersistentSpell = false;
            return ruleSavingThrow;
        }

        public ActionList Actions;

        public ContextDiceValue Value;

        private BlueprintAbilityReference[] m_AbilityList;

        private TimeSpan m_LastFrameTime;

        private readonly HashSet<UnitEntityData> m_AffectedThisFrame = new HashSet<UnitEntityData>();

    }
}
