using System;
using System.IO;
using System.Linq;
using static System.Console;
using System.Collections.Generic;

class IB 
{
	static string GetReference(string line)
	{
		var colonIndex = line.IndexOf(":");
		var spaceIndex = line.IndexOf(" ", colonIndex);
		return line.Substring(0, spaceIndex).Trim();
	}

	static string GetBook(string reference)
	{
		return reference.Split(' ')[0].Trim();
	}

	static int GetChapter(string reference)
	{
		return int.Parse(reference.Split(':')[0].Split(' ')[1].Trim());
	}

	static int GetVerse(string reference)
	{
		return int.Parse(reference.Split(':')[1].Trim());
	}

	static string GetText(string line)
	{
		var reference = GetReference(line);
		var start = line.IndexOf(reference);
		start += reference.Length;
		return line.Substring(start).Trim();
	}

	static string[] GetVerses(string file, string book, int chapter=0)
	{
		var lines = File.ReadAllLines(file);
		var reference = $"{book} {(chapter == 0 ? "" : chapter.ToString() + ":")}";
		return lines.Where(l => l.StartsWith(reference)).ToArray();
	}

	static readonly string Base = @"Code\Elise.Net\Elise\Elise\Resources\Sources\";
	static readonly string Headings = @"Docs\bible-outline.txt";

	static string GetChapterHeading(string reference)
	{
		var lines = File.ReadAllLines(Headings);
		var book = GetBook(reference);
		var chapter = GetChapter(reference);
		var line = lines.Where(l => l.StartsWith($"{book} {chapter}:")).FirstOrDefault();
		if (string.IsNullOrEmpty(line)) return line;
		return line.Split(':')[1].Trim();
	}

	static string GetVerseHeading(string reference)
	{
		var lines = File.ReadAllLines(Headings);
		var book = GetBook(reference);
		var chapter = GetChapter(reference);
		var verseNumber = GetVerse(reference);
		var line = lines.Where(l => l.StartsWith($"{book} {chapter}:")).FirstOrDefault();
		var lineIndex = Array.IndexOf(lines, line);
		var heading = string.Empty;

		for (var i = lineIndex + 1; i < lines.Length; i++)
		{
			var verse = lines[i];
			if (!char.IsDigit(verse[0])) break;
			var currentVerse = int.Parse(verse.Split('.')[0].Trim());
			heading = verse.Split('.')[1].Trim();
			if (verseNumber > currentVerse) break;
		}

		return heading;
	}

	static void Build()
	{
		var lines = GetVerses(Base + "kjv.src", "Ge");
		
		foreach (var line in lines)
		{
			var reference = GetReference(line);
			var book = GetBook(reference);
			var chapter = GetChapter(reference);
			var verse = GetVerse(reference);
			var text = GetText(line);
			var chapterHeading = GetChapterHeading(reference);
			var verseHeading = GetVerseHeading(reference);
			WriteLine($"{book} {chapter}:{verse} ({chapterHeading}) -- {text} ({verseHeading})");
		}
	}

	static void Main()
	{
		try 
		{
			Build();
		}
		catch (Exception e)
		{
			WriteLine(e);
		}
	}
}