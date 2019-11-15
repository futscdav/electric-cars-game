using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public static class LevelManager {

	public static int level = 4; // default for debugging purposes
	public static LevelProperties properties;

	//filename with persistent level data (user "saves")
	private static string filename = "/levels.dat";
	private static int levelcount = -1;
	private static List<LevelData> levelData;

	public static float scoreSum = -1f;

	//Enumerates level in the game
	public static int EnumerateLevels() {
		if (levelcount > 0) {
			return levelcount;
		}

		string levelfiles = Resources.Load<TextAsset>("Levels/levelcount").text;
		//careful about exceptions
		levelcount = int.Parse(levelfiles);
		return levelcount;
	}

	public static List<LevelData> GetLevelData() {
		if (levelData == null) {
			LoadLevelData();
		}
		return levelData;
	}

	public static LevelData GetLevelData(int index) {
		List<LevelData> data = GetLevelData();
		if (data.Count < index-1) {
			return null;
		}
		return data[index-1];
	}

	//Loads serialized list of level data
	private static void LoadLevelData() {
		int levels = EnumerateLevels();

		if (File.Exists(Application.persistentDataPath + filename)) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + filename, FileMode.Open);
			List<LevelData> data = (List<LevelData>)bf.Deserialize(file);
			file.Close();

			levelData = data;

		} else {
			levelData = new List<LevelData>();
		}
		
		//Add more stuff if necessary
		if (levelData.Count < levels) {
			for (int i = levelData.Count; i < levels; ++i) {
				levelData.Add(new LevelData());
			}
		}

		//scoresum
		scoreSum = 0;
		foreach (LevelData data in levelData) {
			if (data.complete)
				scoreSum += data.score;
		}
		
	}

	//levels are numbered 1 - inf
	public static void SaveLevel(LevelData data, int index = -1) {
		if (index == -1) {
			index = level;
		}
		List<LevelData> alldata = GetLevelData();
		float oldscore = alldata[index - 1].score;
		alldata[index-1] = data;
		if (oldscore > alldata[index - 1].score) {
			alldata[index - 1].score = oldscore;
		}
		SaveLevelData();
		scoreSum -= oldscore;
		scoreSum += alldata[index - 1].score;
	}

	public static void SaveLevelData() {
		List<LevelData> data = GetLevelData();

		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + filename);

		bf.Serialize(file, data);
		file.Close();
	}

}

[Serializable()]
public class LevelData {
	public bool complete;
	public float score;
}

