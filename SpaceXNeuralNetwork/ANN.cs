using accord = Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace SpaceXNeuralNetwork
{
	class ANN
	{
		public static double[] CalculateHiddenValues(Booster b)
		{
			double[] hiddenLayer = accord.Matrix.Dot(b.GetWeights1(), b.GetInputs());
			for (int i = 0; i < accord.Matrix.Rows(hiddenLayer); i++)
			{
				hiddenLayer[i] = Booster.Sigmoid(hiddenLayer[i]);
			}
			return hiddenLayer;
		}

		public static double[] CalculateOutputs(Booster b, double[] hiddenLayer)
		{
			double[] outputs = accord.Matrix.Dot(b.GetWeights2(), hiddenLayer);
			for (int i = 0; i < accord.Matrix.Rows(outputs); i++)
			{
				outputs[i] = Booster.Sigmoid(outputs[i]);
			}
			return outputs;
		}

		public static BoosterList BreedNewGeneration(BoosterList currentGen, object[] initParams, int eliteIndividuals, float crossoverRate, int[] crossover1Section, int[] crossover2Section, float mutationRate, int numNewIndividuals, int numEliteChildren)
		{
			Random rnd = new Random(DateTime.Now.Millisecond);
			BoosterList newGeneration = new BoosterList();

			currentGen.Sort((a, b) => b.GetGameScore().CompareTo(a.GetGameScore()));
			for (int i = 0; i < eliteIndividuals; i++)
			{
				Booster elite = currentGen[i];
				Debug.Print("Elite individual " + (i + 1) + " landed with a position of " + elite.GetBoosterPosition().X + ", a y-velocity of " + elite.GetBoosterVelocity().Y +
							", a rotation of " + elite.GetBoosterRotation() + ", with " + elite.GetFuel() + " fuel left and " + elite.GetMonopropellant() + " mono left, with a score of " + elite.GetGameScore() +
							"and weights1 of " + elite.GetWeights1()[0, 0] + ", " + elite.GetWeights1()[0, 1] + ", " + elite.GetWeights1()[0, 2] + ", " + elite.GetWeights1()[0, 3] + ", " + elite.GetWeights1()[0, 4]);
				newGeneration.AddBooster(elite.GetBoosterTexture(), (Vector2)initParams[0], (Vector2)initParams[1], (float)initParams[2], (float)initParams[3], (float)initParams[4], (float)initParams[5], elite.GetWeights1(), elite.GetWeights2());
				newGeneration[i].ChangeTint(Color.Blue);
				newGeneration[i].ChangeLayerDepth(0f);
			}

			for (int i = 0; i < (numEliteChildren / eliteIndividuals); i++)
			{
				for (int j = 0; j < eliteIndividuals; j++)
				{
					newGeneration.AddRange(CreateChildren(rnd, currentGen, newGeneration[j], RouletteSelection(rnd, currentGen), initParams, crossoverRate, crossover1Section, crossover2Section, mutationRate));
				}
			}

			for (int i = eliteIndividuals; i < numEliteChildren; i++)
			{
				newGeneration[i].ChangeTint(Color.Purple);
				newGeneration[i].ChangeLayerDepth(0.1f);
			}

			while(newGeneration.Count() < (currentGen.Count() - numNewIndividuals))
			{
				newGeneration.AddRange(CreateChildren(rnd, currentGen, RouletteSelection(rnd, currentGen), RouletteSelection(rnd, currentGen), initParams, crossoverRate, crossover1Section, crossover2Section, mutationRate));
			}

			while(newGeneration.Count() < currentGen.Count())
			{
				newGeneration.AddBooster(newGeneration[0].GetBoosterTexture(), (Vector2)initParams[0], (Vector2)initParams[1], (float)initParams[2], (float)initParams[3], (float)initParams[4], (float)initParams[5]);
				newGeneration[newGeneration.Count() - 1].ChangeTint(Color.Yellow);
				newGeneration[newGeneration.Count() - 1].ChangeLayerDepth(0.2f);
			}

			return newGeneration;
		}

		private static BoosterList CreateChildren(Random rnd, BoosterList currentGen, Booster parent1, Booster parent2, object[] initParams, float crossoverRate, int[] crossover1Section, int[] crossover2Section, float mutationRate)
		{
			BoosterList children = new BoosterList();
			double[,] child1Weights1 = accord.Matrix.Copy(parent1.GetWeights1());
			double[,] child1Weights2 = accord.Matrix.Copy(parent1.GetWeights2());
			double[,] child2Weights1 = accord.Matrix.Copy(parent2.GetWeights1());
			double[,] child2Weights2 = accord.Matrix.Copy(parent2.GetWeights2());
			double[,] temp;

			if (rnd.NextDouble() <= crossoverRate)
			{
				temp = accord.Matrix.Copy(child1Weights1);
				for (int i = crossover1Section[0]; i <= crossover1Section[1]; i++)
				{
					for (int j = crossover1Section[2]; j <= crossover1Section[3]; j++)
					{
						child1Weights1[i, j] = child2Weights1[i, j];
						child2Weights1[i, j] = temp[i, j];
					}
				}

				temp = accord.Matrix.Copy(child1Weights2);
				for (int i = 0; i < accord.Matrix.Rows(child1Weights2); i++)
				{
					for (int j = 0; j < accord.Matrix.Columns(child1Weights2); j++)
					{
						child1Weights2[i, j] = child2Weights2[i, j];
						child2Weights2[i, j] = temp[i, j];
					}
				}
			}

			child1Weights1 = MutateMatrix(rnd, child1Weights1, mutationRate);
			child1Weights2 = MutateMatrix(rnd, child1Weights2, mutationRate);
			child2Weights1 = MutateMatrix(rnd, child2Weights1, mutationRate);
			child2Weights2 = MutateMatrix(rnd, child2Weights2, mutationRate);

			children.AddBooster(parent1.GetBoosterTexture(), (Vector2)initParams[0], (Vector2)initParams[1], (float)initParams[2], (float)initParams[3], (float)initParams[4], (float)initParams[5], child1Weights1, child1Weights2);
			children.AddBooster(parent2.GetBoosterTexture(), (Vector2)initParams[0], (Vector2)initParams[1], (float)initParams[2], (float)initParams[3], (float)initParams[4], (float)initParams[5], child2Weights1, child2Weights2);

			return children;
		}

		private static double[,] MutateMatrix(Random rnd, double[,] matrix, float mutationRate)
		{
			int numElementsToMutate = (int) Math.Floor(mutationRate * accord.Matrix.GetNumberOfElements(matrix));

			for (int i = 0; i < numElementsToMutate; i++)
			{
				matrix[rnd.Next(0, accord.Matrix.Rows(matrix)), rnd.Next(0, accord.Matrix.Columns(matrix))] = rnd.NextDouble() * 2 + (-1);
			}
			return matrix;
		}

		private static Booster RouletteSelection(Random rnd, BoosterList currentGen)
		{
			float sumOfFitness = 0;
			int fixedPoint;
			float partialSum = 0;

			foreach (Booster b in currentGen)
			{
				sumOfFitness += b.GetGameScore();
			}

			fixedPoint = rnd.Next((int)sumOfFitness);

			foreach (Booster b in currentGen)
			{
				partialSum += b.GetGameScore();
				if (partialSum > fixedPoint) return b;
			}

			return currentGen[0];
		}

		private static Booster TournamentSelection(Random rnd, BoosterList currentGen, int tournamentSize)
		{ 
			BoosterList selection = new BoosterList();

			for (int i = 0; i < tournamentSize; i++)
			{
				int index = rnd.Next(0, currentGen.Count());
				selection.Add(currentGen[index]);
			}

			selection.Sort((a, b) => b.GetGameScore().CompareTo(a.GetGameScore()));
			return selection[0];
		}
	}
}
