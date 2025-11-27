using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Katana : MeleeWeapon
    {
        // ✅ hitbox del ataque y duración
        public Rectangle? AttackHitbox { get; private set; }
        private float attackTimer;
        private const float AttackDuration = 0.12f; // segundos
        public bool HasHitThisSwing { get; set; }

        public Katana(Texture2D texture, Rectangle srcRec, Rectangle destRect, Color color)
            : base(texture, srcRec, destRect, fireRate: 2.0f, knockback: 0f, color)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (attackTimer > 0f)
            {
                attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (attackTimer <= 0f)
                {
                    AttackHitbox = null;
                    HasHitThisSwing = false;
                }
            }
        }

        public override void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner)
        {
            if (!CanFire) return;

            if (dir == Vector2.Zero)
                dir = Vector2.UnitX;

            dir.Normalize();

            var ownerRect = owner.destinationRectangle;

            // ✅ hitbox delante del jugador
            int hitboxWidth  = ownerRect.Width + 40;
            int hitboxHeight = ownerRect.Height;

            int x;
            if (dir.X < 0)
                x = ownerRect.Left - hitboxWidth; // hacia la izquierda
            else
                x = ownerRect.Right;               // hacia la derecha

            int y = ownerRect.Top;

            AttackHitbox = new Rectangle(x, y, hitboxWidth, hitboxHeight);
            attackTimer  = AttackDuration;
            HasHitThisSwing = false;

            ResetCooldown();
        }
    }
}
