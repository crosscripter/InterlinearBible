using System;
using System.IO;
using System.Text;
using System.Linq;
using static System.Console;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class InterlinearBible
{
	static string OutPath = "out.html";
	static string Path = @"Docs\kjv.txt";
	static string HebrewPath = @"Docs\WLC.src";
	static string GreekPath = @"Docs\STR.src";

	static string[] Lines()
	{
		return File.ReadAllLines(Path, Encoding.UTF8);		
	}

	static string Line(int index)
	{
		return Lines()[index];
	}

	static string Reference(string line)
	{
		return line.Substring(0, line.IndexOf(" ", line.IndexOf(":"))).Trim();
	}

	static dynamic ParseReference(string reference)
	{
		var book = reference.Split(':')[0].Split(' ')[0].Trim();
		var chapter = int.Parse(reference.Split(':')[0].Split(' ')[1].Trim());
		var verse = int.Parse(reference.Split(':')[1].Trim());

		return new {
			Book = book,
			Chapter = chapter,
			Verse = verse
		};
	}

	static string Text(string line)
	{
		return line.Replace(Reference(line), string.Empty).Trim();
	}

	static string ELS(string line)
	{
		return Regex.Replace(Text(line).ToUpper(), @"[^A-Z]", string.Empty).Trim();
	}

	static string[] Words(string line)
	{
		return Regex.Split(Text(line), " ")
			.Select(w => Regex.Replace(w, @"\W", string.Empty))
			.ToArray();
	}

	static string Word(string line, int index)
	{
		return Words(line)[index];
	}

	static bool StartsWith(string text, string find)
	{
		return text.ToUpper().StartsWith(find.ToUpper());
	}

	static string[] Book(string book)
	{
		return Lines().Where(l => StartsWith(l, book)).ToArray();		
	}

	static string[] Chapter(string book, int chapter)
	{
		return Book(book).Where(l => StartsWith(l, $"{book} {chapter}:")).ToArray();
	}

	static string Verse(string book, int chapter, int verse)
	{
		return Chapter(book, chapter).Where(l => StartsWith(l, $"{book} {chapter}:{verse} ")).FirstOrDefault();
	}

	static string[] Verses(string book, int chapter, int from, int to)
	{
		var verses = Chapter(book, chapter);
		var start = Array.IndexOf(verses, Verse(book, chapter, from));
		var end = Array.IndexOf(verses, Verse(book, chapter, to));

		return Chapter(book, chapter)
			.Select((v, i) => new { v, i })
			.Where(x => x.i >= start && x.i <= end)
			.Select(x => x.v).ToArray();
	}

	static string HTML(string verse)
	{
		var reference = Reference(verse);
		var number = int.Parse(reference.Split(':')[1].Trim());
		var text = Regex.Replace(Text(verse), @"\[(.*?)\]", "<em>$1</em>");
		return $"<p><sup>{number}</sup><span>{text}</span></p>";
	}

	static readonly Dictionary<string,string> Books  =new Dictionary<string,string>
	{
		{"Ge", "Genesis"},
		{"Ex", "Exodus"},
		{"Le", "Leviticus"},
		{"Nu", "Numbers"},
		{"De", "Deuteronomy"},
		{"Mt", "Matthew"},
		{"Mr", "Mark"},
		{"Lu", "Luke"},
		{"Joh", "John"}
	};

	static bool IsOT(string book)
	{
		return Array.IndexOf(Books.Keys.ToArray(), book) <= 4; // 38;
	}

	static bool IsNT(string book)
	{
		return !IsOT(book);
	}

	static string Original(string book, int chapter, int verse)
	{
		var source = IsOT(book) ? HebrewPath : GreekPath;
		var oldPath = Path;
		Path = source;
		var original = Verse(book, chapter, verse);
		Path = oldPath;
		return original;
	}

	static void Write(string text)
	{
		WriteLine(text);
		File.AppendAllText(OutPath, text, Encoding.UTF8);
	}

	static void WriteBook(string book)
	{
		var name = Books[book];
		OutPath = name + ".html";
		File.WriteAllText(OutPath, string.Empty, Encoding.UTF8);

		Write($@"
<!doctype html>
<html>
<head>
<title>{name}</title>
<meta charset=""utf-8"">
</head>
<body>");

		var chapter = 0;

		foreach (var verse in Book(book))
		{
			var refer = ParseReference(Reference(verse));

			if (refer.Chapter != chapter)
			{
				chapter = refer.Chapter;
				Write($"<h5>Chapter {chapter}</h5>");
			}

			var text = HTML(verse);
			var original = HTML(Original(refer.Book, refer.Chapter, refer.Verse));
			Write($"{original}{text}\n");
		}

Write(@"
</body>
</html>
");
	}

	static void Main()
	{
		foreach (var book in Books.Take(1))
		{
			var name = Books[book.Key];
			WriteLine($"Writing book of {name}");
			WriteBook(book.Key);
			WriteLine("DONE");
		}
	}
}