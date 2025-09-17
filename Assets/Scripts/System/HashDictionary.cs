using UnityEngine;

namespace Game
{
    public static class HashDictionary
    {
        public static int velocityX = Animator.StringToHash("VelocityX");
        public static int velocityZ = Animator.StringToHash("VelocityZ");

        public static int climbY = Animator.StringToHash("ClimbY");

        public static int jumping = Animator.StringToHash("Jumping");
        public static int climbing = Animator.StringToHash("Climbing");

        public static int win = Animator.StringToHash("Win");
        public static int dead = Animator.StringToHash("Dead");
        public static int revive = Animator.StringToHash("Ground");

        public static int knockback = Animator.StringToHash("Knockback");
        public static int tug = Animator.StringToHash("Tug");
        public static int sit = Animator.StringToHash("Sitting");

        public static int block = Animator.StringToHash("SlashEnd");
        
        public static int IsHolding = Animator.StringToHash("IsHolding");
        public static int HoldingMotion = Animator.StringToHash("HoldingMotion");
    }
}
