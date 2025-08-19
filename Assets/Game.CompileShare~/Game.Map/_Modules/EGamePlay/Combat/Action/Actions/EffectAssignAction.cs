using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using EGamePlay.Combat;
using ECS;

namespace EGamePlay.Combat
{
    public class EffectAssignAbility : EcsEntity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public bool Enable { get; set; }


        public bool TryMakeAction(out EffectAssignAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<EffectAssignAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// 赋给效果行动
    /// </summary>
    public class EffectAssignAction : EcsEntity, IActionExecute
    {
        /// 创建这个效果赋给行动的源能力
        public EcsEntity SourceAbility { get; set; }
        /// 目标行动
        public IActionExecute TargetAction { get; set; }
        public AbilityEffect AbilityEffect { get; set; }
        public Effect EffectConfig => AbilityEffect.EffectConfig;
        /// 行动能力
        public EcsEntity ActionAbility { get; set; }
        /// 效果赋给行动源
        public EffectAssignAction SourceAssignAction { get; set; }
        /// 行动实体
        public CombatEntity Creator { get; set; }
        /// 目标对象
        public EcsEntity Target { get; set; }
        /// 赋给目标
        public EcsEntity AssignTarget { get; set; }
        /// 触发上下文
        public TriggerContext TriggerContext { get; set; }


        ///// 前置处理
        //private void PreProcess()
        //{
        //    if (Target == null)
        //    {
        //        Target = AssignTarget;
        //        if (AssignTarget is IActionExecute actionExecute) Target = actionExecute.Target;
        //        if (AssignTarget is AbilityExecution skillExecution) Target = skillExecution.InputTarget;
        //    }
        //}

        //public void AssignEffect()
        //{
        //    PreProcess();
        //    //Log.Debug($"EffectAssignAction AssignEffect {EffectConfig.GetType().Name}");
        //    foreach (var item in AbilityEffect.Components.Values)
        //    {
        //        if (item is IEffectTriggerSystem effectTriggerSystem)
        //        {
        //            effectTriggerSystem.OnTriggerApplyEffect(this);
        //        }
        //    }

        //    PostProcess();

        //    FinishAction();
        //}

        ///// 后置处理
        //private void PostProcess()
        //{
        //    Creator.TriggerActionPoint(ActionPointType.ExecuteAssignEffect, this);
        //    if (!Target.IsDisposed)
        //    {
        //        Target.GetComponent<BehaviourPointComponent>().TriggerActionPoint(ActionPointType.SufferAssignEffect, this);
        //    }

        //    var decorators = AbilityEffect.EffectConfig.Decorators;
        //    if (decorators != null)
        //    {
        //        foreach (var item in decorators)
        //        {
        //            if (item is TriggerNewEffectWhenAssignEffectDecorator effectDecorator)
        //            {
        //                var abilityTriggerComp = AbilityEffect.OwnerAbility.GetComponent<AbilityTriggerComponent>();
        //                var effects = abilityTriggerComp.AbilityTriggers;
        //                var ExecuteTriggerType = effectDecorator.ExecuteTriggerType;
        //                for (int i = 0; i < effects.Count; i++)
        //                {
        //                    if (i == (int)ExecuteTriggerType - 1 || ExecuteTriggerType == ExecuteTriggerType.AllTriggers)
        //                    {
        //                        var effect = effects[i];
        //                        effect.OnTrigger(new TriggerContext() { Target = Target });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public void FinishAction()
        //{
        //    Entity.Destroy(this);
        //}
    }
}