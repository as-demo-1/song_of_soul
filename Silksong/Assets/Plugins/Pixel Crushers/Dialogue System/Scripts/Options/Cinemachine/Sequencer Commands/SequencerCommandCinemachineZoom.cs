#if USE_CINEMACHINE
using System.Collections;
using UnityEngine;
using Cinemachine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Sequencer command CinemachineZoom(vcam, zoom, [duration])
    /// 
    /// Sets the target of a Cinemachine vcam.
    /// 
    /// - vcam: The name of a GameObject containing a CinemachineVirtualCamera.
    /// - zoom: The lens orthograpic size.
    /// - duration: Duration over which to transition to zoom. (Default: 0 [instant])
    /// </summary>
    public class SequencerCommandCinemachineZoom: SequencerCommand
    {

        protected virtual IEnumerator Start()
        {
            var vcamGO = GetSubject(0);
            var vcam = (vcamGO != null) ? vcamGO.GetComponent<CinemachineVirtualCamera>() : null;
            var zoom = GetParameterAsFloat(1);
            var duration = GetParameterAsFloat(2, 0);
            if (vcam == null)
            {
                if (DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Sequencer: CinemachineZoom(" + 
                    GetParameters() + "): Can't find virtual camera '" + GetParameter(0) + ".");
            }
            else
            {
                if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Sequencer: CinemachineZoom(" + vcam + ", " + 
                    zoom + ", " + duration + ")");
                if (vcam.m_Lens.Orthographic)
                { 
                    if (duration > 0)
                    {
                        var originalSize = vcam.m_Lens.OrthographicSize;
                        float elapsed = 0;
                        while (elapsed < duration)
                        {
                            vcam.m_Lens.OrthographicSize = Mathf.Lerp(originalSize, zoom, elapsed / duration);
                            yield return null;
                            elapsed += DialogueTime.deltaTime;
                        }
                    }
                    vcam.m_Lens.OrthographicSize = zoom;
                }
                else
                {
                    // May need to wait until Cinemachine 3 for 3D params to be exposed.
                    if (DialogueDebug.LogInfo) Debug.LogWarning("Dialogue System: Sequencer: CinemachineZoom(" + vcam + ", " +
                    zoom + ", " + duration + ") not supported yet for 3D.");
                }
            }
            Stop();
        }

    }

}
#endif
