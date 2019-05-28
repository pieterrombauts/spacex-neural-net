using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace SpaceXNeuralNetwork
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
		Texture2D boosterTexture;
		BoosterList boosterList;
		SpriteFont arial;
		Texture2D backgroundTexture;
		Texture2D landingTexture;
		Vector2 landingPosition;
		Boolean generationComplete;
        float mainRocketThrust;
        float directionalThrust;
        float gravity;
		double angularAcceleration;

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
        
        public Game1()
        {
			graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1920,
				PreferredBackBufferHeight = 1080
			};
			Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
			// TODO: Add your initialization logic here
			boosterList = new BoosterList();
			for(int i = 0; i < 30; i++)
			{
				boosterList.AddBooster(Content.Load<Texture2D>("booster"), new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), new Vector2(0f, 0f), 0f, 1f, 4f, 10f);
			}
			landingPosition = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight - 200);
            mainRocketThrust = 0.07f;
            directionalThrust = 0.01f;
			angularAcceleration = 0f;
            gravity = 0.05f;
			generationComplete = false;

			base.Initialize();
        }

		private void Reset()
		{
			generationComplete = false;
			boosterList.Clear();
			for (int i = 0; i < 30; i++)
			{
				boosterList.AddBooster(Content.Load<Texture2D>("booster"), new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), new Vector2(0f, 0f), 0f, 1f, 4f, 10f);
			}
		}

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			arial = Content.Load<SpriteFont>("fuels");
			backgroundTexture = Content.Load<Texture2D>("ocean background");
			boosterTexture = Content.Load<Texture2D>("booster");
			landingTexture = Content.Load<Texture2D>("landing pad");
		}

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            var kstate = Keyboard.GetState();

			generationComplete = true;
			foreach (Booster b in boosterList)
			{
				if (b.GetGameScore() == 0)
				{
					generationComplete = false;
					if (kstate.IsKeyDown(Keys.Up) && b.GetFuel() > 0)
					{
						b.ChangeVelocity(new Vector2((float)(mainRocketThrust * Math.Sin(b.GetBoosterRotation()) * b.GetBoosterMass()), (float)(gravity - (mainRocketThrust * Math.Cos(b.GetBoosterRotation()) * b.GetBoosterMass()))));
						b.ChangeFuel(-0.02f);
					}
					else if ((b.GetBoosterPosition().Y + (b.GetBoosterTexture().Height / 2)) < landingPosition.Y)
					{
						b.ChangeVelocity(new Vector2(0f, gravity));
					}

					if (kstate.IsKeyDown(Keys.Left) && b.GetMonopropellant() > 0 && ((b.GetBoosterPosition().Y + (b.GetBoosterTexture().Height / 2)) < landingPosition.Y))
					{
						angularAcceleration = (1 / 2 * 10 * 10 * Math.Sin(b.GetBoosterRotation()) - (directionalThrust));
						angularAcceleration = angularAcceleration / (0.66 * 10 * 10);
						b.ChangeAngVelocity(angularAcceleration);
						b.ChangeMono(-0.01f);
					}
					else if (kstate.IsKeyDown(Keys.Right) && b.GetMonopropellant() > 0 && ((b.GetBoosterPosition().Y + (b.GetBoosterTexture().Height / 2)) < landingPosition.Y))
					{
						angularAcceleration = (1 / 2 * 10 * 10 * Math.Sin(b.GetBoosterRotation()) + (directionalThrust));
						angularAcceleration = angularAcceleration / (0.66 * 10 * 10);
						b.ChangeAngVelocity(angularAcceleration);
						b.ChangeMono(-0.01f);
					}
					else
					{
						angularAcceleration = (1 / 2 * 10 * 10 * Math.Sin(b.GetBoosterRotation()));
						angularAcceleration = angularAcceleration / (0.66 * 10 * 10);
						b.ChangeAngVelocityMult((float)0.99);
						b.ChangeAngVelocity(angularAcceleration);
					}

					if (b.GetFuel() < 0) b.ChangeFuel(Math.Abs(b.GetFuel()));
					if (b.GetMonopropellant() < 0) b.ChangeMono(Math.Abs(b.GetMonopropellant()));

					b.ChangeRotation(b.GetAngVelocity());
					b.ChangePosition(new Vector2(b.GetBoosterVelocity().X, b.GetBoosterVelocity().Y));


					if ((b.GetBoosterPosition().Y + (b.GetBoosterTexture().Height / 2)) >= landingPosition.Y)
					{
						b.SetPositionY(landingPosition.Y - (b.GetBoosterTexture().Height / 2));
						b.SetVelocityX(0f);
						b.SetRotation(0f);

						if ((b.GetBoosterVelocity().Y < 0.5f) && (b.GetBoosterVelocity().X < 0.2f)
							&& (b.GetBoosterPosition().X > (landingPosition.X - landingTexture.Width / 2 + 20))
							&& (b.GetBoosterPosition().X < (landingPosition.X + landingTexture.Width / 2 - 20))
							&& Math.Abs(b.GetBoosterRotation()) < 0.05)
						{
							b.ChangeGameScore(100);
							b.ChangeGameScore(-Math.Abs(b.GetBoosterPosition().X - landingPosition.X));
							b.ChangeGameScore(10 * b.GetFuel());
							b.ChangeGameScore(10 * b.GetMonopropellant());
							b.ChangeTint(Color.LightGreen);
						}
						else
						{
							b.ChangeGameScore(-Math.Abs(b.GetBoosterPosition().X - landingPosition.X));
							b.ChangeGameScore(-b.GetBoosterVelocity().X);
							b.ChangeGameScore(-b.GetBoosterVelocity().Y);
							b.ChangeGameScore((float)-b.GetBoosterRotation());
							b.ChangeTint(Color.Salmon);
						}
					}
				}
			}

			if (generationComplete = true && kstate.IsKeyDown(Keys.Enter))
			{
				Reset();
			}

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
			var kstate = Keyboard.GetState();
			int startCoord = 600;

			// TODO: Add your drawing code here
			spriteBatch.Begin();
			spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
			

			foreach(Booster b in boosterList)
			{
				spriteBatch.DrawString(arial, "VelocityX: " + b.GetBoosterVelocity().X, new Vector2(startCoord, 150), Color.Black);
				spriteBatch.DrawString(arial, "VelocityY: " + b.GetBoosterVelocity().Y, new Vector2(startCoord, 170), Color.Black);
				spriteBatch.DrawString(arial, "Rotation: " + b.GetBoosterRotation(), new Vector2(startCoord, 190), Color.Black);
				spriteBatch.DrawString(arial, "Fuel: " + b.GetFuel(), new Vector2(startCoord, 210), Color.Black);
				spriteBatch.DrawString(arial, "Monopropellant: " + b.GetMonopropellant(), new Vector2(startCoord, 230), Color.Black);
				spriteBatch.DrawString(arial, "Score: " + b.GetGameScore(), new Vector2(startCoord, 250), Color.Black);
				startCoord += 400;
			}
			
			if (kstate.IsKeyDown(Keys.Up)) { spriteBatch.DrawString(arial, "Up", new Vector2(1800, 100), Color.Black); }
			if (kstate.IsKeyDown(Keys.Left)) { spriteBatch.DrawString(arial, "Left", new Vector2(1800, 120), Color.Black); }
			if (kstate.IsKeyDown(Keys.Right)) { spriteBatch.DrawString(arial, "Right", new Vector2(1800, 140), Color.Black); }

			spriteBatch.Draw(landingTexture, landingPosition, null, Color.White, 0f, new Vector2(landingTexture.Width / 2, landingTexture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
			boosterList.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
