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
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KineticistElementsExpanded.Components
{
    class AbilityDeliverProjectileClustered : AbilityDeliverEffect
    {
        public ContextValue m_ProjectileCount;
        public BlueprintProjectileReference m_Projectile;
        private static System.Random rnd = new System.Random();

        public ContextValue ProjectileCount
        {
            get
            {
                return this.m_ProjectileCount;
            }
        }

        public BlueprintProjectile Projectile
        {
            get
            {
                return this.m_Projectile.Get();
            }
        }

        public float random_float
        {
            get
            {
                return rnd.Next(1, 359);
            }
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            var launcher = context.MaybeCaster;
            int targetsCount = ProjectileCount.Calculate(context);
            UnitEntityData Target = target.Unit;

            if (launcher == null || Target == null)
            {
                Main.PrintError("Launcher or Target Missing");
                yield break;
            }

            var processes = new IEnumerator<AbilityDeliveryTarget>[targetsCount];
            for (var i = 0; i<targetsCount; i++)
            {
                Vector3 normalized = (target.Point - context.Caster.Position).normalized;
                Vector3 a = Quaternion.Euler(0f, random_float, 0f) * normalized;
                Vector3 startPosition = GeometryUtils.ProjectToGround(target.Point + a * rnd.Next(5,10));
                var temp = new IEnumerator<AbilityDeliveryTarget>[] { DeliverInternal(context, launcher, Target, startPosition) };
                processes = processes.Concat(temp).ToArray();
                var startTime = Game.Instance.TimeController.GameTime;
                while (Game.Instance.TimeController.GameTime - startTime < (0.1).Seconds())
                    yield return null;
            }
            Main.Print("Deliver Processes Created");
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
                        while ((flag = ((IEnumerator<AbilityDeliveryTarget>)p).MoveNext()) && p.Current != null)
                        {
                            yield return p.Current;
                        }
                        if (!flag)
                        {
                            processes[j] = null;
                        }
                        p = null;
                    }
                    num = j + 1;
                }
                yield return null;
            }
            Main.Print("End of Deliver");
            yield break;
        }

        private IEnumerator<AbilityDeliveryTarget> DeliverInternal(AbilityExecutionContext context, UnitEntityData launcher, TargetWrapper target, Vector3 startPosition)
        {
            Projectile proj = Game.Instance.ProjectileController.Launch(launcher, target, this.Projectile, startPosition, null);
            proj.IsFirstProjectile = true;

            while (!proj.IsHit)
            {
                yield return null;
                if (proj.Cleared)
                    yield break;
                EntityPoolEnumerator<UnitEntityData> entityPoolEnumerator = default(EntityPoolEnumerator<UnitEntityData>);
            }

            yield return new AbilityDeliveryTarget(proj.Target)
            {
                Projectile = proj
            };
            yield break;
        }
    }
}
