using CodexLib;
using Kingmaker;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KineticistElementsExpanded.Components
{
    public class ContextActionRemoveBuffAll : ContextAction
    {
        public float radius;
        public BlueprintBuff m_Buff;

        public override string GetCaption()
        {
            return "Removing Buff from All";
        }

        public override void RunAction()
        {
            try
            {
                foreach (UnitEntityData unitEntityData in Game.Instance.State.Units)
                {
                    var buff_list = unitEntityData.Buffs.Enumerable.ToArray();
                    foreach (var buff in buff_list)
                    {
                        if (buff.Blueprint.name == this.m_Buff.name)
                        {
                            buff.Remove();
                        }
                    }
                }
            } catch (Exception ex)
            {
                Helper.PrintNotification($"Exception: {ex.Message}");
            }
        }
    }
}
