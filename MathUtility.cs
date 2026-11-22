using OpenTK.Mathematics;

namespace CG
{
    public static class MathUtility
    {
        public static float ExponentialLerpBlend(float speed, float delta) => 1f - MathF.Exp(-speed * delta);
        public static float LerpAngleDegrees(float current, float target, float t)
        {
            float delta = (target - current) % 360f;
            if (delta > 180f) delta -= 360f;
            if (delta < -180f) delta += 360f;
            return current + delta*t;
        }
        public static Vector3 LerpEulerDegrees(Vector3 current, Vector3 target, float t)
        {
            return new Vector3(
                LerpAngleDegrees(current.X, target.X, t),
                LerpAngleDegrees(current.Y, target.Y, t),
                LerpAngleDegrees(current.Z, target.Z, t)
            );
        }
    }
}