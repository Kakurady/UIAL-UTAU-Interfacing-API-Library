using Microsoft.VisualStudio.TestTools.UnitTesting;
using zuoanqh.UIAL.Engine;

namespace zuoanqh.UIAL.Testing
{
  [TestClass]
  public class EngineInterfaceTest
  {
    [TestMethod]
    public void TestPB()
    {
      var param = new ResamplerParameter();
      string[] testCases = new string[] {
        "1E#14#1T18244B5N6T7K7r7z#2#7y7x7w7v7t7s7q7o7m7k7j7h7g7e7d7d7c#2#7e7k7s748H8X8q8+9U9q+A+W+q++/P/f/r/1/8//AA#3#ABABACADAEAFAGAHAIAJAKALAM#4#ALALAJAIAGAEACAA/9/6/3/0/x/u/s/p/n/m/l/k#2#/l/m/n/p/r/t/v/y/1/4/7//ACAFAIALAOARATAWAYAZAaAbAc#2#AbAaAYAXAVASAQANAKAHAEAA/9/6/3/0/x/u/s/q/o/m/l/k#2#/l/l/n/o/q/s/v/x/0/3/6/+ABAEAHALAOAQATAVAXAZAaAbAc#2#AbAZAYAWATARAOAMAJAGAEAB/+/8/6/4/2/0/z/y/x#5#/y/z/0/1",
        "AJAWAvBOBxCTCvDBDHDI#13#DGC3CaBxA/AI/S+g929Y9I9H#14#9V9y+Y/B/j/6",
        "84#38#85#2#8686878788#2#89898+#5#9A9O9m+F+o/K/m/5//",
        "AA#43#" };
      int[][] expected = new int[][]
      {
        new int[]{ -700, -700, -700, -700, -700, -700, -700, -700, -700, -700, -700, -700, -700, -700, -685, -644, -584, -511, -435, -365, -310, -277, -269, -269, -270, -271, -272, -273, -275, -276, -278, -280, -282, -284, -285, -287, -288, -290, -291, -291, -292, -292, -290, -284, -276, -264, -249, -233, -214, -194, -172, -150, -128, -106, -86, -66, -49, -33, -21, -11, -4, -1, 0, 0, 0, 1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 12, 12, 12, 11, 11, 9, 8, 6, 4, 2, 0, -3, -6, -9, -12, -15, -18, -20, -23, -25, -26, -27, -28, -28, -27, -26, -25, -23, -21, -19, -17, -14, -11, -8, -5, -1, 2, 5, 8, 11, 14, 17, 19, 22, 24, 25, 26, 27, 28, 28, 27, 26, 24, 23, 21, 18, 16, 13, 10, 7, 4, 0, -3, -6, -9, -12, -15, -18, -20, -22, -24, -26, -27, -28, -28, -27, -27, -25, -24, -22, -20, -17, -15, -12, -9, -6, -2, 1, 4, 7, 11, 14, 16, 19, 21, 23, 25, 26, 27, 28, 28, 27, 25, 24, 22, 19, 17, 14, 12, 9, 6, 4, 1, -2, -4, -6, -8, -10, -12, -13, -14, -15, -15, -15, -15, -15, -14, -13, -12, -11 },
        new int[]{ 9, 22, 47, 78, 113, 147, 175, 193, 199, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 198, 183, 154, 113, 63, 8, -46, -96, -138, -168, -184, -185, -185, -185, -185, -185, -185, -185, -185, -185, -185, -185, -185, -185, -185, -171, -142, -104, -63, -29, -6 },
        new int[]{ -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -200, -199, -199, -198, -198, -197, -197, -196, -196, -195, -195, -194, -194, -194, -194, -194, -192, -178, -154, -123, -88, -54, -26, -7, -1 },
        new int[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
      };
      for (int i = 0; i < testCases.Length; i++ )
      {
        param.PitchbendString = testCases[i];
        //this is necessary because the original algorithm is inconsistent. (possibly stored with higher resolution internally?)
        int[] decoded = CommonReferences.DecodePitchbends(param.PitchbendString);
        CollectionAssert.AreEqual(decoded, expected[i]);
        string recoded = CommonReferences.EncodePitchbends(decoded);
        Assert.AreEqual(recoded, CommonReferences.EncodePitchbends(CommonReferences.DecodePitchbends(recoded)));
      }
    }

    [TestMethod]
    public void TestPBEfficency()
    {
      var param = new ResamplerParameter() { PitchbendString = "1E#14#1T18244B5N6T7K7r7z#2#7y7x7w7v7t7s7q7o7m7k7j7h7g7e7d7d7c#2#7e7k7s748H8X8q8+9U9q+A+W+q++/P/f/r/1/8//AA#3#ABABACADAEAFAGAHAIAJAKALAM#4#ALALAJAIAGAEACAA/9/6/3/0/x/u/s/p/n/m/l/k#2#/l/m/n/p/r/t/v/y/1/4/7//ACAFAIALAOARATAWAYAZAaAbAc#2#AbAaAYAXAVASAQANAKAHAEAA/9/6/3/0/x/u/s/q/o/m/l/k#2#/l/l/n/o/q/s/v/x/0/3/6/+ABAEAHALAOAQATAVAXAZAaAbAc#2#AbAZAYAWATARAOAMAJAGAEAB/+/8/6/4/2/0/z/y/x#5#/y/z/0/1" };
      for (int i = 0; i < 10000; i++)
        CommonReferences.EncodePitchbends(CommonReferences.DecodePitchbends(param.PitchbendString));
    }


  }
}
