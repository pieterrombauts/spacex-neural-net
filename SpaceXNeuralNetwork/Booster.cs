using MathNet.Numerics.LinearAlgebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceXNeuralNetwork
{
	class Booster
	{
		private Matrix<double> weights1;
		private Matrix<double> weights2;
		private Texture2D boosterTexture;
		private Color tint;
		private Vector2 boosterPosition;
		private Vector2 boosterVelocity;
		private double boosterRotation;
		private double angularVelocity;
		private float boosterMass;
		private float monopropellant;
		private float fuel;
		private float gameScore;

		public Booster(Texture2D boosterTexture, Vector2 boosterPosition, Vector2 boosterVelocity, double boosterRotation, float boosterMass, float monopropellant, float fuel)
		{
			this.boosterTexture = boosterTexture;
			this.tint = Color.White;
			this.boosterPosition = boosterPosition;
			this.boosterVelocity = boosterVelocity;
			this.boosterRotation = boosterRotation;
			this.boosterMass = boosterMass;
			this.monopropellant = monopropellant;
			this.fuel = fuel;
			angularVelocity = 0f;
			gameScore = 0f;
			weights1 = Matrix<double>.Build.Random(8, 7);
			weights2 = Matrix<double>.Build.Random(3, 8);
		}

		public void ChangeVelocity(Vector2 accelerationVector)
		{
			boosterVelocity.X += accelerationVector.X;
			boosterVelocity.Y += accelerationVector.Y;
		}

		public void SetVelocityX(float velX) { boosterVelocity.X = velX; }

		public void SetVelocityY(float velY) { boosterVelocity.Y = velY; }

		public void ChangePosition(Vector2 velocityVector)
		{
			boosterPosition.X += velocityVector.X;
			boosterPosition.Y += velocityVector.Y;
		}

		public void SetPositionX(float posX) { boosterPosition.X = posX; }

		public void SetPositionY(float posY) { boosterPosition.Y = posY; }

		public void ChangeAngVelocity(double angAcceleration) { angularVelocity += angAcceleration; }

		public void ChangeAngVelocityMult(float drag) { angularVelocity *= drag; }

		public void ChangeRotation(double angVelocity) { boosterRotation += angVelocity; }

		public void SetRotation(float rotation) { boosterRotation = rotation; }

		public void ChangeFuel(float fuelChange) { fuel += fuelChange; }

		public void ChangeMono(float monoChange) { monopropellant += monoChange; }

		public void ChangeGameScore(float gameScoreChange) { gameScore += gameScoreChange; }

		public void ChangeTint(Color tintChange) { tint = tintChange; }

		public Texture2D GetBoosterTexture() { return boosterTexture; }

		public Color GetTint() { return tint; }

		public Vector2 GetBoosterPosition() { return boosterPosition; }

		public Vector2 GetBoosterVelocity() { return boosterVelocity; }

		public double GetBoosterRotation() { return boosterRotation; }

		public double GetAngVelocity() { return angularVelocity; }

		public float GetBoosterMass() { return boosterMass; }

		public float GetFuel() { return fuel; }

		public float GetMonopropellant() { return monopropellant; }

		public float GetGameScore() { return gameScore; }

	}

	class BoosterList : List<Booster>
	{
		public void AddBooster(Texture2D boosterTexture, Vector2 boosterPosition, Vector2 boosterVelocity, double boosterRotation, float boosterMass, float monopropellant, float fuel)
		{
			this.Add(new Booster(boosterTexture, boosterPosition, boosterVelocity, boosterRotation, boosterMass, monopropellant, fuel));
		}

		public void Update()
		{
			
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (Booster b in this)
			{	
				spriteBatch.Draw(b.GetBoosterTexture(), b.GetBoosterPosition(), null, b.GetTint(), (float)b.GetBoosterRotation(), new Vector2(b.GetBoosterTexture().Width / 2, b.GetBoosterTexture().Height / 2), Vector2.One, SpriteEffects.None, 0f);
			}
		}
	}
}
