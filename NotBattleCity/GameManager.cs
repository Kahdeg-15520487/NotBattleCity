using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using Utility;
using Utility.CustomJsonConverter;
using Utility.Drawing;
using Utility.Drawing.Animation;
using Utility.ScreenManager;

namespace NotBattleCity
{
    public class GameManager : Game
    {
        GraphicsDeviceManager graphics;

        public GameManager()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 600,
                PreferredBackBufferHeight = 400
            };
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";

            CONTENT_MANAGER.Content = Content;
            CONTENT_MANAGER.GameInstance = this;

            CONTENT_MANAGER.LocalRootPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location);
            this.IsMouseVisible = true;

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new RectangleJsonConverter());
                settings.Converters.Add(new AnimationJsonConverter());
                settings.Converters.Add(new AnimatedEntityJsonConverter());
                return settings;
            };
        }

        protected override void Initialize()
        {
            DrawingHelper.Initialize(GraphicsDevice);
            CONTENT_MANAGER.spriteBatch = new SpriteBatch(GraphicsDevice);
            CONTENT_MANAGER.GameInstance = this;
            Primitive2DActionGenerator.CreateThePixel(CONTENT_MANAGER.spriteBatch);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            CONTENT_MANAGER.LoadSpriteSheet("tank", 32, 32);
            CONTENT_MANAGER.LoadSpriteSheet("bullet", 32, 32);
            var terrain = CONTENT_MANAGER.LoadSpriteSheet("terrain", 16, 16);

            CONTENT_MANAGER.LoadFonts("default");

            var animdata = File.ReadAllText("Content/animdata/yellow_tank.json");
            CONTENT_MANAGER.LoadAnimation("yellow_tank", "tank", animdata);

            animdata = File.ReadAllText("Content/animdata/silver_tank.json");
            CONTENT_MANAGER.LoadAnimation("silver_tank", "tank", animdata);

            animdata = File.ReadAllText("Content/animdata/green_tank.json");
            CONTENT_MANAGER.LoadAnimation("green_tank", "tank", animdata);

            animdata = File.ReadAllText("Content/animdata/red_tank.json");
            CONTENT_MANAGER.LoadAnimation("red_tank", "tank", animdata);

            animdata = File.ReadAllText("Content/animdata/bullet.json");
            CONTENT_MANAGER.LoadAnimation("bullet", "bullet", animdata);

            InitScreen();
        }

        private void InitScreen()
        {
            SCREEN_MANAGER.AddScreen(new Screens.GameScreen(GraphicsDevice));
            SCREEN_MANAGER.AddScreen(new Screens.LobbyScreen(GraphicsDevice));

            //SCREEN_MANAGER.GotoScreen("GameScreen");
            SCREEN_MANAGER.GotoScreen("LobbyScreen");

            SCREEN_MANAGER.Init();
        }

        protected override void Update(GameTime gameTime)
        {
            CONTENT_MANAGER.CurrentInputState = new InputState(Mouse.GetState(), Keyboard.GetState());

            SCREEN_MANAGER.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            SCREEN_MANAGER.Draw(CONTENT_MANAGER.spriteBatch, gameTime);

            base.Draw(gameTime);
        }
    }
}
