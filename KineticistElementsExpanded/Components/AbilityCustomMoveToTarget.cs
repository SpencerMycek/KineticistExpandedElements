using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.View;

namespace KineticistElementsExpanded.Components
{
	public class AbilityCustomMoveToTarget : AbilityCustomLogic, IAbilityTargetRestriction
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
			UnitEntityData caster = context.Caster;
			IEnumerator<AbilityDeliveryTarget> casterDelivery = AbilityCustomDimensionDoor.Deliver(this.CreateSettings(caster), caster, target.Point);
			while (casterDelivery.MoveNext())
			{
				yield return null;
			}
			yield return new AbilityDeliveryTarget(target);
			yield break;
		}

		public override void Cleanup(AbilityExecutionContext context)
		{
		}

		public bool IsTargetRestrictionPassed(UnitEntityData caster, TargetWrapper target)
		{
			return ObstacleAnalyzer.IsPointInsideNavMesh(target.Point) && !FogOfWarController.IsInFogOfWar(target.Point);
		}

		public string GetAbilityTargetRestrictionUIText(UnitEntityData caster, TargetWrapper target)
		{
			return LocalizedTexts.Instance.Reasons.TargetIsInvalid;
		}

		public BlueprintProjectileReference m_Projectile;

		public PrefabLink DisappearFx;

		public float DisappearDuration;

		public PrefabLink AppearFx;

		public float AppearDuration;
	}
}