using System;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Lists;

namespace TypeProyect
{
    public class Lyrics<T> where T : LyricPhrase
    {
        [JsonProperty("englishLyrics")]
        public SortedList<T> EnglishLyrics = new SortedList<T>();

        [JsonProperty("romajiLyrics")]
        public SortedList<T> RomajiLyrics = new SortedList<T>();

        [JsonProperty("kanjiLyrics")]
        public SortedList<T> KanjiLyrics = new SortedList<T>();

        [JsonIgnore]
        public bool HasEnglishLyrics => EnglishLyrics.Count > 0;

        [JsonIgnore]
        public bool HasJapaneseLyrics => KanjiLyrics.Count > 0 && RomajiLyrics.Count > 0;

        public bool HasJapaneseLyricsAt(double time) => HasLyricsInListAt(RomajiLyrics, time) || HasLyricsInListAt(KanjiLyrics, time);

        public bool HasEnglishLyricsAt(double time) => HasLyricsInListAt(EnglishLyrics, time);

        public bool HasLyricsAt(double time) => HasJapaneseLyricsAt(time) || HasEnglishLyricsAt(time);

        protected bool HasLyricsInListAt(SortedList<T> list, double time) => list.Any(p => p.StartTime >= time && p.EndTime < time);

    }

    public class SideLyrics : Lyrics<SideLyricPhrase>
    {
        public bool HasLyricsInSideAt(LyricSide side, double time)
        {
            return HasLyricsInListAt(SelectLyricsFromSide(EnglishLyrics, side), time) || 
                HasLyricsInListAt(SelectLyricsFromSide(RomajiLyrics, side), time) || 
                HasLyricsInListAt(SelectLyricsFromSide(KanjiLyrics, side), time);
        }

        public SortedList<SideLyricPhrase> SelectLyricsFromSide(SortedList<SideLyricPhrase> list, LyricSide side) => (SortedList<SideLyricPhrase>)(list.Where(p => ((int)(p.Side & side) >= 1)));
    }

    public class LyricPhrase : IComparable
    {
        [JsonProperty("highlighted")]
        public bool Highlighted = false;

        [JsonProperty("endTime")]
        public double EndTime = 10;

        [JsonProperty("pieces")]
        public SortedList<LyricPiece> Phrase = new SortedList<LyricPiece>();

        [JsonIgnore]
        public double StartTime => Phrase[0].StartTime;

        [JsonIgnore]
        public string Text => string.Concat(Phrase.Select(p => p.Text));
        
        public int CompareTo(object obj)
        {
            var phrase = obj as LyricPhrase;
            if(StartTime == phrase.StartTime)
                throw new ArgumentException("Two LyricPhrases must not share the same StartTime", nameof(StartTime));
            if (StartTime > phrase.StartTime)
                return 1;
            else
                return -1;
        }
    }

    public class SideLyricPhrase : LyricPhrase
    {
        [JsonProperty("side")]
        public LyricSide Side = LyricSide.Both;
    }

    public class LyricPiece : IComparable
    {
        [JsonProperty("startTime")]
        public double StartTime = 0;

        [JsonProperty("text")]
        public string Text = "";

        public int CompareTo(object obj)
        {
            var piece = obj as LyricPiece;
            if (StartTime == piece.StartTime)
                throw new ArgumentException("Two LyricPieces must not share the same StartTime", nameof(StartTime));
            if (StartTime > piece.StartTime)
                return 1;
            else
                return -1;
        }
    }

    [Flags]
    public enum LyricSide : byte
    {
        Left = 1,
        Right = 2,
        Both = Left | Right,
    }
}
