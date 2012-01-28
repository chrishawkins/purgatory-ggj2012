﻿
namespace Purgatory.Game.Controls
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using System.Collections.Generic;
    using Purgatory.Game.Graphics;

    public class XboxInputController : IInputController
    {
        private PlayerIndex playerIndex;
        private float shootCooldown;
        private float shootTimer;

        public XboxInputController(PlayerIndex player)
        {
            this.playerIndex = player;
        }

        public void UpdateMovement(Player player, GameTime gameTime)
        {
            GamePadState state = GamePad.GetState(this.playerIndex);

            if (player.DashVelocity == Vector2.Zero)
            {
                player.MovementDirection = new Vector2();
                player.MovementDirection = state.ThumbSticks.Left;

                if (player.MovementDirection.LengthSquared() != 0)
                {
                    player.MovementDirection.Normalize();

                    if (state.IsButtonDown(Buttons.A) && player.TimeSinceLastDash > Player.DashCooldownTime)
                    {
                        AudioManager.Instance.PlayCue(ref player.DashSFX, false);
                        player.DashVelocity = player.Speed * 5 * player.MovementDirection;
                        player.TimeSinceLastDash = 0;
                    }
                }
            }
        }

        public void UpdateShoot(Player player, GameTime gameTime)
        {
            GamePadState state = GamePad.GetState(this.playerIndex);

            this.shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (state.Triggers.Right > 0.5f && this.shootTimer > this.shootCooldown && player.Energy > 0)
            {
                Vector2 bulletPos = player.Position;
                Bullet b = new Bullet(bulletPos, player.BulletDirection, player.BulletBounce, 500.0f, new Sprite(player.BulletSprite), player.Level);
                player.BulletList.Add(b);
                --player.Energy;
                this.shootTimer = 0.0f;
                AudioManager.Instance.PlayCue(ref player.ShootSFX, true);
            }

            List<Bullet> tmpBulletList = new List<Bullet>();
            foreach (var bullet in player.BulletList)
            {
                bullet.Update(gameTime);

                if (!bullet.RemoveFromList)
                {
                    tmpBulletList.Add(bullet);
                }
            }

            player.BulletList = tmpBulletList;
        }
    }
}
