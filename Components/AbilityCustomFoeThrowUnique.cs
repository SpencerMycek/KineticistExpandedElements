using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Utility;
using Kingmaker.View;

namespace KineticistElementsExpanded.Components
{
    class AbilityCustomFoeThrowUnique : AbilityDeliverEffect, IAbilityTargetRestriction
    {

        private BlueprintProjectile Projectile
        {
            get
            {
                return this.m_Projectile;
            }
        }

        private DimensionDoorSettings CreateSettings(UnitEntityData unit)
        {
            return new DimensionDoorSettings
            {
                CasterDisappearFx = this.DisappearFx.Load(false, false),
                CasterDisappearDuration = this.DisappearDuration,
                CasterAppearFx = this.AppearFx.Load(false, false),
                CasterAppearDuration = this.AppearDuration,
                CasterTeleportationProjectile = this.Projectile,
                Targets = new List<UnitEntityData>
                {
                    unit
                },
                LookAtTarget = false,
                RelaxPoints = false
            };
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            UnitEntityData caster = context.MaybeCaster;
            UnitEntityData lifted = getBuffedTarget();

            RuleSavingThrow savingThrow = fortSaveRule(context, lifted);
            context.TriggerRule(savingThrow);
            if (savingThrow.IsPassed)
            {
                yield break;
            }

            IEnumerator<AbilityDeliveryTarget> delivery = AbilityCustomDimensionDoor.Deliver(this.CreateSettings(lifted), caster, target.Point);
            while (delivery.MoveNext())
            {
                    yield return null;
            }

            var weapon = "65951e1195848844b8ab8f46d942f6e8".ToRef<BlueprintItemWeaponReference>().Get().CreateEntity<ItemEntityWeapon>();
            RuleAttackRoll attackRoll = new RuleAttackRoll(caster, target.Unit, weapon, 0) { SuspendCombatLog = false };
            context.TriggerRule(attackRoll);
            if (context.ForceAlwaysHit) attackRoll.SetFake(AttackResult.Hit);
            attackRoll?.ConsumeMirrorImageIfNecessary();

            if (attackRoll.IsHit)
            {
                context.TriggerRule(damageRule(context, attackRoll, target.Unit));
                context.TriggerRule(damageRule(context, attackRoll, lifted));
                RuleCombatManeuver trip = new RuleCombatManeuver(caster, lifted, CombatManeuver.Trip)
                {
                    OverrideRoll = new RuleRollD20(caster)
                    {
                        ResultOverride = 20
                    },
                    DisableBattleLog = true
                };
                context.TriggerRule(trip);
            } else
            {
                context.TriggerRule(damageRuleHalf(context, attackRoll, lifted));
            }
            yield break;
        }

        private RuleSavingThrow fortSaveRule(AbilityExecutionContext context, UnitEntityData unit)
        {
            RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(unit, SavingThrowType.Fortitude, context.Params.DC);
            ruleSavingThrow.Reason = context;
            ruleSavingThrow.Buff =  null;
            ruleSavingThrow.PersistentSpell = false;
            return ruleSavingThrow;
        }

        private RuleDealDamage damageRule(AbilityExecutionContext context, RuleAttackRoll ruleAttackRoll, UnitEntityData target)
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
        
        private RuleDealDamage damageRuleHalf(AbilityExecutionContext context, RuleAttackRoll ruleAttackRoll, UnitEntityData target)
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

        public bool IsTargetRestrictionPassed(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData buffed = getBuffedTarget();
            
            return ObstacleAnalyzer.IsPointInsideNavMesh(target.Point) && !FogOfWarController.IsInFogOfWar(target.Point) && buffed != null && target.Unit != buffed;
        }

        private UnitEntityData getBuffedTarget()
        {
            foreach (UnitEntityData unitEntityData in Game.Instance.State.Units)
            {
                var buff_list = unitEntityData.Buffs.Enumerable.ToArray();
                foreach (var buff in buff_list)
                {
                    if (buff.Blueprint.name == this.m_Buff.name)
                    {
                        return unitEntityData;
                    }
                }
            }
            return null;
        }

        public string GetAbilityTargetRestrictionUIText(UnitEntityData caster, TargetWrapper target)
        {
            return LocalizedTexts.Instance.Reasons.TargetIsInvalid + " Or there is no lifted target.";
        }

        public BlueprintProjectileReference m_Projectile;

        public PrefabLink DisappearFx;

        public float DisappearDuration;

        public PrefabLink AppearFx;

        public float AppearDuration;

        public BlueprintBuff m_Buff;

        public ContextDiceValue Value;

        private DamageTypeDescription DamageTypeDesc = new DamageTypeDescription
        {
            Type = DamageType.Force,
            Common = new DamageTypeDescription.CommomData(),
            Physical = new DamageTypeDescription.PhysicalData() { Form = (PhysicalDamageForm)7 }
        };
    }
}
