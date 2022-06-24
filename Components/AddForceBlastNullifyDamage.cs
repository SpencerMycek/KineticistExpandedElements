using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
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
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KineticistElementsExpanded.Components
{
    class AddForceBlastNullifyDamage : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, IInitiatorRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDrainEnergy>
    {

        public AddForceBlastNullifyDamage(params BlueprintAbilityReference[] list)
        {
            this.m_AbilityList = list;
            AddForceBlastNullifyDamage.alreadyTriggered = false;
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

                if (!AddForceBlastNullifyDamage.alreadyTriggered) {
                    Helper.Print("[AddForceBlastNullifyDamage] Nullify Damage");
                    Predicate<BaseDamage> RemoveAll = delegate (BaseDamage damage) { return true; };
                    ruleDealDamage.Remove(RemoveAll);

                    AddForceBlastNullifyDamage.alreadyTriggered = true;
                } else
                {
                    AddForceBlastNullifyDamage.alreadyTriggered = false;
                }

            } catch (Exception ex)
            {
                Helper.Print($"[AddForceBlastNullifyDamage] Exception: {ex.Message}");
            }
        }

        public void ApplyDidTrigger(RulebookTargetEvent evt, [NotNull] MechanicsContext context)
        {
            try
            {
                RuleDealDamage ruleDealDamage = evt as RuleDealDamage;

                AbilityExecutionContext abilityExecutionContext;
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
                        Helper.Print("Ability is missing");
                        return;
                    }
                    if (!this.AbilityList.Contains(blueprintAbility2) && !this.AbilityList.Contains(blueprintAbility2.Parent))
                    {
                        return;
                    }
                }

                this.RunAction(evt.Target, context);

            }
            catch (Exception ex)
            {
                Helper.Print($"[AddForceBlastNullifyDamage] Exception: {ex.Message}");
            }
        }

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

        private BlueprintAbilityReference[] m_AbilityList;

        public ActionList Actions;

        private TimeSpan m_LastFrameTime;

        private readonly HashSet<UnitEntityData> m_AffectedThisFrame = new HashSet<UnitEntityData>();

        private static bool alreadyTriggered;
    }
}
