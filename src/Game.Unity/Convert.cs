using Asteroids.Math;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Asteroids.Game.Unity
{
    public static class Convert
    {
        public static Vector2 Vector2(Math.Vector2 vector2)
        {
            return new Vector2(vector2.x.ToFloat(), vector2.y.ToFloat());
        }

        public static float RadToUnityDeg(this float angle)
        {
            return (angle - Mathf.PI / 2) * Mathf.Rad2Deg * -1;
        }
    }
}