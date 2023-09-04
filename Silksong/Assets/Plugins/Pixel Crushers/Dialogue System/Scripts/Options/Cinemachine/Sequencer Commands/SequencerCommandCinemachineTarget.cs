#if USE_CINEMACHINE
using UnityEngine;
using Cinemachine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Sequencer command CinemachineTarget(vcam, [target], [look|follow|both], [cut])
    /// 
    /// Sets the target of a Cinemachine vcam.
    /// 
    /// - vcam: The name of a GameObject containing a CinemachineVirtualCamera.
    /// - target: (Optional) The name of a GameObject to look at. Default: speaker.
    /// - mode: (Optional) Set Look At, Follow, or both. Default: both.
    /// </summary>
    public class SequencerCommandCinemachineTarget : SequencerCommand
    {

        public void Awake()
        {
            var vcamGO = GetSubject(0);
            var vcam = (vcamGO != null) ? vcamGO.GetComponent<CinemachineVirtualCamera>() : null;
            var target = GetSubject(1, speaker);
            var mode = GetParameter(2);
            mode = string.IsNullOrEmpty(mode) ? "both" : mode.ToLower();
            if (vcam == null)
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Sequencer: CinemachineTarget(" + GetParameters() +
                    "): Can't find virtual camera '" + GetParameter(0) + ".");
            }
            else if (target == null)
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Sequencer: CinemachineTarget(" + GetParameters() +
                    "): Can't find target.");
            }
            else if (mode != "look" && mode != "follow" && mode != "both")
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Sequencer: CinemachineTarget(" + GetParameters() +
                    "): Mode must be 'look', 'follow', or 'both'.");
            }
            else
            {
                if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Sequencer: CinemachineTarget(" + vcam + ", " + target + ", " + mode + ")");
                if (mode == "look" || mode == "both") vcam.LookAt = target.transform;
                if (mode == "follow" || mode == "both") vcam.Follow = target.transform;
            }
            Stop();
        }

    }

}
#endif
