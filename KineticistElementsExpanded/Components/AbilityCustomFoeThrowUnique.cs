using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodexLib;
using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.FxSpawnSystem;
using Newtonsoft.Json.Linq;
using Owlcat.Runtime.Core.Utils;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace KineticistElementsExpanded.Components
{
    class AbilityCustomFoeThrowUnique : AbilityDeliverEffect, IAbilityTargetRestriction
    {

        public Feet radius = new Feet { m_Value = 60 };
        public TargetType TargetType = TargetType.Enemy;

        private static HashSet<UnitEntityData> Targets = new HashSet<UnitEntityData>();
        private static Dictionary<UnitEntityData, (RuleAttackRoll, bool)> target_details = new Dictionary<UnitEntityData, (RuleAttackRoll, bool)>();

        private BlueprintProjectile Projectile
        {
            get
            {
                return this.m_Projectile;
            }
        }

        internal static bool IsKineticArchetypesEnabled()
        {
            return UnityModManager.modEntries.Where(
                mod => mod.Info.Id.Equals("KineticArchetypes") && mod.Enabled && !mod.ErrorOnLoading)
              .Any();
        }

        private static int DetermineTargetCount(UnitEntityData caster)
        {
            if (!IsKineticArchetypesEnabled())
                return 1;

            UnitPart part = null;
            foreach (UnitPart x in caster.Parts.Parts)
            {
                var partType = x.GetType();
                if (partType.Name == "OnslaughtBlastPart")
                    part = x;
            }
            if (part is null)
                return 1;
            var part_method = part.GetType().GetMethod("CalculateNumberOfBlasts");
            return (int)part_method.Invoke(part, null);
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
                RelaxPoints = true,
                CameraShouldFollow = false,
            };
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            Targets.Clear();
            target_details.Clear();
            UnitEntityData caster = context.MaybeCaster;
            int num_targets = DetermineTargetCount(caster);

            while (0 < num_targets--)
            {
                UnitEntityData lifted = getTarget(Targets, context, target, caster, caster, target.Unit);
                if (lifted != null) Targets.Add(lifted);
            }

            List<IEnumerator> moveRoutines = new List<IEnumerator>();

            foreach (UnitEntityData lifted in Targets)
            {

                DimensionDoorSettings ddSettings = this.CreateSettings(lifted);

                RuleSavingThrow savingThrow = fortSaveRule(context, lifted);
                context.TriggerRule(savingThrow);
                if (savingThrow.IsPassed)
                {
                    continue;
                }

                var weapon = AnyRef.ToRef<BlueprintItemWeaponReference>("65951e1195848844b8ab8f46d942f6e8").Get().CreateEntity<ItemEntityWeapon>(); // KineticBlastPhysicalWeapon
                RuleAttackRoll attackRoll = new RuleAttackRoll(caster, target.Unit, weapon, 0) { SuspendCombatLog = false };
                context.TriggerRule(attackRoll);
                if (context.ForceAlwaysHit) attackRoll.SetFake(AttackResult.Hit);
                attackRoll?.ConsumeMirrorImageIfNecessary();

                // Comments and credits to Microsoftenator
                // Missed attacks are send randomly within 30ft (-Radius/+Radius)
                var targetPoint = attackRoll.IsHit ? target.Point : GeometryUtils.ProjectToGround(target.Point + new Vector3(
                                UnityEngine.Random.Range(0, radius.Value) - radius.Value / 2,
                                UnityEngine.Random.Range(0, radius.Value) - radius.Value / 2,
                                UnityEngine.Random.Range(0, radius.Value) - radius.Value / 2));
                var direction = targetPoint - lifted.Position;
                var distance = direction.magnitude;

                // "World" units appear to be meters
                var vector = direction.normalized * distance.Feet().Meters;

                var expectedDestination = lifted.Position + vector;
                var obstaclePosition = ObstacleAnalyzer.TraceAlongNavmesh(lifted.Position, expectedDestination);

                var distance2d = GeometryUtils.Distance2D(lifted.Position, expectedDestination);
                var obstacleDistance2d = GeometryUtils.Distance2D(lifted.Position, obstaclePosition);

                // Note: Feet.Value is a float -> integer cast
                var distance2dFeet = distance2d.MetersToFeet().Value;
                var obstacleDistance2dFeet = obstacleDistance2d.MetersToFeet().Value;

                target_details.Add(lifted, (attackRoll, distance2d > obstacleDistance2d));

                IFxHandle portalFrom = FxHelper.SpawnFxOnUnit(ddSettings.PortalFromPrefab, lifted.View);
                IFxHandle portalTo = FxHelper.SpawnFxOnUnit(ddSettings.PortalToPrefab, lifted.View);
                while (!(portalTo?.IsSpawned ?? true) || !(portalFrom?.IsSpawned ?? true))
                {
                    yield return null;
                }

                if (portalTo != null)
                {
                    portalTo.SpawnedObject.transform.position = targetPoint;
                }

                Vector3 value = ObjectExtensions.Or((portalFrom == null) ? null : ObjectExtensions.Or(portalFrom.SpawnedObject, null)?.transform.FindChildRecursive(ddSettings.PortalBone), null)?.transform.position ?? lifted.Position;
                lifted.Wake(10f);
                Vector3? intermediateFromPosition = ((portalFrom?.SpawnedObject != null) ? new Vector3?(value) : null);
                IEnumerator moveRoutine = CreateMoveRoutine(ddSettings, lifted, target, intermediateFromPosition, direction, distance);
                moveRoutines.Add(moveRoutine);
            }

            while (moveRoutines.Count > 0)
            {
                for (int i = 0; i < moveRoutines.Count; i++)
                {
                    if (!moveRoutines[i].MoveNext())
                    {
                        moveRoutines.RemoveAt(i);
                        i--;
                    }
                }
                yield return null;
            }
            
            foreach (UnitEntityData lifted in Targets)
            {
                (RuleAttackRoll attackRoll, bool hitTarget) = target_details.Get(lifted);
                if (attackRoll == null)
                    continue;

                // If attack hit, and thrown unit reached target
                if (attackRoll.IsHit && !hitTarget)
                {
                    // Prone lifted/Knock off mount
                    if (!lifted.Descriptor.State.Prone.Active)
                    {
                        lifted.State.Prone.ShouldBeActive = true;
                        if (lifted.CanBeKnockedOff())
                        {
                            EventBus.RaiseEvent<IKnockOffHandler>(h => h.HandleKnockOff(caster, lifted), true);
                        }
                    }
                    yield return new AbilityDeliveryTarget(target);
                }
                yield return new AbilityDeliveryTarget(lifted);
            }
        }

        private static IEnumerator CreateMoveRoutine(DimensionDoorSettings settings, UnitEntityData unit, TargetWrapper target, Vector3? intermediateFromPosition, Vector3 direction, float distance)
        {
            GameObject prefab = settings.CasterDisappearFx;
            GameObject appearFx = settings.CasterAppearFx;
            BlueprintProjectile disappearProjectile = settings.CasterDisappearProjectile;
            BlueprintProjectile appearProjectile = settings.CasterAppearProjectile;
            BlueprintProjectile teleportationProjectile = settings.CasterTeleportationProjectile;
            float appearDuration = settings.CasterAppearDuration;
            float disappearDuration = settings.CasterDisappearDuration;
            appearDuration = Math.Max(appearDuration, 0.3f);
            unit.View.StopMoving();

            FxHelper.SpawnFxOnUnit(prefab, unit.View);
            if (disappearDuration > 0.01f)
            {
                if (TacticalCombatHelper.IsActive)
                {
                    yield return new WaitForSeconds(disappearDuration);
                }
                else
                {
                    TimeSpan startTime2 = Game.Instance.TimeController.GameTime;
                    while (Game.Instance.TimeController.GameTime - startTime2 < disappearDuration.Seconds())
                    {
                        yield return null;
                    }
                }
            }

            if (teleportationProjectile != null && intermediateFromPosition.HasValue)
            {
                IEnumerator projectileRoutine = CreateProjectileRoutine(teleportationProjectile, unit, unit.Position, intermediateFromPosition.Value);
                while (projectileRoutine.MoveNext())
                {
                    yield return null;
                }
            }

            unit.Ensure<UnitPartForceMove>().Push(direction, distance, false);
            if (settings.LookAtTarget)
            {
                unit.ForceLookAt(target.Point);
            }

            if (settings.CameraShouldFollow)
            {
                Game.Instance.UI.GetCameraRig().ScrollTo(target.Point);
            }

            FxHelper.SpawnFxOnUnit(appearFx, unit.View);
            if (TacticalCombatHelper.IsActive)
            {
                yield return new WaitForSeconds(appearDuration);
            }
            else
            {
                TimeSpan startTime2 = Game.Instance.TimeController.GameTime;
                while (Game.Instance.TimeController.GameTime - startTime2 < appearDuration.Seconds())
                {
                    yield return null;
                }
            }
        }

        private static IEnumerator CreateProjectileRoutine(BlueprintProjectile blueprint, UnitEntityData unit, Vector3? sourcePosition, Vector3 targetPosition)
        {
            bool projectileLanded = false;
            if (sourcePosition.HasValue)
            {
                Game.Instance.ProjectileController.Launch(unit, targetPosition, blueprint, sourcePosition.Value, delegate
                {
                    projectileLanded = true;
                });
            }
            else
            {
                Game.Instance.ProjectileController.Launch(unit, targetPosition, blueprint, delegate
                {
                    projectileLanded = true;
                });
            }

            while (!projectileLanded)
            {
                yield return null;
            }
        }

        private RuleSavingThrow fortSaveRule(AbilityExecutionContext context, UnitEntityData unit)
        {
            RuleSavingThrow ruleSavingThrow = new RuleSavingThrow(unit, SavingThrowType.Fortitude, context.Params.DC);
            ruleSavingThrow.Reason = context;
            ruleSavingThrow.Buff =  null;
            ruleSavingThrow.PersistentSpell = false;
            return ruleSavingThrow;
        }
        #region damage rules
        /*
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
            baseDamage.EmpowerBonus = new ValueWithSource<float>(empower ? 1.5f : baseDamage.EmpowerBonus);
            //if (maximize)
            //    baseDamage.CalculationType = DamageCalculationType.Maximized;
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
            baseDamage.EmpowerBonus = new ValueWithSource<float>(empower ? 1.5f : baseDamage.EmpowerBonus);
            //if (maximize)
            //    baseDamage.CalculationType = DamageCalculationType.Maximized;
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
        */
        #endregion
        public bool IsTargetRestrictionPassed(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData buffed = getTarget(Targets, null, target, caster);
            
            return ObstacleAnalyzer.IsPointInsideNavMesh(target.Point) && !FogOfWarController.IsInFogOfWar(target.Point) && buffed != null && target.Unit != null;
        }

        private UnitEntityData getTarget(HashSet<UnitEntityData> usedTargets, AbilityExecutionContext context = null, TargetWrapper target = null, UnitEntityData caster = null, params UnitEntityData[] ignoredUnits)
        {
            foreach (UnitEntityData unitEntityData in Game.Instance.State.Units)
            {
                var buff_list = unitEntityData.Buffs.Enumerable.ToArray();
                foreach (var buff in buff_list)
                {
                    if (buff.Blueprint.name == this.m_Buff.name 
                        && !usedTargets.Contains(unitEntityData)
                        && !ignoredUnits.Contains(unitEntityData)
                        && unitEntityData.IsUnitInRange(target.Point, this.radius.Meters, true))
                    {
                        if (CheckTarget(unitEntityData, caster)) return unitEntityData;
                    }
                }
            }
            foreach (UnitEntityData unitEntityData in Game.Instance.State.Units)
            {
                if (!usedTargets.Contains(unitEntityData)
                        && !ignoredUnits.Contains(unitEntityData)
                        && unitEntityData.IsUnitInRange(target.Point, this.radius.Meters, true))
                {
                    if (CheckTarget(unitEntityData, caster)) return unitEntityData;
                }
            }
            return null;
        }

        private bool CheckTarget(UnitEntityData target, UnitEntityData caster)
        {
            if (target.HPLeft <= 0) return false;
            switch (TargetType)
            {
                case TargetType.Enemy: return target.IsEnemy(caster);
                case TargetType.Ally: return target.IsAlly(caster);
                case TargetType.Any: return true;
                default: return false;
            }
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

        //public ContextDiceValue Value;

        private DamageTypeDescription DamageTypeDesc = new DamageTypeDescription
        {
            Type = DamageType.Physical,
            Common = new DamageTypeDescription.CommomData(),
            Physical = new DamageTypeDescription.PhysicalData() { Form = (PhysicalDamageForm)7 }
        };
    }
}
