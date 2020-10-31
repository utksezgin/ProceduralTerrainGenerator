using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
	public enum DrawMode { NoiseMap, ColorMap, Mesh, MoistureMap, HeatMap }
	public DrawMode drawMode;

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public float heightMultiplier;
	public AnimationCurve heightCurve;

	public int octaves;
	[Range(0, 1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public bool autoUpdate;
	public bool isInfinite;

	public TerrainType[] biomes;

	public void GenerateMap()
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
		Color[] colorMap = new Color[mapWidth * mapHeight];
		Color[] moistureMap = new Color[mapWidth * mapHeight];
		Color[] heatColorMap= new Color[mapWidth * mapHeight];
		float[,] precipitationMap = new float[mapWidth, mapHeight];
		float[,] heatMap = new float[mapWidth, mapHeight];

		for (int i = 0; i < mapHeight; ++i)
		{
			for (int j = 0; j < mapWidth; ++j){
				float currentHeight = noiseMap[j, i];


				precipitationMap[j, i] = noiseMap[j, i];				
				heatMap[j, i] = 1 - 2 * (Mathf.Abs(mapHeight / 2 - i) / (float)mapHeight)  - Mathf.Pow((precipitationMap[j,i]), 10f);
				if (heatMap[j, i] < 0)
					heatMap[j, i] = 0;

				TerrainType.Temperature temperature;
				TerrainType.Precipitation precipitation;
				if (precipitationMap[j, i] < 0.5)
					precipitation = TerrainType.Precipitation.wettest;
				else if(precipitationMap[j, i] < 0.66)
					precipitation = TerrainType.Precipitation.wet;
				else if(precipitationMap[j, i] < 0.80)
					precipitation = TerrainType.Precipitation.dry;
				else
					precipitation = TerrainType.Precipitation.dryest;

				switch (precipitation)
				{
					case TerrainType.Precipitation.dryest:
						moistureMap[i * mapWidth + j] = Color.red;
						break;
					case TerrainType.Precipitation.dry:
						moistureMap[i * mapWidth + j] = Color.yellow;
						break;
					case TerrainType.Precipitation.wet:
						moistureMap[i * mapWidth + j] = Color.blue;
						break;
					case TerrainType.Precipitation.wettest:
						moistureMap[i * mapWidth + j] = Color.cyan;
						break;
				}
				if (heatMap[j, i] < 0.1)
					temperature = TerrainType.Temperature.coldest;
				else if (heatMap[j, i] < 0.66)
					temperature = TerrainType.Temperature.cold;
				else if (heatMap[j, i] < 0.90)
					temperature = TerrainType.Temperature.hot;
				else
					temperature = TerrainType.Temperature.hottest;


				switch (temperature)
				{
					case TerrainType.Temperature.coldest:
						heatColorMap[i * mapWidth + j] = Color.white;
						break;
					case TerrainType.Temperature.cold:
						heatColorMap[i * mapWidth + j] = Color.green;
						break;
					case TerrainType.Temperature.hot:
						heatColorMap[i * mapWidth + j] = Color.yellow;
						break;
					case TerrainType.Temperature.hottest:
						heatColorMap[i * mapWidth + j] = Color.red;
						break;
				}


				for (int k = 0; k < biomes.Length; ++k)
				{
					if (currentHeight <= biomes[1].height) //Water
					{
						//if (temperature != TerrainType.Temperature.coldest && temperature != TerrainType.Temperature.cold)
							colorMap[i * mapWidth + j] = biomes[1].color;
						/*else
							colorMap[i * mapWidth + j] = Color.cyan;*/

						break;
					}
					else
					{
						if (k != 1 && biomes[k].temperature == temperature && biomes[k].precipitation == precipitation)
						{
							colorMap[i * mapWidth + j] = biomes[k].color;
							break;
						}
					}
				}
			}
		}
		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap)
			display.DrawTexture(TextureGenerator.HeightTexture(noiseMap));
		else if (drawMode == DrawMode.ColorMap)
			display.DrawTexture(TextureGenerator.coloredTexture(colorMap, mapWidth, mapHeight));
		else if (drawMode == DrawMode.Mesh)
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve), TextureGenerator.coloredTexture(colorMap, mapWidth, mapHeight));
		else if(drawMode == DrawMode.MoistureMap)
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve), TextureGenerator.coloredTexture(moistureMap, mapWidth, mapHeight));
		else if (drawMode == DrawMode.HeatMap)
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve), TextureGenerator.coloredTexture(heatColorMap, mapWidth, mapHeight));
	}

	void OnValidate()
	{
		if (mapWidth < 1)
		{
			mapWidth = 1;
		}
		if (mapHeight < 1)
		{
			mapHeight = 1;
		}
		if (lacunarity < 1)
		{
			lacunarity = 1;
		}
		if (octaves < 0)
		{
			octaves = 0;
		}
	}
}
[System.Serializable]
public struct TerrainType
{
	public enum Temperature { coldest, cold, hot, hottest};
	public enum Precipitation { wettest, wet, dry, dryest };

	public string name;
	public float height;
	public Temperature temperature;
	public Precipitation precipitation;
	public Color color;
}