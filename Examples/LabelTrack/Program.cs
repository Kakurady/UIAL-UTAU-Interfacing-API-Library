using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zuoanqh.UIAL;
using zuoanqh.UIAL.UST;

namespace LabelTrack
{
  class Program
  {
    static int Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("Hello World!");
        return 1;
      }


      var lines = new List<string>();
      try
      {
        var encoding = Encoding.GetEncoding("shift_jis", new EncoderExceptionFallback(), new DecoderExceptionFallback());
        using (var reader = new StreamReader(File.OpenRead(args[0]), encoding, false, 4096, true))
        {
          string line = reader.ReadLine();
          while (line != null)
          {
            lines.Add(line);
            line = reader.ReadLine();
          }
        }
      }
      catch (DecoderFallbackException)
      {
        throw;
      }
      catch (IOException)
      {
        throw;
      }
      catch (UnauthorizedAccessException)
      {
        throw;
      }
      catch (NotSupportedException)
      {
        throw;
      }

      var ufile = new USTFile(lines.ToArray());
      double tempo = ufile.Tempo;
      double elapsedTime = 0;
      List<Tuple<double, double, string>> noteTimes = new List<Tuple<double, double,string>>();

      //FIXME a better way to sum this
      foreach (var note in ufile.Notes)
      {
        if (note.Attributes.ContainsKey(USTNote.KEY_TEMPO))
        {
          tempo = Convert.ToDouble(note.Attributes[USTNote.KEY_TEMPO]);
        }
        double durationInMilliseconds = CommonReferences.TicksToMilliseconds(note.Length, tempo);

        var beginTime = elapsedTime;
        var endTime = beginTime + durationInMilliseconds;

        if (note.Lyric != "R") {
          noteTimes.Add(new Tuple<double, double, string>(beginTime / 1000, endTime / 1000, note.Lyric));
        }
        elapsedTime += durationInMilliseconds;
      }

      var outFileName = Path.ChangeExtension(args[0], ".txt");
      using (var writer = new StreamWriter(File.OpenWrite(outFileName)))
      {
        foreach (var note in noteTimes)
        {
          var line = String.Format("{0:N6}\t{1:N6}\t{2}", note.Item1, note.Item2, note.Item3);

          writer.WriteLine(line);
          Console.WriteLine(line);
        }
        writer.WriteLine();
      }
      Console.ReadKey();
      return 0;
    }
  }
}
