using System;
using UnityEngine;

namespace Kcc.Base
{
    [Flags]
    public enum TargetTraits
    {
        None = 0,
        Combatable = 1 << 0,
        Interactable = 1 << 1,
    }

    public class TargetTrait : MonoBehaviour
    {
        public TargetTraits traits = TargetTraits.None;
    }
}
