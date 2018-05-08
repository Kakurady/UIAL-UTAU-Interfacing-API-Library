using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// This class models a UST file.
  /// Note: 
  /// %HOME% means the current user's home directory.
  /// %VOICE% is the voicebank directory set in tools->option->path.
  /// if the directory is not absolute, it's the directory relative to UTAU's install directory.
  /// Also, FUCK SHIFT-JIS
  /// </summary>
  public class USTFile
  {
    public const string KEY_PROJECT_NAME = "ProjectName";
    public const string KEY_TEMPO = "Tempo";
    public const string KEY_VOICE_DIR = "VoiceDir";
    public const string KEY_OUT_FILE = "OutFile";
    public const string KEY_CACHE_DIR = "CacheDir";
    /// <summary>
    /// Wavtool, or "append"
    /// </summary>
    public const string KEY_TOOL1 = "Tool1";
    /// <summary>
    /// Resampler, or "resample"
    /// </summary>
    public const string KEY_TOOL2 = "Tool2";
    public const string KEY_MODE2 = "Mode2";

    /// <summary>
    /// Whatever is in [#VERSION] section. 
    /// </summary>
    public string Version;

    /// <summary>
    /// Or BPM.
    /// </summary>
    public double Tempo { get { return Convert.ToDouble(ProjectInfo[KEY_TEMPO]); } set { ProjectInfo[KEY_TEMPO] = Convert.ToString(value); } }
    /// <summary>
    /// Note this is useless on windows. we do however, provide a method converting multi-track files to multiple usts.
    /// </summary>
    //public int Tracks;
    public string ProjectName { get { return ProjectInfo[KEY_PROJECT_NAME]; } set { ProjectInfo[KEY_PROJECT_NAME] = value; } }
    /// <summary>
    /// See class comment for directory meaning.
    /// </summary>
    public string VoiceDir { get { return ProjectInfo[KEY_VOICE_DIR]; } set { ProjectInfo[KEY_VOICE_DIR] = value; } }
    /// <summary>
    /// See class comment for directory meaning.
    /// </summary>
    public string OutFile { get { return ProjectInfo[KEY_OUT_FILE]; } set { ProjectInfo[KEY_OUT_FILE] = value; } }
    /// <summary>
    /// See class comment for directory meaning.
    /// </summary>
    public string CacheDir { get { return ProjectInfo[KEY_CACHE_DIR]; } set { ProjectInfo[KEY_CACHE_DIR] = value; } }
    /// <summary>
    /// The wavtool used.
    /// </summary>
    public string Tool1 { get { return ProjectInfo[KEY_TOOL1]; } set { ProjectInfo[KEY_TOOL1] = value; } }
    /// <summary>
    /// The sampler used.
    /// </summary>
    public string Tool2 { get { return ProjectInfo[KEY_TOOL2]; } set { ProjectInfo[KEY_TOOL2] = value; } }
    /// <summary>
    /// Whether the project is in edit mode 2.
    /// <para />You probably want this to be true since mode 2 is the newer edit mode for UTAU. 
    /// </summary>
    public bool Mode2 { get { return Convert.ToBoolean(ProjectInfo[KEY_MODE2]); } set { ProjectInfo[KEY_MODE2] = Convert.ToString(value); } }



    /// <summary>
    /// Yes, you can have more than 1 tracks. if you would like to pretend there's only one, use "Notes"
    /// </summary>
    public List<List<USTNote>> TrackData;
    /// <summary>
    /// This is a shortcut that gives the first track. 
    /// </summary>
    public List<USTNote> Notes
    {
      get { return TrackData[0]; }
      set
      {
        if (TrackData == null)
          TrackData = new List<List<USTNote>>();
        TrackData[0] = value;
      }
    }

    public IDictionary<string, string> ProjectInfo;

    /// <summary>
    /// Creates a ust from (absolute or relative) path given.
    /// Note: Emperically, we found 8kb to be as good as whole file.
    /// </summary>
    /// <param name="fPath"></param>
    public USTFile(string fPath) //
      //: this(ByLineFileIO.ReadFileNoWhitespace(fPath, zuio.GetEncUde(fPath,8192, Encoding.GetEncoding("Shift_JIS"))).ToArray())
    { }

    private struct USTIniSection
    {
      public string header;
      /// <summary>Line number of beginning of section content (Inclusive)</summary>
      public int startLine;
      /// <summary>Line number of end of section content (Exclusive)</summary>
      public int endLine;

      public int Length { get { return endLine - startLine; } }

      public USTIniSection(string header, int startLine, int endLine)
      {
        this.header = header;
        this.startLine = startLine;
        this.endLine = endLine;
      }
    }

    /// <summary>
    /// Creates a ust from string array.
    /// </summary>
    /// <param name="data"></param>
    public USTFile(string[] data)
    {
      TrackData = new List<List<USTNote>>();

      var lines = new List<string>(data);

      // split by sections first
      USTIniSection preamble = new USTIniSection("", 0, data.Length); // not used.
      List<USTIniSection> sections = new List<USTIniSection>();
      for (int i = 0; i < data.Length; i++)
      {
        //thank you c# for providing @.. escaping more escape character is just...
        var match = Regex.Match(data[i], @"^\[#.*\]$");
        if (match.Success)
        {
          // special case: 
          if (sections.Count == 0)
          {
            preamble.endLine = i;
          }
          else
          {
            // modify the last section
            // why are List<T> and Stack<T> separate classes?
            var lastSection = sections[sections.Count - 1];
            lastSection.endLine = i;
            sections[sections.Count - 1] = lastSection;
          }
          sections.Add(new USTIniSection(data[i], i + 1, data.Length));
        }
      }
      //section[0] is ust version, we only handled the newest.
      Debug.Assert(sections[0].header == "[#VERSION]", "UST's first section isn't [#VERSION]");
      Version = data[sections[0].startLine];
      //section[1] is other project info
      Debug.Assert(sections[1].header == "[#SETTING]", "UST's second section isn't [#SETTING]");
      ProjectInfo = new Dictionary<string, string>(
        lines
        .Skip(sections[1].startLine)
        .Take(sections[1].Length)
        .Where(line => line.Contains('='))
        .ToDictionary(
            line => line.Substring(0, line.IndexOf('=')),
            line => line.Substring(line.IndexOf('=') + 1)
          )
        );

      {
        List<USTNote> track = new List<USTNote>(sections.Count - 3); // the common case is only one track
        foreach (var section in sections.Skip(2))
        {
          if (section.header == "[#TRACKEND]")
          {
            TrackData.Add(track);
            track = new List<USTNote>(0); // more than one track is uncommon, so leaving capacity as empty as we're likely throwing this List away.
            // yes there can be more than 1 tracks. not on windows versions though!
          }
          else
          {
            var noteLines = lines.GetRange(section.startLine, section.Length);
            track.Add(new USTNote(noteLines));
          }
        }
        // unexpected case: notes not followed by [#TRACKEND]
        if (track.Count > 0)
        {
          TrackData.Add(track);
        }
        // unexpected case: no [#TRACKEND] at all
        if (TrackData.Count == 0)
        {
          TrackData.Add(new List<USTNote>());
        }
      }

      //now we need to fix portamentos if any.
      foreach (var track in TrackData)
        for (int i = 1; i < track.Count; i++)//sliding window of i-1, i
          if (track[i].Portamento != null && !track[i].Portamento.HasValidPBS1())//note this is [i-1] - [i] because it's relative to [i]
            track[i].Portamento.PBS[1] = track[i - 1].NoteNum - track[i].NoteNum;
    }

    /// <summary>
    /// This is sort of a copy constructor. Yes, this will try to make deep copies of everything.
    /// </summary>
    /// <param name="Version"></param>
    /// <param name="ProjectInfo"></param>
    /// <param name="TrackData"></param>
    public USTFile(string Version, IDictionary<string, string> ProjectInfo, List<List<USTNote>> TrackData)
    {
      this.Version = Version;
      //this.ProjectInfo = new DictionaryDataObject(ProjectInfo);
      this.TrackData = new List<List<USTNote>>();
      foreach (var t in TrackData)
      {
        var myTrack = new List<USTNote>();
        foreach (var n in t)
          myTrack.Add(new USTNote(n));
        this.TrackData.Add(myTrack);
      }
    }

    /// <summary>
    /// Cheap trick to save code. or did i.
    /// </summary>
    /// <param name="Notes"></param>
    /// <returns></returns>
    private static List<List<USTNote>> MakeTrackData(List<USTNote> Notes)
    {
      var l = new List<List<USTNote>> { Notes };
      return l;
    }

    public USTFile(string Version, IDictionary<string, string> ProjectInfo, List<USTNote> Notes)
      : this(Version, ProjectInfo, MakeTrackData(Notes))
    { }

    //FIXME make a constructor to make an empty USTFile
    public USTFile()
      : this("blahblah Version Blah", new Dictionary<string, string>(), MakeTrackData(new List<USTNote>()))
    {
      throw new NotImplementedException();
    }

    public USTFile(USTFile that)
      : this(that.Version, that.ProjectInfo, that.TrackData)
    { }

    /// <summary>
    /// Converts it back to its ust format. 
    /// </summary>
    /// <returns></returns>
    public List<string> ToStringList()
    {
      var ans = new List<string> { "[#VERSION]", Version, "[#SETTING]" };
      ans.AddRange(ProjectInfo.Select(kv => kv.Key + "=" + kv.Value));

      foreach (var Notes in TrackData)//adding notes for each track.
      {
        for (int i = 0; i < Notes.Count; i++)
        {
          USTNote n = Notes[i];
          string s = "" + i;
          ans.Add("[#" + s.PadLeft(4, '0') + "]");
          ans.AddRange(n.ToStringList());
        }
        ans.Add("[#TRACKEND]");
      }
      ans.Add("");
      return ans;
    }

    /// <summary>
    /// Converts it back to its ust format. 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Join("\r\n", ToStringList().ToArray()) + "\r\n";
    }
  }
}
