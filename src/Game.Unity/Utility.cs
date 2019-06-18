using UnityEngine;

namespace Asteroids.Game.Unity
{
    public static class Utility
    {
        public static float LerpAngleUnclamped(float a, float b, float t)
        {
            var num = Mathf.Repeat(b - a, 360f);
            if (num > 180.0) num -= 360f;
            return a + num * t;
        }
    }
}