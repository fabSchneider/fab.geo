using Fab.Lua.Core;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
	[LuaHelpInfo("Module for generating random numbers and more")]
	public class Random : LuaObject, ILuaObjectInitialize
	{
		private Unity.Mathematics.Random rand;

		private static uint defaultSeed;

		static Random()
		{
			DateTime now = DateTime.UtcNow;
			defaultSeed =
				(uint)now.Year * (uint)31557600 +
				(uint)now.Month * (uint)2629800 +
				(uint)now.Day * (uint)86400 +
				(uint)now.Hour * (uint)3600 +
				(uint)now.Minute * (uint)60 +
				(uint)now.Second;
		}

		public void Initialize()
		{
			rand = new Unity.Mathematics.Random(defaultSeed);
		}


		[LuaHelpInfo("Sets the seed of the random generator")]
		public void set_seed(uint seed)
		{
			rand = new Unity.Mathematics.Random(seed);
		}

		[LuaHelpInfo("returns a random number between 0 [inclusive] and 1 (exclusive)")]
		public float number()
		{
			return rand.NextFloat();
		}

		[LuaHelpInfo("returns a random number between min [inclusive] and max (exclusive)")]
		public float number(float min, float max)
		{
			return rand.NextFloat(min, max);
		}

		[LuaHelpInfo("returns a random whole number between min [inclusive] and max (exclusive)")]
		public float whole_number(int min, int max)
		{
			return rand.NextInt(min, max);
		}

		[LuaHelpInfo("returns a random coordinate")]
		public Coordinate coord()
		{
			return new Coordinate(rand.NextFloat(-math.PI, math.PI), rand.NextFloat(-2 * math.PI, 2 * math.PI));
		}

		[LuaHelpInfo("returns a color with a random hue")]
		public Color color(float saturation = 1f, float brightness = 1f)
		{
			return Color.HSVToRGB(rand.NextFloat(), saturation, brightness);
		}
	}
}
