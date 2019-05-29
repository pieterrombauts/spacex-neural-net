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
		Button btnPlayer;
		Button btnNeural;

		Texture2D boosterTexture;
		BoosterList boosterList;
		SpriteFont arial;
		Texture2D backgroundTexture;
		Texture2D landingTexture;
		static Vector2 landingPosition;
		bool generationComplete;
        float mainRocketThrust;
        float directionalThrust;
        float gravity;
		double angularAcceleration;
		float fuelConsumption;
		float monoConsumption;

		object[] initParams;
		int populationSize;
		int eliteIndividuals;
		int tournamentSize;
		int numNewIndividuals;
		int numEliteChildren;
		int[] crossover1Section;
		int[] crossover2Section;
		float crossoverRate;
		float mutationRate;

		int generation = 1;
		int currentlyFlying;
		int successes = 0;
		float bestGameScoreLastGen = -9999;
		float highestGameScore = -9999;

		enum GameState
		{
			MainMenu,
			PlayerControlled,
			NeuralNet,
		}
		GameState currentGameState = GameState.MainMenu;

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
			//initParams = new object[] { new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), new Vector2(0f, 0f), 0f, 1f, 4f, 10f };

			initParams = new object[] { new Vector2(graphics.PreferredBackBufferWidth / 3, graphics.PreferredBackBufferHeight / 3), new Vector2(0f, 0f), 0f, 10f, 4f, 10f };
			boosterList = new BoosterList();
			landingPosition = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight - 200);
			mainRocketThrust = 15f;
			directionalThrust = 5f;
			angularAcceleration = 0f;
			gravity = 10f;
			fuelConsumption = -0.8f;
			monoConsumption = -0.2f;
			generationComplete = false;

			populationSize = 500;
			eliteIndividuals = 2;
			tournamentSize = 3;
			numNewIndividuals = 20;
			numEliteChildren = 10;
			crossoverRate = 0.6f;
			crossover1Section = new int[] { 4, 7, 0, 7 };
			crossover2Section = new int[] { 4, 7, 0, 2 };
			mutationRate = 0.6f;

			base.Initialize();
        }

		private void Reset()
		{
			
			if (currentGameState == GameState.NeuralNet)
			{
				currentlyFlying = populationSize;
				boosterList = ANN.BreedNewGeneration(boosterList, initParams, eliteIndividuals, tournamentSize, crossoverRate, crossover1Section, crossover2Section, mutationRate, numNewIndividuals, numEliteChildren);
			}
			else if (currentGameState == GameState.PlayerControlled)
			{
				boosterList.Clear();
				boosterList.AddBooster(Content.Load<Texture2D>("booster"), (Vector2)initParams[0], (Vector2)initParams[1], (float)initParams[2], (float)initParams[3], (float)initParams[4], (float)initParams[5]);
			}
			generationComplete = false;
		}

		public static Vector2 GetLandingPosition() { return landingPosition; }

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

			IsMouseVisible = true;

			btnPlayer = new Button(Content.Load<Texture2D>("Player Control Button"), graphics.GraphicsDevice);
			btnPlayer.SetPosition(new Vector2(820, 510));
			btnNeural = new Button(Content.Load<Texture2D>("Neural Network Button"), graphics.GraphicsDevice);
			btnNeural.SetPosition(new Vector2(1000, 510));
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
			float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
	
            // TODO: Add your update logic here
            var kstate = Keyboard.GetState();
			var mstate = Mouse.GetState();

			if (currentGameState != GameState.MainMenu)
			{
				if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.OemTilde)) currentGameState = GameState.MainMenu;

				generationComplete = true;
				foreach (Booster b in boosterList)
				{
					if (b.GetGameScore() == 0)
					{
						generationComplete = false;
						double[] outputs = ANN.CalculateOutputs(b, ANN.CalculateHiddenValues(b));

						if ((((currentGameState == GameState.NeuralNet) && (outputs[0] >= 0.5)) || ((currentGameState == GameState.PlayerControlled) && kstate.IsKeyDown(Keys.Up))) && b.GetFuel() > 0)
						{
							b.ChangeVelocity(new Vector2((float)(mainRocketThrust * Math.Sin(b.GetBoosterRotation()) * b.GetBoosterMass() * deltaTime), (float)(gravity * b.GetBoosterMass() - (mainRocketThrust * Math.Cos(b.GetBoosterRotation()) * b.GetBoosterMass())) * deltaTime));
							b.ChangeFuel(fuelConsumption * deltaTime);
						}
						else if ((b.GetBoosterPosition().Y + (b.GetBoosterTexture().Height / 2)) < landingPosition.Y)
						{
							b.ChangeVelocity(new Vector2(0f, gravity * b.GetBoosterMass() * deltaTime));
						}

						if ((((currentGameState == GameState.NeuralNet) && (outputs[1] >= 0.5)) || ((currentGameState == GameState.PlayerControlled) && kstate.IsKeyDown(Keys.Left)))
							&& b.GetMonopropellant() > 0 && ((b.GetBoosterPosition().Y + (b.GetBoosterTexture().Height / 2)) < landingPosition.Y))
						{
							angularAcceleration = (1 / 2 * gravity * b.GetBoosterMass() * Math.Sin(b.GetBoosterRotation()) - (directionalThrust * b.GetBoosterMass()));
							angularAcceleration = angularAcceleration / (0.66 * b.GetBoosterMass() * 10);
							b.ChangeAngVelocity(angularAcceleration * deltaTime);
							b.ChangeMono(monoConsumption * deltaTime);
						}
						else if ((((currentGameState == GameState.NeuralNet) && (outputs[2] >= 0.5)) || ((currentGameState == GameState.PlayerControlled) && kstate.IsKeyDown(Keys.Right)))
								&& b.GetMonopropellant() > 0 && ((b.GetBoosterPosition().Y + (b.GetBoosterTexture().Height / 2)) < landingPosition.Y))
						{
							angularAcceleration = (1 / 2 * gravity * b.GetBoosterMass() * Math.Sin(b.GetBoosterRotation()) + (directionalThrust * b.GetBoosterMass()));
							angularAcceleration = angularAcceleration / (0.66 * b.GetBoosterMass() * 10);
							b.ChangeAngVelocity(angularAcceleration * deltaTime);
							b.ChangeMono(monoConsumption * deltaTime);
						}
						else
						{
							angularAcceleration = (1 / 2 * gravity * b.GetBoosterMass() * Math.Sin(b.GetBoosterRotation()));
							angularAcceleration = angularAcceleration / (0.66 * b.GetBoosterMass() * 10);
							b.ChangeAngVelocityMult((float)0.99);
							b.ChangeAngVelocity(angularAcceleration * deltaTime);
						}

						if (b.GetFuel() < 0) b.ChangeFuel(Math.Abs(b.GetFuel()));
						if (b.GetMonopropellant() < 0) b.ChangeMono(Math.Abs(b.GetMonopropellant()));

						b.ChangeRotation(b.GetAngVelocity() * deltaTime);
						b.ChangePosition(new Vector2(b.GetBoosterVelocity().X * deltaTime, b.GetBoosterVelocity().Y * deltaTime));


						if ((b.GetBoosterPosition().Y + (b.GetBoosterTexture().Height / 2)) >= landingPosition.Y)
						{
							b.SetPositionY(landingPosition.Y - (b.GetBoosterTexture().Height / 2));

							if ((b.GetBoosterVelocity().Y < 30f) && (b.GetBoosterVelocity().X < 30f)
								&& (b.GetBoosterPosition().X > (landingPosition.X - landingTexture.Width / 2 + 20))
								&& (b.GetBoosterPosition().X < (landingPosition.X + landingTexture.Width / 2 - 20))
								&& Math.Abs(b.GetBoosterRotation()) < 0.1f)
							{
								b.ChangeTint(Color.LightGreen);
							}

							b.ChangeGameScore(-Math.Abs(b.GetBoosterPosition().X - landingPosition.X));
							b.ChangeGameScore(-b.GetBoosterVelocity().Y);
							b.ChangeGameScore(-Math.Abs(b.GetBoosterVelocity().X));
							b.ChangeGameScore(-Math.Abs((float)b.GetBoosterRotation()));
							b.ChangeGameScore(-((float)initParams[5] - b.GetFuel()));
							b.ChangeGameScore(-((float)initParams[4] - b.GetMonopropellant()));
							b.SetGameScore(1000 / Math.Abs(b.GetGameScore()));
							b.SetVelocityX(0f);
						}
					}
				}

				if (generationComplete == true && kstate.IsKeyDown(Keys.Enter))
				{
					Reset();
				}
				else if (generationComplete == true)
				{
					successes = 0;
					bestGameScoreLastGen = -9999;
					foreach (Booster b in boosterList)
					{
						if (b.GetGameScore() > 0) { successes++; }
						if (b.GetGameScore() > bestGameScoreLastGen) { bestGameScoreLastGen = b.GetGameScore(); }
						if (b.GetGameScore() > highestGameScore) { highestGameScore = b.GetGameScore(); }
					}
					if (currentGameState == GameState.NeuralNet)
					{
						generation++;
						Reset();
					}
				}
			}
			else
			{
				if (btnPlayer.isClicked)
				{
					currentGameState = GameState.PlayerControlled;
					boosterList.Clear();
					boosterList.AddBooster(Content.Load<Texture2D>("booster"), (Vector2)initParams[0], (Vector2)initParams[1], (float)initParams[2], (float)initParams[3], (float)initParams[4], (float)initParams[5]);
				}
				btnPlayer.Update(mstate);
				if (btnNeural.isClicked)
				{
					currentGameState = GameState.NeuralNet;
					boosterList.Clear();
					for (int i = 0; i < populationSize; i++)
					{
						boosterList.AddBooster(Content.Load<Texture2D>("booster"), (Vector2)initParams[0], (Vector2)initParams[1], (float)initParams[2], (float)initParams[3], (float)initParams[4], (float)initParams[5]);
					}
				}
				btnNeural.Update(mstate);
				if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
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

			// TODO: Add your drawing code here

			spriteBatch.Begin(SpriteSortMode.BackToFront, null);
			spriteBatch.Draw(backgroundTexture, new Vector2(0, 0), null, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
			switch (currentGameState)
			{
				case GameState.MainMenu:
					btnPlayer.Draw(spriteBatch);
					btnNeural.Draw(spriteBatch);
					break;
				case GameState.NeuralNet:
					spriteBatch.DrawString(arial, "Generation: " + generation, new Vector2(50, 30), Color.Black);
					spriteBatch.DrawString(arial, "Successes: " + successes + "/" + populationSize, new Vector2(50, 50), Color.Black);
					spriteBatch.DrawString(arial, "Best Score in Last Gen: " + bestGameScoreLastGen, new Vector2(50, 70), Color.Black);
					spriteBatch.DrawString(arial, "Highest Overall Score: " + highestGameScore, new Vector2(50, 90), Color.Black);
					spriteBatch.DrawString(arial, "Generation Complete? " + generationComplete, new Vector2(50, 110), Color.Black);
					currentlyFlying = 0;
					foreach (Booster b in boosterList)
					{
						if (b.GetGameScore() == 0) currentlyFlying++;
					}
					spriteBatch.DrawString(arial, "Currently Flying: " + currentlyFlying + "/" + populationSize, new Vector2(50, 130), Color.Black);
					spriteBatch.Draw(landingTexture, landingPosition, null, Color.White, 0f, new Vector2(landingTexture.Width / 2, landingTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.7f);
					boosterList.Draw(spriteBatch);
					break;
				case GameState.PlayerControlled:
					spriteBatch.Draw(landingTexture, landingPosition, null, Color.White, 0f, new Vector2(landingTexture.Width / 2, landingTexture.Height / 2), Vector2.One, SpriteEffects.None, 0.7f);
					boosterList.Draw(spriteBatch);
					foreach (Booster b in boosterList)
					{
						spriteBatch.DrawString(arial, "Distance from target: " + Math.Abs(b.GetBoosterPosition().X - landingPosition.X), new Vector2(500, 30), Color.Black);
						spriteBatch.DrawString(arial, "Vel Y: " + b.GetBoosterVelocity().Y, new Vector2(500, 50), Color.Black);
						spriteBatch.DrawString(arial, "Vel X: " + b.GetBoosterVelocity().X, new Vector2(500, 70), Color.Black);
						spriteBatch.DrawString(arial, "Rotation: " + b.GetBoosterRotation(), new Vector2(500, 90), Color.Black);
						spriteBatch.DrawString(arial, "Fuel: " + ((float) initParams[5] - b.GetFuel()), new Vector2(500, 110), Color.Black);
						spriteBatch.DrawString(arial, "Mono: " + ((float) initParams[4] - b.GetMonopropellant()), new Vector2(500, 130), Color.Black);
						spriteBatch.DrawString(arial, "Score: " + b.GetGameScore(), new Vector2(500, 150), Color.Black);
					}
					break;
			}
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
