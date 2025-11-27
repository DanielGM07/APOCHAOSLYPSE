using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public abstract class Weapon : Item
    {
        protected float fireCooldown;
        public float FireRate { get; private set; }   // disparos por segundo
        public float Knockback { get; private set; }  // knockback en píxeles hacia atrás

        protected Weapon(Texture2D texture, Rectangle srcRec, Rectangle destRect,
                         float fireRate, float knockback, Color color)
            : base(texture, srcRec, destRect, color)
        {
            FireRate = fireRate;
            Knockback = knockback;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (fireCooldown > 0f)
            {
                fireCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        protected bool CanFire => fireCooldown <= 0f;

        protected void ResetCooldown()
        {
            if (FireRate <= 0f)
                fireCooldown = 0f;
            else
                fireCooldown = 1f / FireRate;
        }

        // muzzle: punto de salida del arma
        // dir: dirección normalizada de disparo
        public abstract void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner);
    }
}
