Console.WriteLine("The 12 Days of Christmas");
Console.WriteLine();

var lines = File.ReadAllLines("data.csv");
var numWordDict = new Dictionary<int, string>
{
    { 1, "A" },
    { 2, "two" },
    { 3, "three" },
    { 4, "four" },
    { 5, "five" },
    { 6, "six" },
    { 7, "seven" },
    { 8, "eight" },
    { 9, "nine" },
    { 10, "ten" },
    { 11, "eleven" },
    { 12, "twelve" }
};

// per a comment from James Curran, simplifying.
var vowels = "aeiou";

var specialWords = new Dictionary<string, string>
{
    {"goose", "geese"}
};

var verses = new Dictionary<int, string>();
int currentDay = 0;
string previousLine2 = "";
foreach(var line in lines.Skip(1))
{
    currentDay++;
    var day = line.Split(',');

    (string line1, string line2) = getOutputForDay(day);
    line2 = line2 + Environment.NewLine + previousLine2;

    verses.Add(currentDay, line1 + Environment.NewLine + line2);

    previousLine2 = currentDay == 1 ? line2.Replace("A", "a").Insert(0, "And ") : line2;
}

foreach(int verseNumber in verses.Keys)
{
    Console.WriteLine(verses[verseNumber]);
}

(string line1, string line2) getOutputForDay(string[] day) 
{
    var actualDay = Convert.ToInt16(day[0]);
    string gift = actualDay > 1 ? pluralize(day[2]) : day[2];
    var doing = day[3] == "NA" ? "" : day[3];
    var adjective = day[4] == "NA" ? "" : day[4];
    var location = day[5] == "NA" ? "" : day[5];
    
    string numberOfThings = numberToText(actualDay);

    var line1 = $"On the {day[1]} day of Christmas my true love gave to me";
    string line2 = new[] { numberOfThings, adjective, gift, doing, location }
        .Where(s => !string.IsNullOrEmpty(s))
        .Aggregate("", (acc, cur) => acc + " " + cur)
        .Trim();

    return (line1, line2);
}

string numberToText(int number)
{
    return numWordDict.TryGetValue(number, out var value) ? value : "??";
}

string pluralize(string word)
{
    if(specialWords.ContainsKey(word))
    {
        return specialWords[word];
    }

    // -s, -x, -sh, -ch, -ss or -z
    if(word.EndsWith("s") 
        || word.EndsWith("x") 
        || word.EndsWith("sh") 
        || word.EndsWith("ch") 
        || word.EndsWith("ss") 
        || word.EndsWith("z"))
    {
        return word + "es";
    }
    if(word.EndsWith("y"))
    {
        // lady
        var nextToLast = word[^2];

        if(!vowels.Contains(nextToLast))
        {
            return word[..^1] + "ies";
        } 
        else 
        {
            return word + "s";
        }
    }
    return word + "s";
}