using System;
using System.Collections.Generic;
using System.Linq;
using ViviStreamWebsite.Models;

namespace ViviStreamWebsite.Helpers
{
    public static class EasyCustomSearch
    {
        private static string GetString(string s)
        {
            var str = s.Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u')
                .Replace('Á', 'a').Replace('É', 'e').Replace('Í', 'i').Replace('Ó', 'o').Replace('Ú', 'u')
                .Replace('Ō', 'o').Replace('-', ' ').Replace('/', ' ').Replace(':', ' ').Replace('&', ' ')
                .Replace('(', ' ').Replace(')', ' ').Replace('!', ' ').Replace('?', ' ');

            char[] arr = str.ToCharArray();

            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))));
            return new string(arr);
        }

        public static IEnumerable<AllSongs> SearchSong(string song, List<AllSongs> playlist)
        {
            song = song.ToLower();

            if (song.Contains("\""))
            {
                song = song.Replace("\"", string.Empty);
                return playlist.Where(x => x.Title.Contains(song)).ToList();
            }

            song = GetString(song);

            var result = new List<SearchSong>();
            var words = song.Split(' ');

            foreach (var p in playlist)
            {
                var title = GetString(p.Title);

                //var title = p.LowerCaseTitle.Replace('/', ' ').Replace(':', ' ').Replace('-', ' ').Replace('(', ' ').Replace(')', ' ');
                var pWords = title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var count = 0;
                var totalWords = pWords.Count;

                foreach (var w in words)
                {
                    if (pWords.Contains(w))
                    {
                        count++;
                        pWords.Remove(w);
                    }
                }

                var percentage = count / (double)totalWords;

                if (percentage > 0)
                    result.Add(new SearchSong() { Song = p, Score = percentage, Words = count });
            }

            if (result == null)
                return playlist.Where(x => x.Title.Contains(song)).ToList();

            var search = result.OrderByDescending(x => x.Words).ThenByDescending(x => x.Score);
            return search.Select(x => x.Song);
        }

        public static AllSongs SearchSongFirst(string song, List<AllSongs> playlist)
        {
            song = song.ToLower();

            if (song.Contains("\""))
            {
                song = song.Replace("\"", string.Empty);
                return playlist.Where(x => x.Title.Contains(song)).FirstOrDefault();
            }

            song = GetString(song);

            AllSongs result = null;
            double maxScore = 0;
            int maxMatches = 0;
            var words = song.Split(' ');

            foreach (var p in playlist)
            {
                var title = GetString(p.Title);

                //var title = p.LowerCaseTitle.Replace('/', ' ').Replace(':', ' ').Replace('-', ' ').Replace('(', ' ').Replace(')', ' ');
                var pWords = title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var count = 0;

                foreach (var w in words)
                {
                    if (pWords.Contains(w))
                        count++;
                }

                var percentage = count / (double)pWords.Length;

                if (count >= maxMatches)
                {
                    if (count > maxMatches)
                    {
                        maxScore = percentage;
                        result = p;
                        maxMatches = count;
                    }
                    else
                    {
                        if (percentage > maxScore)
                        {
                            maxScore = percentage;
                            result = p;
                            maxMatches = count;
                        }
                    }
                }
            }

            if (result == null)
                return playlist.Where(x => x.Title.Contains(song)).FirstOrDefault();

            return result;
        }

        public static IEnumerable<MySongs> SearchSong(string song, List<MySongs> playlist)
        {
            song = song.ToLower();

            if (song.Contains("\""))
            {
                song = song.Replace("\"", string.Empty);
                return playlist.Where(x => x.Title.Contains(song)).ToList();
            }

            song = GetString(song);

            var result = new List<MySearchSong>();
            var words = song.Split(' ');

            foreach (var p in playlist)
            {
                var title = GetString(p.Title);

                //var title = p.LowerCaseTitle.Replace('/', ' ').Replace(':', ' ').Replace('-', ' ').Replace('(', ' ').Replace(')', ' ');
                var pWords = title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var count = 0;

                foreach (var w in words)
                {
                    if (pWords.Contains(w))
                        count++;
                }

                var percentage = count / (double)pWords.Length;

                if (percentage > 0)
                    result.Add(new MySearchSong() { Song = p, Score = percentage, Words = count });
            }

            if (result == null)
                return playlist.Where(x => x.Title.Contains(song)).ToList();

            return result.OrderByDescending(x => x.Words).ThenByDescending(x => x.Score).Select(x => x.Song);
        }

        public static IEnumerable<AllSongs> SearchSongByGame(string song, List<AllSongs> playlist)
        {
            song = song.ToLower();

            if (song.Contains("\""))
            {
                song = song.Replace("\"", string.Empty);
                return playlist.Where(x => x.Game.Contains(song)).ToList();
            }

            song = GetString(song);

            var result = new List<SearchSong>();
            var words = song.Split(' ');

            foreach (var p in playlist)
            {
                var game = GetString(p.Game);

                //var game = p.LowerCaseTitle.Replace('/', ' ').Replace(':', ' ').Replace('-', ' ').Replace('(', ' ').Replace(')', ' ');
                var pWords = game.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var count = 0;

                foreach (var w in words)
                {
                    if (pWords.Contains(w))
                        count++;
                }

                var percentage = count / (double)pWords.Length;

                if (percentage > 0)
                    result.Add(new SearchSong() { Song = p, Score = percentage, Words = count });
            }

            if (result == null)
                return playlist.Where(x => x.Game.Contains(song)).ToList();

            return result.OrderByDescending(x => x.Words).ThenByDescending(x => x.Score).Select(x => x.Song);
        }
    }
}
