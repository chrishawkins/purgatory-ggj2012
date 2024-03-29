﻿
namespace Purgatory.Game.PowerUps
{
    using System;

    public class NoClipPowerUp : PlayerPickUp
    {
        public static readonly TimeSpan Duration = TimeSpan.FromSeconds(6);

        public NoClipPowerUp() : base("NoClipPickUp")
        {
        }

        public override void PlayerEffect(Player player)
        {
            player.NoClipTime = TimeSpan.Zero;
        }
    }
}
