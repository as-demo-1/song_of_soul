using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Gamekit2D
{
    public class InputDisplayUI : MonoBehaviour
    {
        private void OnEnable()
        {
            TextMeshProUGUI textUI = GetComponent<TextMeshProUGUI>();
            if (PlayerInput.Instance == null)
            {
                textUI.SetText("## ERROR ## No PlayerInput detected");
                return;
            }

            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("{0} - Move Left\n", PlayerInput.Instance.Horizontal.negative.ToString());
            builder.AppendFormat("{0} - Move Right\n", PlayerInput.Instance.Horizontal.positive.ToString());
            builder.AppendFormat("{0} - Look Up\n", PlayerInput.Instance.Vertical.positive.ToString());
            builder.AppendFormat("{0} - Crouch\n", PlayerInput.Instance.Vertical.negative.ToString());
            builder.AppendFormat("{0} - Jump\n", PlayerInput.Instance.Jump.key.ToString());
            builder.AppendFormat("{0} - Fire range weapon\n", PlayerInput.Instance.RangedAttack.key.ToString());
            builder.AppendFormat("{0} - Melee Attack\n", PlayerInput.Instance.MeleeAttack.key.ToString());
            builder.AppendFormat("{0} - Pause Menu\n", PlayerInput.Instance.Pause.key.ToString());

            textUI.SetText(builder);
        }
    }
}