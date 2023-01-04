using System;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BatterGame
{
    public enum PerformanceType
    {
        idle,
        Run,
        NormalAtk
    }

    public abstract class StateController
    {
        protected Animator _animator;
        public StateController(Animator animator)
        {
            _animator = animator;
        }
        public abstract void DirveAnimatorParameters();
    }

    public class PlayerStateController : StateController
    {
        public PlayerStateController(Animator animator) : base(animator)
        {
            
        }

        public override void DirveAnimatorParameters()
        {
            if (PlayerInput.Instance.normalAttack.Down) _animator.SetTrigger("atk");
            _animator.SetBool("haveHorizontalSignal", Math.Abs(PlayerInput.Instance.horizontal.Value) > 0.001f);
        }
    }

    
    
    
    // Performance内只写和效果有关的逻辑
    public abstract class Performance : StateMachineBehaviour
    {
        public PerformanceType m_performanceType;

        public Performance(PerformanceType performanceType)
        {
            m_performanceType = performanceType;
        }
        public abstract void Play();
    }

    public class IdlePerformance : Performance
    {
        public IdlePerformance(PerformanceType performanceType) : base(performanceType)
        {
            
        }
        
        public override void Play()
        {
            
        }
    }

    public class RunPerformance : Performance
    {
        public RunPerformance(PerformanceType performanceType) : base(performanceType)
        {
            
        }
        public override void Play()
        {
            
        }
    }

    public class NormalAtk : Performance
    {
        private AudioSource _atkAudio;
        public NormalAtk(PerformanceType performanceType) : base(performanceType)
        {
            //TODO : 初始化音频资源
        }
        public override void Play()
        {
            if (_atkAudio != null)
            {
                _atkAudio.Play();
            }
        }
    }
    
    public class PerformanceFector : Singleton<PerformanceFector>
    {
        public Performance Create(PerformanceType performanceType)
        {
            switch (performanceType)
            {
                case PerformanceType.idle:
                    return new IdlePerformance(PerformanceType.idle);
                case PerformanceType.Run:
                    return new RunPerformance(PerformanceType.Run);
                case PerformanceType.NormalAtk:
                    return new NormalAtk(PerformanceType.NormalAtk);
            }

            return new IdlePerformance(PerformanceType.idle);
        }
    }
}