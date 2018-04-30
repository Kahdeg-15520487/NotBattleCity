using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NotBattleCity.Screens;
using Utility;
using Utility.Drawing.Animation;

namespace NotBattleCity
{
    class Bullet
    {
        public Direction Direction { get; set; } = Direction.North;
        public Humper.Base.Vector2 Position { get => collision.Bounds.Location; }

        internal IBox collision;

        public AnimatedEntity animatedEntity { get; set; }

        static readonly Humper.Base.Vector2 horizontal = new Humper.Base.Vector2(1, 0);
        static readonly Humper.Base.Vector2 vertical = new Humper.Base.Vector2(0, 1);
        static readonly Vector2 origin = new Vector2(4, 12);
        private readonly long ID;

        public float Velocity { get; set; } = 0.4f;
        public bool IsFlying = false;
        public bool IsSent = false;

        public event EventHandler<object> Disapear;

        public Bullet(long id)
        {
            ID = id;
        }

        public void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            var mv = GetMovementVector(delta);

            GameScreen.outQueue.Add(NetCommand.WriteCommand(ID, Command.MoveBullet, Position.X + mv.X, Position.Y + mv.Y));

        }

        public void Move(float x, float y)
        {
            var movement = collision.Move(x, y, (c) => Humper.Responses.CollisionResponses.Cross);

            if (movement.HasCollided && !IsSent)
            {
                var other = movement.Hits.First();
                if (other != null
                    && !other.Box.HasTag(CollisionTag.Ignore)
                    && !other.Box.HasTag(CollisionTag.Water))
                {
                    if (other.Box.HasTag(CollisionTag.Player)
                        && other.Box != null
                        && ((Player)other.Box.Data).ID == ID)
                    {

                    }
                    else
                    {
                        Console.WriteLine(other.Box.Data?.ToString());
                        GameScreen.outQueue.Add(NetCommand.WriteCommand(ID, Command.DestroyBullet, 0, 0));
                        IsSent = true;
                    }
                }
            }
        }

        public void Destroy()
        {
            Disapear?.Invoke(this, null);
        }

        Vector2 GetMovementVector(float delta)
        {
            Vector2 mv = new Vector2();
            switch (Direction)
            {
                case Direction.North:
                    mv.Y = -1;
                    break;
                case Direction.West:
                    mv.X = -1;
                    break;
                case Direction.East:
                    mv.X = 1;
                    break;
                case Direction.South:
                    mv.Y = 1;
                    break;
            }
            mv *= delta * Velocity;
            return mv;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animatedEntity.Position = Position.ToMonogameVector2();
            animatedEntity.Draw(spriteBatch, gameTime);
        }

        void OnDisapear(object e)
        {
            Disapear?.Invoke(this, e);
        }
    }
}
