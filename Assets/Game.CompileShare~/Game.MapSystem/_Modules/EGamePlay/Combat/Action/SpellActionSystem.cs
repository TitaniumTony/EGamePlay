using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtils;
using ECS;
using EGamePlay.Combat;
using System;
using ET;
using System.ComponentModel;
using ECSGame;

namespace EGamePlay
{
    public class SpellActionSystem : AEntitySystem<SpellAction>,
        IAwake<SpellAction>
    {
        public void Awake(SpellAction entity)
        {

        }

        public static void FinishAction(SpellAction entity)
        {
            EcsEntity.Destroy(entity);
        }

        //前置处理
        private static void ActionProcess(SpellAction entity)
        {
            BehaviourPointSystem.TriggerActionPoint(entity.Creator, ActionPointType.PreExecuteSpell, entity);
        }

        public static void Execute(SpellAction entity, bool actionOccupy = true)
        {
            ActionProcess(entity);

            var execution = entity.SkillAbility.OwnerEntity.AddChild<AbilityExecution>(x => x.AbilityEntity = entity.SkillAbility);
            entity.SkillExecution = execution;
#if EGAMEPLAY_ET && DEBUG
            SkillAbility.LoadExecution();
#endif
            execution.ExecutionObject = entity.SkillAbility.ExecutionObject;
            AbilityExecutionSystem.LoadExecutionEffects(execution);
            execution.Position = entity.SkillAbility.OwnerEntity.Position + entity.SkillAbility.ExecutionObject.Offset;
            execution.Rotation = entity.InputDirection.GetRotation();
#if EGAMEPLAY_ET
            execution.CreateItemUnit();
#endif

            //entity.SkillAbility.FireEvent("CreateExecution", execution);
            execution.Name = entity.SkillAbility.Name;
            if (entity.SkillTargets.Count > 0)
            {
                entity.SkillExecution.SkillTargets.AddRange(entity.SkillTargets);
            }
            if (entity.SkillAbility.Config.Id != 2001)
            {
                entity.SkillExecution.ActionOccupy = actionOccupy;
            }
            execution.InputTarget = entity.InputTarget;
            execution.InputPoint = entity.InputPoint;
            execution.InputDirection = entity.InputDirection;
            execution.InputRadian = entity.InputRadian;
            AbilityExecutionSystem.BeginExecute(execution);
            //entity.AddComponent<UpdateComponent>();
            if (entity.SkillAbility.Config.Id == 2001)
            {
                execution.GetParent<CombatEntity>().SpellingExecution = null;
            }
        }

        //后置处理
        private static void AfterActionProcess(SpellAction entity)
        {
            BehaviourPointSystem.TriggerActionPoint(entity.Creator, ActionPointType.PostExecuteSpell, entity);
        }

        public static void Update(SpellAction entity)
        {
            if (entity.SkillExecution != null)
            {
                if (entity.SkillExecution.IsDisposed)
                {
                    AfterActionProcess(entity);
                    FinishAction(entity);
                }
            }
        }
    }
}