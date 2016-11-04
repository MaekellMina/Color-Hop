using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSVReader : MonoBehaviour
{
	[HideInInspector]
	public int i_colOfData = 26;   //THIS IS ALREADY CHANGED INSIDE THE FILE ITSELF. ITS TAKING THE NUM OF EVENT FOR T_GAME_DIFFICULTY.CSV

	public int i_level = 10;
	public List<float[]> dataList = new List<float[]>();            // sort by level
	public TextAsset levelObj;
	// Use this for initialization
	void Start()
	{
		string[] tmpSplit = levelObj.text.Split('\n');
		for (int i = 0; i < tmpSplit.Length - 1; i++)
		{
			string[] tmpSplit2 = tmpSplit[i].Split(',');
			if (i > 2)
			{
				dataList.Add(new float[i_colOfData]);
				for (int y = 0; y < tmpSplit2.Length; y++)
				{
					if (tmpSplit2[y] != "")
					{
						//dataList[ dataList.Count - 1 ][y] = System.Convert.ToSingle(tmpSplit2[y]);
						float.TryParse(tmpSplit2[y], out dataList[dataList.Count - 1][y]);
					}
				}
			}
			else if (i == 1)
			{
				i_colOfData = System.Convert.ToInt32(tmpSplit2[0]);
				i_level = System.Convert.ToInt32(tmpSplit2[1]);
			}
		}

		string tmp = "";
		for (int i = 0; i < dataList.Count; i++)
		{
			for (int y = 0; y < dataList[i].Length; y++)
			{
				tmp += dataList[i][y] + " ";
			}
			tmp = "";
		}
	}
	public float[] GetLevelData(int tmp_i)
	{
		if (dataList.Count == 0) Start();
		if (tmp_i > dataList.Count) tmp_i = dataList.Count - 1;
		return dataList[tmp_i];
	}

}
