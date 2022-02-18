using UnityEngine;
using System;
using Unity.Collections;

namespace Fab.Geo
{
	[Serializable]
	public struct CityData
	{
		public string country;
		public string name;
		public float lat;
		public float lng;
	}

	[Serializable]
	public class CityDataCollection
	{
		public static CityDataCollection Create(TextAsset dataAsset)
		{
			string json = dataAsset.text;
			CityDataCollection collection = JsonUtility.FromJson<CityDataCollection>(json);
			return collection;
		}
		private CityDataCollection() { }

		[SerializeField]
		private CityData[] cities;
		public CityData this[int index] => cities[index];

		public NativeArray<Coordinate> GetCityCoordinates(Allocator allocator)
		{
			int count = cities.Length;

			NativeArray<Coordinate> cityData = new NativeArray<Coordinate>(count, allocator);

			for (int i = 0; i < count; i++)
			{
				cityData[i] = new Coordinate(cities[i].lng, cities[i].lat);
			}

			return cityData;
		}
	}

}
