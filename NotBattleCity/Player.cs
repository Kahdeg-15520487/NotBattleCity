using System;
using Humper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NotBattleCity.Screens;
using Utility;
using Utility.Drawing.Animation;

namespace NotBattleCity
{
    class Player
    {
        public Direction direction = Direction.North;
        public Humper.Base.Vector2 Position { get => collision.Bounds.Location; }

        internal IBox collision;

        public string pallete = "yellow";
        public AnimatedEntity animatedEntity { get; set; }

        public Bullet bullet { get; private set; }
        private World _world;

        static readonly Humper.Base.Vector2 horizontal = new Humper.Base.Vector2(1, 0);
        static readonly Humper.Base.Vector2 vertical = new Humper.Base.Vector2(0, 1);
        static readonly float velocity = 2;

        public readonly long ID;

        public Player(Point pos, World world, long id)
        {
            ID = id;

            _world = world;
            collision = world.Create(pos.X, pos.Y, 32, 32).AddTags(CollisionTag.Player);
            collision.Data = this;

            animatedEntity = new AnimatedEntity(CONTENT_MANAGER.AnimationEntities[pallete + "_tank"])
            {
                Depth = LayerDepth.Unit
            };
            bullet = new Bullet(ID)
            {
                animatedEntity = new AnimatedEntity(CONTENT_MANAGER.AnimationEntities["bullet"])
            };
            bullet.animatedEntity.Origin = new Vector2(4, 12);
            bullet.Disapear += (o, e) =>
            {
                bullet.IsFlying = false;
                if (bullet.collision != null)
                {
                    _world.Remove(bullet.collision);
                }
            };
        }

        public void Update(GameTime gameTime)
        {
            if (ID == GameScreen.ID)
            {
                bool isMoved = false;
                Humper.Base.Vector2 Position = collision.Bounds.Location;
                var lastDir = direction;
                if (HelperFunction.IsKeyDown(Keys.Left) || HelperFunction.IsKeyDown(Keys.Right))
                {
                    if (HelperFunction.IsKeyDown(Keys.Left))
                    {
                        Position -= horizontal * velocity;
                        direction = Direction.West;
                        isMoved = true;
                    }

                    if (HelperFunction.IsKeyDown(Keys.Right))
                    {
                        Position += horizontal * velocity;
                        direction = Direction.East;
                        isMoved = true;
                    }
                }
                else
                if (HelperFunction.IsKeyDown(Keys.Up) || HelperFunction.IsKeyDown(Keys.Down))
                {
                    if (HelperFunction.IsKeyDown(Keys.Up))
                    {
                        Position -= vertical * velocity;
                        direction = Direction.North;
                        isMoved = true;
                    }

                    if (HelperFunction.IsKeyDown(Keys.Down))
                    {
                        Position += vertical * velocity;
                        direction = Direction.South;
                        isMoved = true;
                    }
                }

                if (isMoved)
                {
                    GameScreen.QueueCommand(ID, Command.MovePlayer, Position.X, Position.Y);
                }
                else if (animatedEntity.IsPlaying)
                {
                    GameScreen.QueueCommand(ID, Command.StopPlayer, 0, 0);
                }

                if (lastDir != direction)
                {
                    GameScreen.QueueCommand(ID, Command.RotatePlayer, (int)direction, 0);
                }

                if (HelperFunction.IsKeyPress(Keys.Tab))
                {
                    int color = 0;
                    switch (pallete)
                    {
                        case "yellow":
                            color = 1;
                            break;
                        case "silver":
                            color = 2;
                            break;
                        case "green":
                            color = 3;
                            break;
                        case "red":
                            color = 0;
                            break;
                    }
                    GameScreen.QueueCommand(ID, Command.ChangePlayerColor, color, 0);
                }

                if (HelperFunction.IsKeyPress(Keys.Space) && !bullet.IsFlying)
                {
                    float x = 0, y = 0;
                    switch (direction)
                    {
                        case Direction.North:
                            x = Position.X + 10;
                            y = Position.Y - 8;
                            break;
                        case Direction.South:
                            x = Position.X + 10;
                            y = Position.Y + collision.Bounds.Height;
                            break;

                        case Direction.West:
                            x = Position.X - 8;
                            y = Position.Y + 12;
                            break;
                        case Direction.East:
                            x = Position.X + collision.Bounds.Width;
                            y = Position.Y + 12;
                            break;
                    }
                    GameScreen.QueueCommand(ID, Command.CreateBullet, x, y);

                }

                if (bullet.IsFlying)
                {
                    bullet.Update(gameTime);
                }

                if (isMoved)
                {
                    animatedEntity.ContinueAnimation();
                }
                else
                {
                    animatedEntity.PauseAnimation();
                }
            }

            switch (direction)
            {
                case Direction.North:
                    animatedEntity.PlayAnimation("up");
                    break;
                case Direction.West:
                    animatedEntity.PlayAnimation("left");
                    break;
                case Direction.East:
                    animatedEntity.PlayAnimation("right");
                    break;
                case Direction.South:
                    animatedEntity.PlayAnimation("down");
                    break;
            }

            animatedEntity.Update(gameTime);
        }

        internal void ExecuteCommand(NetCommand netcmd)
        {
            switch (netcmd.Command)
            {
                case Command.ChangePlayerColor:
                    switch (netcmd.I1)
                    {
                        case 0:
                            animatedEntity = new AnimatedEntity(CONTENT_MANAGER.AnimationEntities["yellow_tank"]);
                            pallete = "yellow";
                            break;
                        case 1:
                            animatedEntity = new AnimatedEntity(CONTENT_MANAGER.AnimationEntities["silver_tank"]);
                            pallete = "silver";
                            break;
                        case 2:
                            animatedEntity = new AnimatedEntity(CONTENT_MANAGER.AnimationEntities["green_tank"]);
                            pallete = "green";
                            break;
                        case 3:
                            animatedEntity = new AnimatedEntity(CONTENT_MANAGER.AnimationEntities["red_tank"]);
                            pallete = "red";
                            break;
                    }
                    break;
                case Command.MovePlayer:
                    Move(netcmd.X, netcmd.Y);
                    break;
                case Command.RotatePlayer:
                    direction = (Direction)netcmd.I1;
                    break;
                case Command.StopPlayer:
                    Stop();
                    break;
                case Command.MoveBullet:
                    bullet.Move(netcmd.X, netcmd.Y);
                    break;
                case Command.CreateBullet:
                    CreateBullet(netcmd.X, netcmd.Y);
                    break;
                case Command.DestroyBullet:
                    bullet.Destroy();
                    break;
            }
        }

        public void Move(float x, float y)
        {
            collision.Move((int)x, (int)y, (c) =>
            {

                if (c.Other.HasTag(CollisionTag.Ignore))
                {
                    return Humper.Responses.CollisionResponses.Cross;
                }

                return Humper.Responses.CollisionResponses.Slide;
            });

            animatedEntity.ContinueAnimation();
        }

        public void Stop()
        {
            animatedEntity.PauseAnimation();
        }

        public void CreateBullet(float x, float y)
        {
            switch (direction)
            {
                case Direction.North:
                    bullet.animatedEntity.PlayAnimation("up");
                    break;
                case Direction.South:
                    bullet.animatedEntity.PlayAnimation("down");
                    break;

                case Direction.West:
                    bullet.animatedEntity.PlayAnimation("left");
                    break;
                case Direction.East:
                    bullet.animatedEntity.PlayAnimation("right");
                    break;
            }
            bullet.IsFlying = true;
            bullet.IsSent = false;
            bullet.Direction = direction;
            bullet.collision = _world.Create(x, y, 8, 8).AddTags(CollisionTag.Bullet);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animatedEntity.Position = Position.ToMonogameVector2();
            animatedEntity.Draw(spriteBatch, gameTime);

            if (bullet.IsFlying)
            {
                bullet.Draw(gameTime, spriteBatch);
            }
        }
    }
}
