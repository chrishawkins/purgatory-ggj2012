﻿
namespace Purgatory.Game
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Purgatory.Game.Controls;
    using Purgatory.Game.Graphics;
    using Purgatory.Game.Physics;
    using System.Collections.Generic;
    using System;
    using Microsoft.Xna.Framework.Audio;

    public class Player : IMoveable
    {
        public static bool InputFrozen = false;
        public float Speed { get; set; }
        private float dashShadowInterval = 1f;

        private Vector2 direction;
        public Vector2 MovementDirection { get; set; }
        private DirectionalSprite sprite;
        private PlayerNumber playerNumber;
        public Vector2 BulletDirection { get; set; }

        private List<float> xPenetrations;
        private List<float> yPenetrations;

        public Vector2 DashVelocity { get; set; }

        public const float DashCooldownTime = 1;
        public float TimeSinceLastDash { get; set; }

        private List<DashSprite> dashPath;
        private Vector2 lastDashSprite;

        public Sprite BulletSprite { get; set; }
        private IInputController inputController;

        public int Health { get; set; }
        public int Energy { get; set; }

        public int BulletBounce { get; set; }
        public List<Bullet> BulletList { get; set; }

        private Rectangle collisionRectangle;

        public Cue ShootSFX;
        public Cue DamageSFX;
        public Cue DeathSFX;
        public Cue DashSFX;

        public Player(PlayerNumber playerNumber)
        {
            this.BulletBounce = 0;
            dashPath = new List<DashSprite>();
            lastDashSprite = new Vector2(float.PositiveInfinity);

            this.TimeSinceLastDash = 100;
            this.Speed = 350;
            this.playerNumber = playerNumber;
            this.Health = 20;
            this.Energy = 100;
            this.BulletList = new List<Bullet>();
            this.direction = new Vector2(0, 1);

            this.xPenetrations = new List<float>();
            this.yPenetrations = new List<float>();

            if (this.playerNumber == PlayerNumber.PlayerOne)
            {
                this.ShootSFX = AudioManager.Instance.LoadCue("Purgatory_HaloThrow");
                this.DamageSFX = AudioManager.Instance.LoadCue("Purgatory_LifeDamageScream");
                this.DeathSFX = AudioManager.Instance.LoadCue("Purgatory_LifeDyingScream");
            }
            else
            {
                this.ShootSFX = AudioManager.Instance.LoadCue("Purgatory_ScytheThrow");
                this.DamageSFX = AudioManager.Instance.LoadCue("Purgatory_DeathDamageScream");
                this.DeathSFX = AudioManager.Instance.LoadCue("Purgatory_DeathDyingScream");
            }

            this.DashSFX = AudioManager.Instance.LoadCue("Purgatory_PlayerDash");
        }

        public void Initialize(IInputController controller, DirectionalSprite sprite, Sprite bulletSprite)
        {
            this.inputController = controller;
            this.sprite = sprite;
            this.BulletSprite = bulletSprite;
            this.collisionRectangle = new Rectangle(0, 0, sprite.Width, sprite.Height);
            
            this.position = Level.FindSpawnPoint(true);
            
            //if (this.playerNumber == PlayerNumber.PlayerOne)
            //{
            //    this.position = new Vector2(Level.TileWidth) * 55;
            //}
            //else if (this.playerNumber == PlayerNumber.PlayerTwo)
            //{
            //    this.position = new Vector2(Level.TileWidth * 127, Level.TileWidth * 141);
            //}
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
        }

        public Vector2 LastPosition { get; set; }

        private Vector2 position;

        public void Update(GameTime gameTime)
        {
            if (!InputFrozen)
            {
                TimeSinceLastDash += (float)gameTime.ElapsedGameTime.TotalSeconds;
                this.UpdateMovement(gameTime);

                // Update player direction. Dont change if movement direction has no length
                if (MovementDirection.LengthSquared() != 0)
                {
                    this.direction = MovementDirection;
                    this.sprite.PlayAnimation = true;
                }
                else
                {
                    this.sprite.PlayAnimation = false;
                }

                this.inputController.UpdateShoot(this, gameTime);
            }

            this.sprite.UpdateAnimation(gameTime);
            this.Level.CheckPickUpCollisions(this);
        }

        public void Draw(SpriteBatch batch, Bounds bounds)
        {
            this.sprite.Draw(this.direction, batch, bounds.AdjustPoint(this.Position));

            foreach (var bullet in BulletList)
            {
                bullet.Draw(batch, bounds);
            }

            foreach (var dash in dashPath)
            {
                dash.Draw(batch, bounds);
            }
        }

        private void UpdateMovement(GameTime time)
        {
            this.inputController.UpdateMovement(this, time);

            if (this.DashVelocity != Vector2.Zero)
            {
                this.LastPosition = this.Position;
                this.position += DashVelocity * (float)time.ElapsedGameTime.TotalSeconds;
                this.DashVelocity -= 30 * this.Speed * this.MovementDirection * (float)time.ElapsedGameTime.TotalSeconds;

                if (DashVelocity.LengthSquared() <= Speed * Speed)
                {
                    this.DashVelocity = Vector2.Zero;
                }
            }
            else
            {
                this.lastDashSprite = new Vector2(float.PositiveInfinity);
                this.LastPosition = this.Position;
                this.position += MovementDirection * Speed * (float)time.ElapsedGameTime.TotalSeconds;
            }

            this.CheckForCollisions();

            // Update dash path transparency.
            List<DashSprite> tmp = new List<DashSprite>();
            foreach (var dashSprite in this.dashPath)
            {
                dashSprite.update(time);
                if (!dashSprite.RemoveFromList)
                {
                    tmp.Add(dashSprite);
                }
            }
            this.dashPath = tmp;

            if(this.DashVelocity != Vector2.Zero)
            {   
                float distanceCheck = 15;
                if(Vector2.DistanceSquared(lastDashSprite, this.position) > distanceCheck * distanceCheck)
                {
                    if (float.IsInfinity(this.lastDashSprite.X))
                    {
                        this.lastDashSprite = this.position;

                    }
                    else
                    {
                        Vector2 posChange = (this.position - lastDashSprite);
                        posChange.Normalize();
                        lastDashSprite += (posChange * distanceCheck);
                    }

                    DashSprite dashSprite = new DashSprite(this.sprite.CreateSprite(this.MovementDirection), lastDashSprite);
                    this.dashPath.Add(dashSprite);
                }
            }
        }

        private void CheckForCollisions()
        {
            if (this.Position != this.LastPosition)
            {
                this.xPenetrations.Clear();
                this.yPenetrations.Clear();
                List<Rectangle> possibleRectangles = Level.GetPossibleRectangles(Position, LastPosition);

                foreach (Rectangle r in possibleRectangles)
                {
                    Vector2 penetration = CollisionSolver.SolveCollision(this, r);
                    if (penetration.X != 0)
                    {
                        this.xPenetrations.Add(penetration.X);
                    }
                    if (penetration.Y != 0)
                    {
                        this.yPenetrations.Add(penetration.Y);
                    }
                }

                if (xPenetrations.Count != 0 || yPenetrations.Count != 0)
                {
                    if (xPenetrations.Count >= yPenetrations.Count)
                    {
                        this.position.X -= xPenetrations[0];
                    }
                    if (yPenetrations.Count >= xPenetrations.Count)
                    {
                        this.position.Y -= yPenetrations[0];
                    }
                }
            }
        }

        public Level Level { get; set; }

        public Rectangle CollisionRectangle
        {
            get { return GeometryUtility.GetAdjustedRectangle(this.Position, this.collisionRectangle); }
        }

        public void CheckBulletCollisions(List<Bullet> list)
        {
            foreach (var bullet in list)
            {
                if (this.CollisionRectangle.Intersects(bullet.CollisionRectangle))
                {
                    bullet.RemoveFromList = true;
                    this.Health -= 1;

                    if (this.Health > 0)
                    {
                        AudioManager.Instance.PlayCue(ref this.DamageSFX, false);
                    }
                    else if (Level is PurgatoryLevel)
                    {
                        AudioManager.Instance.PlayCue(ref this.DeathSFX, false);
                        Player.InputFrozen = true;
                    }
                }
            }
        }

        internal void SetBulletDirection(Vector2 targetPosition)
        {
            this.BulletDirection = targetPosition - this.Position;
            this.BulletDirection.Normalize();
        }
    }
}
