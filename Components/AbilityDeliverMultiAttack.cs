using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KineticistElementsExpanded.Components
{
    public class AbilityDeliverMultiAttack : AbilityDeliverEffect, IAbilityAoERadiusProvider
    {
        public bool TargetDead;
        public float DelayBetweenChain;
        public Feet radius;
        public ContextValue TargetsCount;
        public TargetType TargetType;
        public ConditionsChecker Condition;
        public BlueprintItemWeaponReference Weapon;
        public BlueprintProjectileReference[] Projectiles;
        public bool NeedAttackRoll => Weapon != null;
        private static System.Random rnd = new System.Random();

        public Feet AoERadius 
        {
            get
            {
                return this.radius;
            }
        
        }

        public TargetType Targets
        {
            get
            {
                return this.TargetType;
            }

        }

        public BlueprintProjectile Projectile
        {
            get
            {
                return Projectiles[rnd.Next(1, Projectiles.Length)].Get();
            }
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            var launcher = context.MaybeCaster;
            var Targets = new HashSet<UnitEntityData>();
            int targetsCount = this.TargetsCount.Calculate(context);

            if (launcher == null)
                yield break;

            int targetIndex = 0;

            while (targetIndex < targetsCount)
            {
                var temp = SelectNextTarget(context, target, Targets, this.AoERadius.Meters);
                if (temp == null) break;
                Targets.Add(temp);
                targetIndex++;
            }

            var processes = new IEnumerator<AbilityDeliveryTarget>[targetsCount];
            foreach (var currentTarget in Targets)
            {
                processes = (IEnumerator<AbilityDeliveryTarget>[])processes.Append(DeliverInternal(context, launcher, currentTarget));
            }

            for (;;)
            {
                if (!processes.HasItem((IEnumerator<AbilityDeliveryTarget> i) => i != null))
                {
                    break;
                }
                int num;
                for (int j = 0; j < processes.Length; j = num)
                {
                    IEnumerator<AbilityDeliveryTarget> p = processes[j];
                    if (p != null)
                    {
                        bool flag;
                        while ((flag = p.MoveNext()) && p.Current != null)
                        {
                            yield return p.Current;
                        }
                        if (!flag)
                        {
                            processes[j] = null;
                            //yield return p.Current;
                        }
                        p = null;
                    }
                    num = j + 1;
                }
                yield return null;
            }
            
            yield break;
        }

        private IEnumerator<AbilityDeliveryTarget> DeliverInternal(AbilityExecutionContext context, UnitEntityData launcher, UnitEntityData target)
        {
                Projectile proj = Game.Instance.ProjectileController.Launch(launcher, target, this.Projectile);
                proj.IsFirstProjectile = true;
                RuleAttackRoll attackRoll = null;

                if (this.NeedAttackRoll) // decide whenever the attack hit or not
                {
                    var weapon = this.Weapon.Get().CreateEntity<ItemEntityWeapon>();
                    attackRoll = new RuleAttackRoll(context.MaybeCaster, target, weapon, 0) { SuspendCombatLog = true };
                    context.TriggerRule(attackRoll);
                    if (context.ForceAlwaysHit)
                        attackRoll.SetFake(AttackResult.Hit);

                    proj.AttackRoll = attackRoll;
                    proj.MissTarget = context.MissTarget;
                }

                while (!proj.IsHit) // wait until projectile hit
                {
                    if (proj.Cleared) // stop if projectile controller cleared projectiles
                        yield break;
                    yield return null;
                }

                attackRoll?.ConsumeMirrorImageIfNecessary();

            yield return new AbilityDeliveryTarget(proj.Target)
            {
                AttackRoll = proj.AttackRoll,
                Projectile = proj
            };
            yield break;
        }

        private UnitEntityData SelectNextTarget(AbilityExecutionContext context, TargetWrapper center, HashSet<UnitEntityData> usedTargets, float radius)
        {
            var point = center.Point;
            float min = float.MaxValue;
            UnitEntityData result = null;
            foreach (UnitEntityData unitEntityData in Game.Instance.State.Units)
            {
                float distance = (unitEntityData.Position - point).magnitude;
                if (this.CheckTarget(context, unitEntityData) && distance <= radius && !usedTargets.Contains(unitEntityData) && distance < min)
                {
                    min = distance;
                    result = unitEntityData;
                }
            }
            return result;
        }

        private bool CheckTarget(AbilityExecutionContext context, UnitEntityData unit)
        {
            if (unit.Descriptor.State.IsDead && !this.TargetDead)
                return false;

            if ((this.TargetType == TargetType.Enemy && !context.MaybeCaster.IsEnemy(unit)) || (this.TargetType == TargetType.Ally && context.MaybeCaster.IsEnemy(unit)))
                return false;

            if (this.TargetType == TargetType.Any && context.HasMetamagic(Metamagic.Selective) && !this.Condition.HasIsAllyCondition() && !context.MaybeCaster.IsEnemy(unit))
                return false;

            if (unit.State.Features.Blinking && !context.MaybeOwner.Descriptor.IsSeeInvisibility)
            {
                var ruleRollD = new RuleRollD100(context.MaybeOwner);
                Rulebook.Trigger(ruleRollD);
                if (ruleRollD < 50)
                    return false;
            }

            if (this.Condition?.HasConditions == true)
            {
                using (context.GetDataScope(unit))
                {
                    return this.Condition.Check();
                }
            }

            return true;
        }

        public bool WouldTargetUnit(AbilityData ability, Vector3 targetPos, UnitEntityData unit)
        {
            return unit.IsUnitInRange(targetPos, this.AoERadius.Meters, true);
        }
    }
}
