using Asteroids.Game.Match.Physics;
using Asteroids.Math;

namespace Asteroids.Game.Match.Components
{
    public sealed class SpaceshipController : Component
    {
        private const long MoveAcceleration = FixedMath.One / 180;
        private const long MoveDeceleration = FixedMath.One / 30;
        private const long MoveDamping = FixedMath.One / 230;
        private const long MaxForwardSpeed = FixedMath.One / 13;
        private const long MaxBackwardSpeed = FixedMath.One / 17;

        private const long RotationAcceleration = FixedMath.TwoPi / Config.TicksPerSecond / 4;
        private const long RotationDeceleration = FixedMath.TwoPi / Config.TicksPerSecond;
        private const long RotationDamping = FixedMath.TwoPi / Config.TicksPerSecond / 10;
        private const long MaxRotationSpeed = FixedMath.TwoPi / Config.TicksPerSecond / 2;

        private const uint SimpleShotCooldown = Config.TicksPerSecond / 2;
        private const uint AdvancedShotCooldown = Config.TicksPerSecond * 4;


        private const int MoveForwardMask = 1;
        private const int MoveBackwardMask = 1 << 1;
        private const int RotateLeftMask = 1 << 2;
        private const int RotateRightMask = 1 << 3;
        private const int SimpleShotMask = 1 << 4;
        private const int AdvancedShotMask = 1 << 5;

        private readonly Transform transform;

        private byte lastControls;

        private long moveVelocity;
        private long rotationVelocity;

        private uint lastShotTick;
        private uint lastAdvancedShotTick;

        internal SpaceshipController(Entity entity) : base(entity)
        {
            transform = entity.GetComponent<Transform>();
        }

        internal override void OnCreate()
        {
            lastControls = 0;

            moveVelocity = 0;
            rotationVelocity = 0;

            lastShotTick = 0;
            lastAdvancedShotTick = 0;
        }

        internal void SetControls(byte controls)
        {
            lastControls = controls;
        }

        internal override void Update()
        {
            if ((lastControls & MoveForwardMask) != 0)
            {
                if (moveVelocity >= 0) ApplyAcceleration(1, MoveAcceleration, MaxForwardSpeed, ref moveVelocity);
                else ApplyBraking(-1, MoveDeceleration, ref moveVelocity);
            }
            else if ((lastControls & MoveBackwardMask) != 0)
            {
                if (moveVelocity <= 0) ApplyAcceleration(-1, MoveAcceleration, MaxBackwardSpeed, ref moveVelocity);
                else ApplyBraking(1, MoveDeceleration, ref moveVelocity);
            }
            else
            {
                ApplyBraking(moveVelocity >= 0 ? 1 : -1, MoveDamping, ref moveVelocity);
            }

            if ((lastControls & RotateRightMask) != 0)
            {
                if (rotationVelocity <= 0) ApplyAcceleration(-1, RotationAcceleration, MaxRotationSpeed, ref rotationVelocity);
                else ApplyBraking(1, RotationDeceleration, ref rotationVelocity);
            }
            else if ((lastControls & RotateLeftMask) != 0)
            {
                if (rotationVelocity >= 0) ApplyAcceleration(1, RotationAcceleration, MaxRotationSpeed, ref rotationVelocity);
                else ApplyBraking(-1, RotationDeceleration, ref rotationVelocity);
            }
            else
            {
                ApplyBraking(rotationVelocity >= 0 ? 1 : -1, RotationDamping, ref rotationVelocity);
            }

            if ((lastControls & SimpleShotMask) != 0 && entity.entityManager.matchManager.CurrentTick - lastShotTick > SimpleShotCooldown)
            {
                lastShotTick = entity.entityManager.matchManager.CurrentTick;

                OnShot();
            }

            if ((lastControls & AdvancedShotMask) != 0 && entity.entityManager.matchManager.CurrentTick - lastAdvancedShotTick > AdvancedShotCooldown)
            {
                lastAdvancedShotTick = entity.entityManager.matchManager.CurrentTick;

                OnAdvancedShot();
            }

            transform.Position += new Vector2(FixedMath.Cos(transform.Rotation), FixedMath.Sin(transform.Rotation)) * moveVelocity;

            transform.Rotation += rotationVelocity;
        }

        private void OnShot()
        {
            var bullet = entity.entityManager.CreateEntity("Bullet", entity.Owner);

            bullet.GetComponent<Collider>().SetCollisionLayer(CollisionLayersConfig.WeaponLayer);

            var bulletTransform = bullet.GetComponent<Transform>();

            var spaceshipMoveDir = new Vector2(FixedMath.Cos(transform.Rotation), FixedMath.Sin(transform.Rotation));

            //pos with small forward offset
            bulletTransform.Rotation = transform.Rotation;
            bulletTransform.Position = transform.Position + spaceshipMoveDir * FixedMath.One / 120;

            bullet.GetComponent<BulletController>().RecalculateMoveSpeed();
        }

        private void OnAdvancedShot()
        {
            var laser = entity.entityManager.CreateEntity("Laser", entity.Owner);

            var spaceshipMoveDir = new Vector2(FixedMath.Cos(transform.Rotation), FixedMath.Sin(transform.Rotation));

            laser.GetComponent<LaserController>().Init(entity.Pointer, spaceshipMoveDir);
        }

        private static void ApplyAcceleration(int sign, long acceleration, long maxSpeed, ref long velocity)
        {
            velocity += acceleration * sign;
            if (velocity * sign > maxSpeed) velocity = maxSpeed * sign;
        }

        private static void ApplyBraking(int sign, long braking, ref long velocity)
        {
            velocity -= braking * sign;
            if (velocity * sign < 0) velocity = 0;
        }

        public static byte PackControls(int move, int rotation, bool simpleShot, bool advancedShot)
        {
            byte controls = 0;

            if (move > 0) controls = MoveForwardMask;
            else if (move < 0) controls |= MoveBackwardMask;

            if (rotation > 0) controls |= RotateRightMask;
            else if (rotation < 0) controls |= RotateLeftMask;

            if (simpleShot) controls |= SimpleShotMask;

            if (advancedShot) controls |= AdvancedShotMask;

            return controls;
        }
    }
}