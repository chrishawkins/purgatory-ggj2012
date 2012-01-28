﻿
namespace Purgatory.Game.Animation
{
    using System;
    using Microsoft.Xna.Framework;
    using Purgatory.Game.Graphics;

    public class PainEffect : SpriteEffect
    {
        public PainEffect()
        {
            this.Duration = TimeSpan.FromMilliseconds(700);
        }

        public override void Update(Sprite sprite, float time)
        {
            if (time > 0.5f)
            {
                time = 1f - time;
            }

            sprite.Tint = new Color(1f, 1f - time * 2, 1f - time * 2);
        }
    }
}
