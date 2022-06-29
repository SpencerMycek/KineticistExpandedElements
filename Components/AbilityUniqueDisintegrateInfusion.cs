using JetBrains.Annotations;
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

        bool RemoveAllDamage(List<BaseDamage> list)
        {
            return true;
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

                // Replace damage with 'doubled' or 'halved'
                Predicate<BaseDamage> RemoveAll = delegate (BaseDamage damage) { return true; };
                ruleDealDamage.Remove(RemoveAll);

                ContextAttackData contextAttackData = ContextData<ContextAttackData>.Current;
                RuleAttackRoll ruleAttackRoll = contextAttackData.AttackRoll;
                BaseDamage damage = baseDamage(context, ruleAttackRoll, evt.Target);

                ruleDealDamage.Add(damage);

                // Quarter for passed save
                RuleSavingThrow fort_save = fortSaveRule(context, evt.Target);
                context.TriggerRule(fort_save);
                if(fort_save.IsPassed)
                {
                    ruleDealDamage.Half = true;
                    ruleDealDamage.HalfBecauseSavingThrow = true;
                }
            } catch (Exception ex)
            {
                Helper.Print($"[AddForceBlastNullifyDamage] Exception: {ex.Message}");
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

        private BaseDamage baseDamage(MechanicsContext context, RuleAttackRoll ruleAttackRoll, UnitEntityData target)
        {
            DiceFormula dices = new DiceFormula(this.Value.DiceCountValue.Calculate(context), this.Value.DiceType);
            int bolsteredBonus = MetamagicHelper.GetBolsteredDamageBonus(context, dices);
            int bonus = this.Value.BonusValue.Calculate(context);
            bool empower = context.HasMetamagic(Metamagic.Empower) || this.Empowered;
            bool maximize = context.HasMetamagic(Metamagic.Maximize) || this.Maximized;
            DamageCriticalModifierType? crit = (ruleAttackRoll.IsCriticalConfirmed) ? ((context != null) ? DamageCriticalModifierTypeExtension.FromInt(ruleAttackRoll.WeaponStats.CriticalMultiplier) : new DamageCriticalModifierType?(ruleAttackRoll.Weapon.Blueprint.CriticalModifier)) : null;

            BaseDamage baseDamage = this.DamageTypeDesc.GetDamageDescriptor(dices, bonus + bolsteredBonus).CreateDamage();
            baseDamage.EmpowerBonus = (empower ? 1.5f : baseDamage.EmpowerBonus);
            if (maximize)
                baseDamage.CalculationType = DamageCalculationType.Maximized;
            baseDamage.CriticalModifier = ((crit != null) ? new int?(crit.GetValueOrDefault().IntValue()) : null);
            baseDamage.SourceFact = ContextDataHelper.GetFact();
            return baseDamage;
        }

        private void SetMetamagics(MechanicsContext context, UnitEntityData caster)
        {
            var empower = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f5f3aa17dd579ff49879923fb7bc2adb"); // Empower
            var maximize = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("870d7e67e97a68f439155bdf465ea191"); // Maximize
            var empowerCheap = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f8d0f7099e73c95499830ec0a93e2eeb"); // EmpowerCheaper
            var maximizeCheap = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("b8f43f0040155c74abd1bc794dbec320"); // MaximizeCheaper

            BuffCollection buffs = caster.Buffs;
            if (buffs.GetBuff(empower) != null || buffs.GetBuff(empowerCheap) != null)
            {
                this.Empowered = true;
            } else
            {
                this.Empowered = false;
            }
            if (buffs.GetBuff(maximize) != null || buffs.GetBuff(maximizeCheap) != null)
            {
                this.Maximized = true;
            } else
            {
                this.Maximized = false;
            }
        }

        public ActionList Actions;

        public ContextDiceValue Value;

        private BlueprintAbilityReference[] m_AbilityList;

        private TimeSpan m_LastFrameTime;

        private readonly HashSet<UnitEntityData> m_AffectedThisFrame = new HashSet<UnitEntityData>();

        private DamageTypeDescription DamageTypeDesc = new DamageTypeDescription
        {
            Type = DamageType.Force,
            Common = new DamageTypeDescription.CommomData(),
            Physical = new DamageTypeDescription.PhysicalData()
        };

        private bool Empowered;
        private bool Maximized;
    }
}
