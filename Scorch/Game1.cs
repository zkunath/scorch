using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Scorch.DataModels;
using Scorch.Graphics;

namespace Scorch
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ScorchGame : Game
    {
        public delegate void GameEventHandler(ScorchGame game);
        
        GraphicsDeviceManager GraphicsDeviceManager;
        GraphicsEngine GraphicsEngine;
        HeadsUpDisplay HUD;
        Dictionary<string, Texture2D> TextureAssets;
        Random Randomizer;

        Terrain Terrain;
        Tank[] Tanks;

        SpriteFont HudFont;

        int CurrentPlayerIndex = 0;

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
            GraphicsEngine = new GraphicsEngine(GraphicsDevice);

            Tanks = new Tank[2];

            Tanks[0] = new Tank(
                "0",
                GraphicsDevice,
                TextureAssets["Tank"],
                TextureAssets["TankBarrel"],
                TextureAssets["PowerIndicator"],
                Color.Blue);
            GraphicsEngine.AddDrawableToField(Tanks[0]);

            Tanks[1] = new Tank(
                "1",
                GraphicsDevice,
                TextureAssets["Tank"],
                TextureAssets["TankBarrel"],
                TextureAssets["PowerIndicator"],
                Color.Red);
            GraphicsEngine.AddDrawableToField(Tanks[1]);

            Terrain = new Terrain(
                Randomizer,
                GraphicsDevice,
                (int)GraphicsEngine.FieldSize.X,
                (int)GraphicsEngine.FieldSize.Y);

            Terrain.Regenerate(Tanks);

            GraphicsEngine.AddDrawableToField(Terrain);

            HUD = new HeadsUpDisplay(GraphicsDevice, HudFont, TextureAssets["AimOverlay"]);
            HUD.InputControls["terrainButton"].AddOnButtonPressedEventHandler(new GameEventHandler(RegenerateTerrain), this);
            HUD.InputControls["playerButton"].AddOnButtonPressedEventHandler(new GameEventHandler(NextPlayer), this);

            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
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
            var gesture = default(GestureSample);

            var touchPanelState = TouchPanel.GetState();
            HUD.Update(gameTime, touchPanelState);

            while(TouchPanel.IsGestureAvailable)
            {
                gesture = TouchPanel.ReadGesture();
                switch (gesture.GestureType)
                {
                    case GestureType.Tap:
                        Terrain.Regenerate(Tanks);
                        break;
                    case GestureType.FreeDrag:
                        if (HUD.Mode == HudMode.Aim)
                        {
                            if (HUD.AimOverlayPosition == -Vector2.One)
                            {
                                //HUD.AimOverlayPosition = gesture.Position;
                                HUD.AimOverlayPosition = Tanks[CurrentPlayerIndex].Position + Tanks[CurrentPlayerIndex].ChildObjects["barrel"].Position;
                                Tanks[CurrentPlayerIndex].ChildObjects["powerIndicator"].Visible = true;
                            }
                            else
                            {
                                var aim = gesture.Position - HUD.AimOverlayPosition;
                                Tanks[CurrentPlayerIndex].SetAngleAndPowerByTouchGesture(
                                    aim,
                                    GraphicsEngine.CameraSize.Y / 32,
                                    GraphicsEngine.CameraSize.Y / 4);
                            }
                        }
                        break;
                    case GestureType.DragComplete:
                        if (HUD.Mode == HudMode.Aim)
                        {
                            HUD.AimOverlayPosition = -Vector2.One;
                            Tanks[CurrentPlayerIndex].ChildObjects["powerIndicator"].Visible = false;
                        }
                        break;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsEngine.DrawField();

            Tank currentPlayerTank = Tanks[CurrentPlayerIndex];

            HUD.Draw(
                currentPlayerTank.Id,
                currentPlayerTank.BarrelAngleInDegrees.ToString(),
                currentPlayerTank.Power.ToString());

            base.Draw(gameTime);
        }

        private static void RegenerateTerrain(ScorchGame game)
        {
            game.Terrain.Regenerate(game.Tanks);
        }

        private static void NextPlayer(ScorchGame game)
        {
            game.CurrentPlayerIndex++;
            if (game.CurrentPlayerIndex >= game.Tanks.Length)
            {
                game.CurrentPlayerIndex = 0;
            }
        }
    }
}
