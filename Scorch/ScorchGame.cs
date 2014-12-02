using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Scorch.DataModels;
using Scorch.Graphics;
using Scorch.Input;
using Scorch.Physics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Scorch
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ScorchGame : Game
    {
        // graphics
        public SpriteBatch SpriteBatch;
        public GraphicsEngine GraphicsEngine;
        public GraphicsDeviceManager GraphicsDeviceManager;

        // content
        public Dictionary<string, Texture2D> TextureAssets;
        public SpriteFont HudFont;

        // input
        public InputManager InputManager;
        public HeadsUpDisplay HUD;
        public delegate void GameEventHandler(ScorchGame game);

        // data models
        public Terrain Terrain;
        public Tank[] Tanks;
        public int CurrentPlayerIndex;
        public int ProjectileId;
        public Tank CurrentPlayerTank
        {
            get
            {
                return Tanks[CurrentPlayerIndex];
            }
        }

        // other
        public PhysicsEngine PhysicsEngine;
        public Random Randomizer;

        public ScorchGame()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Randomizer = new Random();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            InitializeInput();
            InitializeGraphics(HUD);
            InitializeField(GraphicsEngine);
            HUD.SetCurrentPlayer(CurrentPlayerTank);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";
            TextureAssets = new Dictionary<string, Texture2D>();
            foreach (var textureFilePath in Directory.GetFiles(@"Content\Graphics"))
            {
                var textureFilePathParts = textureFilePath.Split('\\');
                var fileName = textureFilePathParts[textureFilePathParts.Length - 1];
                var assetName = fileName.Substring(0, fileName.Length - 4); // remove ".xnb"
                TextureAssets[assetName] = Content.Load<Texture2D>(@"Graphics\" + assetName);
            }

            HudFont = Content.Load<SpriteFont>(@"Fonts\HudFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            PhysicsEngine.Update(this, gameTime);
            InputManager.Update();
            HUD.Update(this, gameTime, InputManager.TouchInputs, InputManager.TouchGestures);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            GraphicsEngine.DrawField(this);
            HUD.Draw(this);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void InitializeGraphics(HeadsUpDisplay hud)
        {
            var viewportSize = GraphicsUtility.GetViewportSize(GraphicsDevice);
            var fieldSize = viewportSize - new Vector2(0, hud.BackgroundHeight);

            GraphicsEngine = new GraphicsEngine(GraphicsDevice, viewportSize, fieldSize);
            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void InitializeInput()
        {
            var viewportSize = GraphicsUtility.GetViewportSize(GraphicsDevice);

            HUD = new HeadsUpDisplay(
                GraphicsDevice,
                viewportSize,
                HudFont,
                TextureAssets);
            HUD.InputControls["resetButton"].AddOnButtonPressedEventHandler(new GameEventHandler(ResetField), this);
            HUD.InputControls["fireButton"].AddOnButtonPressedEventHandler(new GameEventHandler(Fire), this);
            HUD.InputControls["playerButton"].AddOnButtonPressedEventHandler(new GameEventHandler(NextPlayer), this);

            InputManager = new InputManager();
            TouchPanel.EnabledGestures = GestureType.FreeDrag;
        }

        private void InitializeField(GraphicsEngine graphicsEngine)
        {
            var numPlayers = Constants.Game.NumPlayers;
            Tanks = new Tank[numPlayers];
            for (int i = 0; i < numPlayers; i++)
            {
                AddPlayer(i);
            }

            Terrain = new Terrain(
                Randomizer,
                GraphicsDevice,
                (int)graphicsEngine.FieldSize.X,
                (int)graphicsEngine.FieldSize.Y);

            graphicsEngine.AddDrawableObject(Terrain);
            PhysicsEngine = new PhysicsEngine(Terrain);

            for (int i = 0; i < numPlayers; i++)
            {
                graphicsEngine.AddDrawableObject(Tanks[i]);
                PhysicsEngine.AddPhysicsObject(Tanks[i]);
            }

            ResetField(this);
        }

        private void AddPlayer(int i)
        {
            Tanks[i] = new Tank(
                i.ToString(),
                GraphicsDevice,
                TextureAssets,
                Color.Black);
        }

        private static void ResetField(ScorchGame game)
        {
            foreach (var tank in game.Tanks)
            {
                tank.Power = Constants.HUD.InitialPower;
            }

            game.Terrain.Regenerate(game.Tanks);
            game.HUD.SetCurrentPlayer(game.CurrentPlayerTank);
        }

        private static void NextPlayer(ScorchGame game)
        {
            game.CurrentPlayerIndex++;
            if (game.CurrentPlayerIndex >= game.Tanks.Length)
            {
                game.CurrentPlayerIndex = 0;
            }

            game.HUD.SetCurrentPlayer(game.CurrentPlayerTank);
        }

        private static void Fire(ScorchGame game)
        {
            var velocity = game.CurrentPlayerTank.Power * Constants.Physics.PlayerPowerVelocityFactor * new Vector2(
                (float)Math.Cos(game.CurrentPlayerTank.BarrelAngleInRadians),
                (float)Math.Sin(game.CurrentPlayerTank.BarrelAngleInRadians));

            var projectile = new Projectile(
                "projectile" + game.ProjectileId++,
                game.TextureAssets,
                game.CurrentPlayerTank.BarrelEndPosition,
                velocity);

            game.GraphicsEngine.AddDrawableObject(projectile);
            game.PhysicsEngine.AddPhysicsObject(projectile);
        }
    }
}
