﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace zuoanqh.UIAL
{
  public class CommonReferences
  {
    
    /// <summary>
    /// This seems to be the default for utau.
    /// </summary>
    public static readonly double TICKS_PER_BEAT = 480;

    private static readonly double CONVERSION_RATE = 60000 / TICKS_PER_BEAT;

    public static double TicksToMilliseconds(double Ticks, double BPM)
    { return Ticks * CONVERSION_RATE / BPM; }

    public static double MillisecondsToTicks(double Milliseconds, double BPM)
    { return Milliseconds * BPM / CONVERSION_RATE; }

    /// <summary>
    /// Return the effect of Velocity in multiplier -- 1 means 100%.
    /// </summary>
    /// <param name="Velocity"></param>
    /// <returns></returns>
    public static double GetEffectiveVelocityFactor(double Velocity)
    {
      return 2 * Math.Pow(0.5, (Velocity / 100));
    }
    /// <summary>
    /// Convert from a multiplier back to its velocity value.
    /// </summary>
    /// <param name="EffectiveVelocityFactor"></param>
    /// <returns></returns>
    public static double GetVelocity(double EffectiveVelocityFactor)
    {
      return Math.Log(Math.Pow(EffectiveVelocityFactor / 2, 100), 0.5);
    }

    /// <summary>
    /// Encoding of the 13th parameter given to resamplers.
    /// </summary>
    public static readonly string PITCHBEND_ENCODING = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    /// <summary>
    /// Converts a character to its encoded number.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static int GetEncoding(char c)
    { return PITCHBEND_ENCODING.IndexOf(c); }

    /// <summary>
    /// Encode a sequence of pitchbend magnitudes into the string for resampler's 13th parameter.
    /// </summary>
    /// <param name="Pitchbends"></param>
    /// <returns></returns>
    public static string EncodePitchbends(int[] Pitchbends)
    {
      
      //Item1 fold it into Tuples of (pitch, times repeated)
      List<Tuple<int, int>> l = new List<Tuple<int, int>>();
      int current = 0;
      while (current < Pitchbends.Length)
      {
        int count = 1;

        //count how many time the current element has appeared
        for (int i = current + 1; i < Pitchbends.Length; i++)
        {
          if (Pitchbends[i] != Pitchbends[current]) break;
          count++;
        }

        l.Add(new Tuple<int, int>(Pitchbends[current], count));
        current += count;
      }

      //now encode that string.
      StringBuilder ans = new StringBuilder();
      foreach (var v in l)
      {
        //convert things back.
        int val = v.Item1;
        if (val < 0) val += 4096;//again some magic defined by original encoding algorithm

        //segment the two digits out
        ans.Append(CommonReferences.PITCHBEND_ENCODING[val / 64])
          .Append(CommonReferences.PITCHBEND_ENCODING[val % 64]);

        if (v.Item2 > 2)//if repeated, add that bit.
          ans.Append("#").Append(v.Item2).Append("#");
        else if (v.Item2 == 2)//had to do some testing to find this out.
          ans.Append(ans[ans.Length - 2]).Append(ans[ans.Length - 2]);
      }

      return ans.ToString();
    }

    /// <summary>
    /// Decode the string given to resampler's 13th parameter back to pitchbend magnitudes.
    /// </summary>
    /// <remarks>
    /// This function expects pitch bend parameter to be well-formed, without
    /// whitespace or extra characters.
    /// 
    /// The pitch bend parameter is a kind of run-length encoding, where each
    /// segment is encoded with two characters in a custom base-64 scheme,
    /// giving 4096 levels where negative levels are represented in two's
    /// complement format. Repeated segments are replaced with the value and 
    /// the number of repeats.
    /// 
    /// Sometimes segments that could have been run-length encoded are instead 
    /// repeated verbatim. Maybe UTAU stores them in a higher resolution
    /// internally?
    /// </remarks>
    /// <param name="PitchbendString"></param>
    /// <returns></returns>
    public static int[] DecodePitchbends(string PitchbendString)
    {
      // The pitch bend format is probably parsable with a finite automata, but I'm not sure how to
      // express it in RegEx in a way that makes sense, so here's a handwritten parser
      
      string input = PitchbendString;
      List<int> l = new List<int>(input.Length / 2);

      int i = 0;
      while (i < input.Length)
      {
        // Step one: parse a run 
        // we expect a run to be two characters...
        if (i + 2 > input.Length) {
          throw new ArgumentException($"Pitch bend segment must be two characters (at position {i})");
        }

        // ...that are also in the list
        int hi = CommonReferences.GetEncoding(input[i + 0]);
        int lo = CommonReferences.GetEncoding(input[i + 1]);
        if (hi < 0 || lo < 0)
        {
          throw new ArgumentException($"Pitch bend segment \"{input[i + 0]}{input[i + 1]}\" (at position {i}) isn't in the encoding dictionary");
        }
        int run = hi * 64 + lo;
        if (run >= 2048) run -= 4096;//i know, that IS weird, but that is what we need to work with.

        // move over the run we just parsed,
        i += 2;
        // now we expect another run, a length, or end of input

        // Step two: parse length (if we have one)
        bool hasLength = false;
        int length = 0;
        if (i < input.Length && input[i] == '#')
        {
          hasLength = true;
          // move over the #
          i++;
          // now we expect digits
          while(i < input.Length && input[i] != '#')
          {
            int val = Convert.ToInt32(input[i]) - Convert.ToInt32('0');
            if (val < 0 || val > 9) {
              throw new ArgumentException($"Pitch bend run length '{input[i]}' (at position {i}) is not a decimal number");
            }
            length = length * 10 + val;
            i++;
            // now we expect digits, or end of input, or #
          }
          // now we're either at the next # or at the end of input.
          i++;
        }
        // now we're at another run, end of input, or one char past end of input.

        if (!hasLength) { length = 1; }

        for(int j = 0; j < length; j++)
        {
          l.Add(run);
        }
      }

      return l.ToArray();
    }

    /// <summary>
    /// All possible note names from C1 to B7.
    /// </summary>
    public static readonly IReadOnlyList<string> NOTENAMES;

    public static readonly string NOTENAME_HIGHEST;
    public static readonly string NOTENAME_LOWEST;

    /// <summary>
    /// This reverse the mapping of NOTENAMES.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> NOTENAME_INDEX_RANK;

    /// <summary>
    /// This converts note names into NoteNums.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> NOTENAME_INDEX_UST;

    static CommonReferences()
    {
      NOTENAMES = new string[] {
      "C1", "C#1", "D1", "D#1", "E1", "F1", "F#1", "G1", "G#1", "A1", "A#1", "B1",
      "C2", "C#2", "D2", "D#2", "E2", "F2", "F#2", "G2", "G#2", "A2", "A#2", "B2",
      "C3", "C#3", "D3", "D#3", "E3", "F3", "F#3", "G3", "G#3", "A3", "A#3", "B3",
      "C4", "C#4", "D4", "D#4", "E4", "F4", "F#4", "G4", "G#4", "A4", "A#4", "B4",
      "C5", "C#5", "D5", "D#5", "E5", "F5", "F#5", "G5", "G#5", "A5", "A#5", "B5",
      "C6", "C#6", "D6", "D#6", "E6", "F6", "F#6", "G6", "G#6", "A6", "A#6", "B6",
      "C7", "C#7", "D7", "D#7", "E7", "F7", "F#7", "G7", "G#7", "A7", "A#7", "B7"
      }.ToList();

      NOTENAME_HIGHEST = NOTENAMES[NOTENAMES.Count - 1];
      NOTENAME_LOWEST = NOTENAMES[0];

      var indexrank = new Dictionary<string, int>();
      var indexust = new Dictionary<string, int>();

      for (int i = 0; i < NOTENAMES.Count; i++)
      {
        indexrank.Add(NOTENAMES[i], i);
        indexust.Add(NOTENAMES[i], i + 24);//0 is 24 in USTs.
      }
      NOTENAME_INDEX_RANK = indexrank;
      NOTENAME_INDEX_UST = indexust;

    }

    /// <summary>
    /// Converts NoteNum into its note name.
    /// </summary>
    /// <param name="NoteNum">C1 is 24.</param>
    /// <returns></returns>
    public static string GetNoteName(int NoteNum)
    {
      return NOTENAMES[NoteNum - 24];
    }

    private CommonReferences() { }
  }
}
