using MoonSharp.Interpreter;
using Unity.Mathematics;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("Module for generating random numbers and more")]
    public class RandomProxy : ProxyBase
    {
        private Random rand;

        [MoonSharpHidden]
        public RandomProxy() 
        {
            rand.InitState();
        }

        public override string Name => "random";

        [LuaHelpInfo("Sets the seed of the random generator")]
        public void set_seed(uint seed)
        {
            rand = new Random(seed);
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
    }
}
