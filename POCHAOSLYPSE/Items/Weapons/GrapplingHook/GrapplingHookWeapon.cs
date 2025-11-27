using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class GrapplingHookWeapon : Weapon
    {
        // Configuración del hook
        private const float HookSpeed       = 900f;
        private const float HookMaxDistance = 900f;

        // Configuración de la fuerza de atracción
        private const float MinPullStrength = 2000f; // fuerza a corta distancia
        private const float MaxPullStrength = 8000f; // fuerza a larga distancia
        private const float StopDistance    = 24f;   // distancia mínima para "llegó"

        public GrappleProjectile CurrentHook { get; private set; }

        public GrapplingHookWeapon(Texture2D texture, Rectangle srcRec, Rectangle destRect, Color color)
            // FireRate bajo: no queremos spamear hooks
            : base(texture, srcRec, destRect, fireRate: 1.5f, knockback: 0f, color)
        {
        }

        // No usamos el sistema normal de proyectiles para este arma
        public override void Fire(Vector2 muzzle, Vector2 dir,
                                  List<Projectile> projectiles,
                                  Entity owner)
        {
            // Intencionalmente vacío: el hook se maneja con StartGrapple/UpdateHook/Release
        }

        public void StartGrapple(Vector2 origin, Vector2 dir, Player owner)
        {
            if (!CanFire)
                return;

            // Si ya hay un hook vivo, lo matamos y creamos uno nuevo
            if (CurrentHook != null && CurrentHook.IsAlive)
            {
                CurrentHook.ForceKill();
                CurrentHook = null;
            }

            if (dir == Vector2.Zero)
                dir = Vector2.UnitX;

            dir.Normalize();

            Vector2 vel = dir * HookSpeed;

            CurrentHook = new GrappleProjectile(
                startPos: origin,
                velocity: vel,
                maxDistance: HookMaxDistance,
                radius: 4f
            );

            ResetCooldown();
        }

        public void UpdateHook(GameTime gameTime, TileMap tileMap, Player owner, bool isHoldingButton)
        {
            if (CurrentHook == null || !CurrentHook.IsAlive)
            {
                CurrentHook = null;
                return;
            }

            // Actualizar viaje / colisiones
            CurrentHook.Update(gameTime, tileMap);

            if (!CurrentHook.IsAlive)
            {
                CurrentHook = null;
                return;
            }

            if (CurrentHook.IsLatched)
            {
                if (!isHoldingButton)
                {
                    // Si soltaste el botón, dejamos de tirar y removemos el hook
                    CurrentHook.ForceKill();
                    CurrentHook = null;
                    return;
                }

                // Atracción hacia el HookPoint
                Vector2 toHook = CurrentHook.HookPoint - owner.Center;
                float dist     = toHook.Length();

                if (dist <= StopDistance)
                {
                    // Ya llegó suficientemente cerca: matar hook
                    CurrentHook.ForceKill();
                    CurrentHook = null;
                    return;
                }

                if (dist > 0.1f)
                {
                    Vector2 dir = toHook / dist;

                    // Normalizar factor dist/maxDist
                    float t = MathHelper.Clamp(dist / HookMaxDistance, 0f, 1f);

                    // Interpolamos fuerza entre min y max
                    float strength = MathHelper.Lerp(MinPullStrength, MaxPullStrength, t);

                    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Usamos ApplyKnockback como "impulso" hacia el hook
                    owner.ApplyKnockback(dir * strength * dt);
                }
            }
        }

        public void Release()
        {
            if (CurrentHook != null)
            {
                CurrentHook.ForceKill();
                CurrentHook = null;
            }
        }
    }
}
