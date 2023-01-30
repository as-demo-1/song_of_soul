// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// /// <summary>
// /// 输入管理器，用于检测按键输入并向事件中心发送对应事件
// /// </summary>
// public class InputManager : Singleton<InputManager> {
//     List<KeyCode> upKeyTester;
//     public InputManager () {
//         upKeyTester = new List<KeyCode> {
//             KeyCode.W,
//             KeyCode.A,
//             KeyCode.D,
//             KeyCode.Q,
//             KeyCode.E,
//             KeyCode.J,
//             KeyCode.S,
//             KeyCode.Space
//         };
//     }
//
//     public void StartInputListenAny () {
//         MonoManager.Instance.AddUpdateEvent (CheckInputAny);
//     }
//
//     public void StopInputListenAny () {
//         MonoManager.Instance.RemoveUpdateEvent (CheckInputAny);
//     }
//
//     /// <summary>
//     /// 向事件中心发送按键按下事件
//     /// </summary>
//     /// <param name="key">附加的按键信息</param>
//     private void KeyDown (KeyCode key) {
//         EventCenter.Instance.TiggerEvent ("keyDown", key);
//     }
//
//     /// <summary>
//     /// 向事件中心发送松开按键事件
//     /// </summary>
//     /// <param name="key">附加的按键信息</param>
//     private void KeyUp (KeyCode key) {
//         EventCenter.Instance.TiggerEvent ("keyUp", key);
//     }
//
//     /// <summary>
//     /// 向事件中心发送保持按键按下事件
//     /// </summary>
//     /// <param name="key">附加的按键信息</param>
//     private void KeyKeep (KeyCode key) {
//         EventCenter.Instance.TiggerEvent ("keyKeep", key);
//     }
//
//     /// <summary>
//     /// 检测抬起的任何按键
//     /// </summary>
//     void anyKeyUp () {
//         // if (Input.GetKeyUp (KeyCode.W)) {
//         //     KeyUp (KeyCode.W);
//         // }
//         // if (Input.GetKeyUp (KeyCode.A)) {
//         //     KeyUp (KeyCode.A);
//         // }
//         // if (Input.GetKeyUp (KeyCode.D)) {
//         //     KeyUp (KeyCode.D);
//         // }
//         // if (Input.GetKeyUp (KeyCode.Q)) {
//         //     KeyUp (KeyCode.Q);
//         // }
//         // if (Input.GetKeyUp (KeyCode.E)) {
//         //     KeyUp (KeyCode.E);
//         // }   
//         // if (Input.GetKeyUp (KeyCode.J)) {
//         //     KeyUp (KeyCode.J);
//         // }
//         // if (Input.GetKeyUp (KeyCode.S)) {
//         //     KeyUp (KeyCode.S);
//         // }
//         // if (Input.GetKeyUp (KeyCode.Space))
//         // {
//         //     KeyCode
//         // }
//         foreach (KeyCode key in Enum.GetValues (typeof (KeyCode))) {
//             if (Input.GetKeyUp (key)) {
//                 KeyUp (key);
//             };
//         }
//     }
//
//     /// <summary>
//     /// 在Mono中检测按键输入
//     /// </summary>
//     private void CheckInputAny () {
//         if (Input.anyKey) {
//             foreach (KeyCode keyCode in Enum.GetValues (typeof (KeyCode))) {
//                 if (Input.GetKey (keyCode)) {
//                     KeyKeep (keyCode);
//                 }
//             }
//         }
//         if (Input.anyKeyDown) {
//             foreach (KeyCode keyCode in Enum.GetValues (typeof (KeyCode))) {
//                 if (Input.GetKeyDown (keyCode)) {
//                     KeyDown (keyCode);
//                 }
//             }
//         }
//         anyKeyUp ();
//     }
//
//     private void CheckInputUpdate () {
//         if (Input.anyKey) {
//             foreach (KeyCode keyCode in Enum.GetValues (typeof (KeyCode))) {
//                 if (Input.GetKey (keyCode)) {
//                     KeyKeep (keyCode);
//                 }
//             }
//         }
//     }
// }