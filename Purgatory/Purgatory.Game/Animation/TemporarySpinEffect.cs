﻿
namespace Purgatory.Game.Animation
{
    using System;
    using Microsoft.Xna.Framework;
    using Purgatory.Game.Graphics;

    public class TemporarySpinEffect : SpriteEffect
    {
        private float startRotation;
        private float numSpins;

        public TemporarySpinEffect(float milliseconds, float startRotation, float numSpins)
        {
            this.Duration = TimeSpan.FromMilliseconds(milliseconds);
            this.startRotation = startRotation;
            this.numSpins = numSpins;
        }

        protected override void Update(Sprite sprite, float time)
        {
            sprite.Rotation = MathHelper.WrapAngle(startRotation + (time * MathHelper.TwoPi) * numSpins);
        }
    }
}
