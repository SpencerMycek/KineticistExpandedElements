using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KineticistElementsExpanded.Components
{
    class AbilityUniqueSingularityRunAction : AbilityAreaEffectRunAction
    {
        public override void OnRound(MechanicsContext context, AreaEffectEntityData areaEffect)
        {
            if (this.damage.HasActions && num_round == 0)
            {
                using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
                {
                    try
                    {
                        foreach (UnitEntityData unit in areaEffect.InGameUnitsInside)
                        {
                            using (context.GetDataScope(unit))
                            {
                                this.damage.Run();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Helper.Print($"Damage: Exception: {ex.Message}");
                    }
                }
            }

            if (this.spawn.HasActions && num_round == 1)
            {
                try
                {
                    using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
                    {
                        using (context.GetDataScope(areaEffect.m_Target))
                        {
                            this.spawn.Run();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helper.Print($"Spawn: Exception: {ex.Message}");
                }
            }
            num_round = (num_round == 1) ? 0 : 1;
        }

        private int num_round = 0;
        public ActionList spawn;
        public ActionList damage;
    }
}
