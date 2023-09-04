#if USE_CINEMACHINE
#if UNITY_2017_1_OR_NEWER
using UnityEngine;
using Cinemachine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Sequencer commannd CinemachinePriority(vcam, [priority], [cut])
    /// 
    /// Sets the priority of a Cinemachine virtual camera.
    /// 
    /// - vcam: The name of a GameObject containing a CinemachineVirtualCamera.
    ///    Alternate: 'all' or 'except:GameObject': All affects all vcams; except affects all except one.
    /// - priority: (Optional) New priority level. Default: 999.
    /// - cut: (Optional) Cuts to the vcam instead of allowing Cinemachine to ease there.
    /// </summary>
    public class SequencerCommandCinemachinePriority : SequencerCommand
    {
        private static bool hasRecordedBlendMode = false;

        public System.Collections.IEnumerator Start()
        {
            bool all = false;
            string allExcept = string.Empty;
            bool checkExcept = false;
            CinemachineVirtualCamera vcam = null;

            var vcamName = GetParameter(0);
            if (vcamName == "all")
            {
                all = true;
            }
            else if (vcamName.StartsWith("except:"))
            {
                all = true;
                checkExcept = true;
                allExcept = vcamName.Substring("except:".Length);
            }
            else
            {
                var subject = GetSubject(0);
                vcam = (subject != null) ? subject.GetComponent<CinemachineVirtualCamera>() : null;
            }
            var priority = GetParameterAsInt(1, 999);
            var cut = string.Equals(GetParameter(2), "cut", System.StringComparison.OrdinalIgnoreCase);
            if (!all && vcam == null)
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Sequencer: CinemachinePriority(" + GetParameters() +
                    "): Can't find virtual camera '" + GetParameter(0) + ".");
            }
            else
            {
                if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Sequencer: CinemachinePriority(" + vcamName + ", " + priority + ", cut=" + cut + ")");

                // Handle cut:
                var shouldIRestoreBlendMode = false;
                var cinemachineBrain = cut ? FindObjectOfType<CinemachineBrain>() : null;
                var previousBlendStyle = CinemachineBlendDefinition.Style.EaseInOut;
                var previousBlendTime = 0f;
                if (cut && cinemachineBrain != null)
                {
                    shouldIRestoreBlendMode = !hasRecordedBlendMode;
                    hasRecordedBlendMode = true;
                    previousBlendStyle = cinemachineBrain.m_DefaultBlend.m_Style;
                    previousBlendTime = cinemachineBrain.m_DefaultBlend.m_Time;
                    cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
                    cinemachineBrain.m_DefaultBlend.m_Time = 0;
                    cinemachineBrain.enabled = false;
                }

                if (all)
                {
                    var allVcams = FindObjectsOfType<CinemachineVirtualCamera>();
                    foreach (CinemachineVirtualCamera avcam in allVcams)
                    {
                        if (checkExcept && string.Equals(avcam.name, allExcept)) continue;
                        avcam.Priority = priority;
                        if (cut)
                        {
                            avcam.enabled = false;
                            avcam.enabled = true;
                        }
                    }
                }
                else
                {
                    vcam.Priority = priority;
                    if (cut)
                    {
                        vcam.enabled = false;
                        vcam.enabled = true;
                    }
                }

                // Clean up cut:
                if (cut && cinemachineBrain != null)
                {
                    cinemachineBrain.enabled = true;
                    if (shouldIRestoreBlendMode)
                    {
                        yield return null;
                        cinemachineBrain.m_DefaultBlend.m_Style = previousBlendStyle;
                        cinemachineBrain.m_DefaultBlend.m_Time = previousBlendTime;
                        hasRecordedBlendMode = false;
                    }
                }
            }
            Stop();
        }

    }

}
#endif
#endif
