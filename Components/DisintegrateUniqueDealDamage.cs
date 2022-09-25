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
    class DisintegrateUniqueDealDamage : ContextAction
    {
        public override string GetCaption()
        {
            return "Disintegrate Infusion Deal Damage";
        }

        public override void RunAction()
        {
            MechanicsContext context = base.Context;
            ContextAttackData contextAttackData = ContextData<ContextAttackData>.Current;
            RuleAttackRoll ruleAttackRoll = contextAttackData.AttackRoll;
            UnitEntityData caster = context.MaybeCaster;
            TargetWrapper target = context.MainTarget;
            RunInternal(context, ruleAttackRoll, caster, target.Unit);
        }

        public void RunInternal(MechanicsContext context, RuleAttackRoll ruleAttackRoll, UnitEntityData caster, UnitEntityData target)
        {
            // Trigger Saving throw
            // If Pass
            //  Full Damage
            // If Fail
            //  Quarter Damage
            RuleSavingThrow fort_save = fortSaveRule(context, target);
            context.TriggerRule(fort_save);
            RuleDealDamage dealDamage;
            if (fort_save.IsPassed)
            {
                dealDamage = damageRuleQuarter(context, ruleAttackRoll, target);
            }
            else
            {
                dealDamage = damageRule(context, ruleAttackRoll, target);
            }
            context.TriggerRule(dealDamage);
        }

        private RuleSavingThrow fortSaveRule(MechanicsContext context, UnitEntityData unit)
        {
            RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(unit, SavingThrowType.Fortitude, context.Params.DC);
            ruleSavingThrow.Reason = context;
            ruleSavingThrow.Buff = null;
            ruleSavingThrow.PersistentSpell = false;
            return ruleSavingThrow;
        }

        private RuleDealDamage damageRule(MechanicsContext context, RuleAttackRoll ruleAttackRoll, UnitEntityData target)
        {
            DiceFormula dices = new DiceFormula(this.Value.DiceCountValue.Calculate(context), this.Value.DiceType);
            int bolsteredBonus = MetamagicHelper.GetBolsteredDamageBonus(context, dices);
            int bonus = this.Value.BonusValue.Calculate(context);
            bool halfBecauseSavingThrow = false;
            bool empower = context.HasMetamagic(Metamagic.Empower);
            bool maximize = context.HasMetamagic(Metamagic.Maximize);
            DamageCriticalModifierType? crit = (ruleAttackRoll.IsCriticalConfirmed) ? ((context != null) ? DamageCriticalModifierTypeExtension.FromInt(ruleAttackRoll.WeaponStats.CriticalMultiplier) : new DamageCriticalModifierType?(ruleAttackRoll.Weapon.Blueprint.CriticalModifier)) : null;

            BaseDamage baseDamage = this.DamageTypeDesc.GetDamageDescriptor(dices, bonus + bolsteredBonus).CreateDamage();
            baseDamage.EmpowerBonus = (empower ? 1.5f : baseDamage.EmpowerBonus);
            if (maximize)
                baseDamage.CalculationType = DamageCalculationType.Maximized;
            baseDamage.CriticalModifier = ((crit != null) ? new int?(crit.GetValueOrDefault().IntValue()) : null);
            baseDamage.SourceFact = ContextDataHelper.GetFact();
            ContextAttackData contextAttackData = ContextData<ContextAttackData>.Current;
            DamageBundle damageBundle = new DamageBundle(new BaseDamage[]
            {
                baseDamage
            });
            ItemEntityWeapon weapon;
            if (contextAttackData == null)
            {
                weapon = null;
            }
            else
            {
                RuleAttackRoll attackRoll = contextAttackData.AttackRoll;
                weapon = ((attackRoll != null) ? attackRoll.Weapon : null);
            }
            damageBundle.Weapon = weapon;
            damageBundle.IsAoE = false;
            DamageBundle damage = damageBundle;
            RuleDealDamage ruleDealDamage = new RuleDealDamage(context.MaybeCaster, target, damage)
            {
                DisablePrecisionDamage = context.HasMetamagic(Metamagic.Bolstered),
                Projectile = ((contextAttackData != null) ? contextAttackData.Projectile : null),
                AttackRoll = ((contextAttackData != null) ? contextAttackData.AttackRoll : null),
                HalfBecauseSavingThrow = halfBecauseSavingThrow,
                MinHPAfterDamage = null,
                SourceAbility = context.SourceAbility,
                SourceArea = (context.AssociatedBlueprint as BlueprintAbilityAreaEffect),
                Half = false,
                AlreadyHalved = false
            };
            RuleReason ruleReason = null;
            bool flag2;
            if (contextAttackData == null)
            {
                flag2 = (null != null);
            }
            else
            {
                RuleAttackRoll attackRoll2 = contextAttackData.AttackRoll;
                flag2 = (((attackRoll2 != null) ? attackRoll2.Reason : null) != null);
            }
            if (flag2)
            {
                RuleReason proto;
                if (contextAttackData == null)
                {
                    proto = null;
                }
                else
                {
                    RuleAttackRoll attackRoll3 = contextAttackData.AttackRoll;
                    proto = ((attackRoll3 != null) ? attackRoll3.Reason : null);
                }
                ruleReason = new RuleReason(proto, null, baseDamage.SourceFact);
            }
            else if (baseDamage.SourceFact != null)
            {
                ruleReason = baseDamage.SourceFact;
            }
            if (ruleReason != null)
            {
                ruleDealDamage.Reason = ruleReason;
            }
            return ruleDealDamage;
        }

        private RuleDealDamage damageRuleQuarter(MechanicsContext context, RuleAttackRoll ruleAttackRoll, UnitEntityData target)
        {
            DiceFormula dices = new DiceFormula(this.Value.DiceCountValue.Calculate(context), this.Value.DiceType);
            int bolsteredBonus = MetamagicHelper.GetBolsteredDamageBonus(context, dices);
            int bonus = this.Value.BonusValue.Calculate(context);
            bool halfBecauseSavingThrow = true;
            bool empower = context.HasMetamagic(Metamagic.Empower);
            bool maximize = context.HasMetamagic(Metamagic.Maximize);
            DamageCriticalModifierType? crit = (ruleAttackRoll.IsCriticalConfirmed) ? ((context != null) ? DamageCriticalModifierTypeExtension.FromInt(ruleAttackRoll.WeaponStats.CriticalMultiplier) : new DamageCriticalModifierType?(ruleAttackRoll.Weapon.Blueprint.CriticalModifier)) : null;

            BaseDamage baseDamage = this.DamageTypeDesc.GetDamageDescriptor(dices, bonus + bolsteredBonus).CreateDamage();
            baseDamage.EmpowerBonus = (empower ? 1.5f : baseDamage.EmpowerBonus);
            if (maximize)
                baseDamage.CalculationType = DamageCalculationType.Maximized;
            baseDamage.CriticalModifier = ((crit != null) ? new int?(crit.GetValueOrDefault().IntValue()) : null);
            baseDamage.SourceFact = ContextDataHelper.GetFact();
            ContextAttackData contextAttackData = ContextData<ContextAttackData>.Current;
            DamageBundle damageBundle = new DamageBundle(new BaseDamage[]
            {
                baseDamage
            });
            ItemEntityWeapon weapon;
            if (contextAttackData == null)
            {
                weapon = null;
            }
            else
            {
                RuleAttackRoll attackRoll = contextAttackData.AttackRoll;
                weapon = ((attackRoll != null) ? attackRoll.Weapon : null);
            }
            damageBundle.Weapon = weapon;
            damageBundle.IsAoE = false;
            DamageBundle damage = damageBundle;
            RuleDealDamage ruleDealDamage = new RuleDealDamage(context.MaybeCaster, target, damage)
            {
                DisablePrecisionDamage = context.HasMetamagic(Metamagic.Bolstered),
                Projectile = ((contextAttackData != null) ? contextAttackData.Projectile : null),
                AttackRoll = ((contextAttackData != null) ? contextAttackData.AttackRoll : null),
                HalfBecauseSavingThrow = halfBecauseSavingThrow,
                MinHPAfterDamage = null,
                SourceAbility = context.SourceAbility,
                SourceArea = (context.AssociatedBlueprint as BlueprintAbilityAreaEffect),
                Half = true,
                AlreadyHalved = false
            };
            RuleReason ruleReason = null;
            bool flag2;
            if (contextAttackData == null)
            {
                flag2 = (null != null);
            }
            else
            {
                RuleAttackRoll attackRoll2 = contextAttackData.AttackRoll;
                flag2 = (((attackRoll2 != null) ? attackRoll2.Reason : null) != null);
            }
            if (flag2)
            {
                RuleReason proto;
                if (contextAttackData == null)
                {
                    proto = null;
                }
                else
                {
                    RuleAttackRoll attackRoll3 = contextAttackData.AttackRoll;
                    proto = ((attackRoll3 != null) ? attackRoll3.Reason : null);
                }
                ruleReason = new RuleReason(proto, null, baseDamage.SourceFact);
            }
            else if (baseDamage.SourceFact != null)
            {
                ruleReason = baseDamage.SourceFact;
            }
            if (ruleReason != null)
            {
                ruleDealDamage.Reason = ruleReason;
            }
            return ruleDealDamage;
        }

#pragma warning disable CS0649 // Field 'DisintegrateUniqueDealDamage.Value' is never assigned to, and will always have its default value null
        public ContextDiceValue Value;
#pragma warning restore CS0649 // Field 'DisintegrateUniqueDealDamage.Value' is never assigned to, and will always have its default value null

#pragma warning disable CS0649 // Field 'DisintegrateUniqueDealDamage.m_Buff' is never assigned to, and will always have its default value null
        public BlueprintBuff m_Buff;
#pragma warning restore CS0649 // Field 'DisintegrateUniqueDealDamage.m_Buff' is never assigned to, and will always have its default value null

        private DamageTypeDescription DamageTypeDesc = new DamageTypeDescription
        {
            Type = DamageType.Force,
            Common = new DamageTypeDescription.CommomData(),
            Physical = new DamageTypeDescription.PhysicalData()
        };
    }
}
