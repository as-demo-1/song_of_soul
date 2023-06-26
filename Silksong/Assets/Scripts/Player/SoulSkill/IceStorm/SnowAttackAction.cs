using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class SnowAttackAction : Action
{
   public Animator animator;
   public GameObject atkEffect;
   public SharedTransform target;
   public override void OnAwake()
   {
      animator = transform.GetComponentInChildren<Animator>();
      base.OnAwake();
   }

   public override void OnStart()
   {
      if (target.Value!=null)
      {
         Debug.Log("旋转");
         atkEffect.transform.right = target.Value.position - atkEffect.transform.position;
      }
      atkEffect.SetActive(true);
      DOVirtual.DelayedCall( 2.0f, ()=>atkEffect.SetActive(false));
      base.OnStart();
   }
}
