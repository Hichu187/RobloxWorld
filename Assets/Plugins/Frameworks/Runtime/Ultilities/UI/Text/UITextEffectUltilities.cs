using System.Collections;
using TMPro;
using UnityEngine;

namespace Hichu
{
    public static class UITextEffectUltilities
    {
        public static IEnumerator TextMeshTypingEffect(TextMeshProUGUI tmp, string str, float speed)
        {
            tmp.text = "";
            yield return new WaitForSeconds(1f);

            foreach (char letter in str.ToCharArray())
            {
                tmp.text += letter;
                yield return new WaitForSeconds(speed);
            }
        }
    }
}
