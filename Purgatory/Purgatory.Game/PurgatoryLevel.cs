﻿
namespace Purgatory.Game
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Purgatory.Game.Graphics;
    using Purgatory.Game.PowerUps;
    using Purgatory.Game.Animation;

    public class PurgatoryLevel : Level
    {
        private Sprite overlay;
        public Portal Portal { get; set; }

        public PurgatoryLevel(string levelType, TileType[][] maze1, TileType[][] maze2)
            : base()
        {
            this.pickupSFX = AudioManager.Instance.LoadCue("Purgatory_PickupItem");

            // Temp list of rectangles to return
            rectangles = new List<Rectangle>();

            this.overlay = new Sprite(BigEvilStatic.Content.Load<Texture2D>("PurgMapOverlay"), 800, 800);
            this.overlay.Alpha = 1f;
            this.overlay.Zoom = 1.15f;
            this.overlay.Effects.Add(new PulsateEffect(1000f, 0.4f));
            this.overlay.Effects.Add(new SpinEffect(4000f));

            // Set Tiles Wide
            HalfTilesWideOnScreen = (int)Math.Ceiling((double)BigEvilStatic.Viewport.Width / 4 / TileWidth);
            HalfTilesLongOnScreen = (int)Math.Ceiling((double)BigEvilStatic.Viewport.Height / 2 / TileWidth);

            //Debug textures
            Texture2D wallTex = BigEvilStatic.Content.Load<Texture2D>(levelType + "Wall");
            Texture2D wallTopTex = BigEvilStatic.Content.Load<Texture2D>(levelType + "WallTop");

            wall = new Sprite(wallTex, TileWidth, TileWidth);
            wallTop = new Sprite(wallTopTex, TileWidth, TileWidth);

            Texture2D backgroundTex = BigEvilStatic.Content.Load<Texture2D>(levelType + "Ground");
            backgroundGround = new Sprite(backgroundTex, backgroundTex.Width, backgroundTex.Height);

            //Fill tile array from pixel data
            WalkableTile = new TileType[maze1.Length][];
            for (int i = 0; i < maze1.Length; ++i)
            {
                WalkableTile[i] = new TileType[maze1[i].Length];
            }

            for (int i = 0; i < WalkableTile.Length; ++i)
            {
                for (int j = 0; j < WalkableTile[i].Length; ++j)
                {
                    if (maze1[i][j] == TileType.Ground || maze2[i][j] == TileType.Ground)
                    {
                        WalkableTile[i][j] = TileType.Ground;
                    }
                    else
                    {
                        WalkableTile[i][j] = TileType.Wall;
                    }
                }
            }

            //for (int i = 0; i < WalkableTile.Length; ++i)
            //{
            //    for (int j = 0; j < WalkableTile[i].Length; ++j)
            //    {
            //        if (WalkableTile[i][j] == TileType.Wall)
            //        {
            //            if (j + 1 < WalkableTile[i].Length && WalkableTile[i][j + 1] == TileType.Ground)
            //            {
            //                WalkableTile[i][j] = TileType.WallTop;
            //            }
            //            else if (j - 1 >= 0 && WalkableTile[i][j - 1] == TileType.Ground)
            //            {
            //                WalkableTile[i][j] = TileType.WallBottom;
            //            }
            //        }
            //    }
            //}

            pickUps = new List<PlayerPickUp>();
        }

        internal override void PlayPurgatoryAnimation()
        {
            base.PlayPurgatoryAnimation();
            this.purgatoryText.Alpha = 1;
            this.findPortalText.Alpha = 1;
            this.purgatoryText.Effects.Add(new FadeEffect(5000, true, 1));
            this.findPortalText.Effects.Add(new FadeEffect(5000, true, 1));
        }

        public override void AddToPickups(PlayerPickUp pickUp, Vector2 playerPosition, int minDistance, bool safe4)
        {
            base.AddToPickups(pickUp, playerPosition, minDistance, safe4);

            if (pickUp is Portal)
            {
                this.Portal = pickUp as Portal;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            this.overlay.Alpha = 1f;
            this.overlay.UpdateEffects(gameTime);
        }

        public override void Draw(SpriteBatch batch, Bounds bounds)
        {
            base.Draw(batch, bounds);
            this.overlay.Draw(batch, new Vector2(bounds.Rectangle.Left + bounds.Rectangle.Width / 2f, bounds.Rectangle.Height / 2f));
            //this.overlay.Draw(batch, new Vector2(0f, 0f));
        }
    }
}
