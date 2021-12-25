using MoonSharp.Interpreter;
using Unity.Mathematics;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class RandomProxy : ProxyBase
    {

        private Random rand;

        [MoonSharpHidden]
        public RandomProxy() 
        {
            rand.InitState();
        }

        public override string Name => "random";

        public override string Description => "Module for generating random numbers and more";

        [LuaHelpInfo("Sets the seed of the random generator")]
        public void set_seed(uint seed)
        {
            rand = new Random(seed);
        }

        [LuaHelpInfo("returns a random number between 0 and 1")]
        public float number()
        {
            return rand.NextFloat();
        }

        [LuaHelpInfo("returns a random number between min and max")]
        public float number(float min, float max)
        {
            return rand.NextFloat(min, max);
        }

        [LuaHelpInfo("returns a random coordinate")]
        public Coordinate coord()
        {
            return new Coordinate(rand.NextFloat(-math.PI, math.PI), rand.NextFloat(-2 * math.PI, 2 * math.PI));
        }
    }
}
