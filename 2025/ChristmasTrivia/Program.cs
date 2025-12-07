using Spectre.Console;
using System.Text;
using System.Text.Json;
using ChristmasTrivia;

const int minimumQuestions = 5;
const int maximumQuestions = 100;
var numberOfQuestionsValidationMessage = $"Anything less than 5 questions disappoints Santa and we only have {maximumQuestions} questions total.";
const string dataFile = "data.json";

AnsiConsole.MarkupLine($":christmas_tree: {GetSeasonalString("Christmas Trivia!")} :christmas_tree:");
AnsiConsole.WriteLine();
var numberOfQuestions = AnsiConsole.Prompt(
    new TextPrompt<int>("How many questions would you like to answer?")
        .DefaultValue(5)
        .Validate(n => n is >= minimumQuestions and <= maximumQuestions, numberOfQuestionsValidationMessage)
);

AnsiConsole.MarkupLine("Great! You will be answering [bold]{0}[/] questions.", numberOfQuestions);

IReadOnlyList<TriviaQuestion> allQuestions = Array.Empty<TriviaQuestion>();

AnsiConsole.Status()
    .Spinner(Spinner.Known.Christmas)
    .Start("Loading questions...", ctx =>
    {
        // load questions
        allQuestions = LoadQuestions();
         
        Thread.Sleep(2000);
        AnsiConsole.MarkupLine($"[green]{allQuestions.Count} Questions loaded! Selecting {numberOfQuestions} for you![/]");
    });

var random = new Random();
var pool = allQuestions.ToList();          // make a mutable copy
pool.Shuffle(random);                      // shuffle in-place (no duplicates)
var selectedQuestions = pool
    .Take(Math.Min(numberOfQuestions, pool.Count))
    .ToList();

int correct = 0;

foreach (var question in selectedQuestions)
{
    if (!AskQuestion(question))
        break;
}

ShowSummary();

return;

bool AskQuestion(TriviaQuestion question)
{
    var options = question.Options ?? [];
    
    // Display options with numbers
    AnsiConsole.MarkupLine($"[bold]{question.Question}[/]");
    for (int i = 0; i < options.Count; i++)
    {
        AnsiConsole.MarkupLine($"  [yellow]{i + 1}[/]. {options[i]}");
    }

    while (true)
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("Select an option: ")
                .AllowEmpty()
                .DefaultValue("")
        ).Trim();

        if (int.TryParse(input, out int num) && num >= 1 && num <= options.Count)
        {
            int selectedIndex = num - 1;
            var isCorrect = question.Answer?.Contains(selectedIndex) == true;
            if (isCorrect)
            {
                correct++;
                AnsiConsole.MarkupLine("[bold green]Correct![/]");
            }
            else
            {
                var correctOption = question.Answer?.FirstOrDefault() ?? -1;
                var correctText = correctOption >= 0 && correctOption < options.Count
                    ? options[correctOption]
                    : "Unknown";
                AnsiConsole.MarkupLine($"[bold red]Incorrect.[/] The right answer is [green]{correctText}[/].");
            }
            if (!string.IsNullOrWhiteSpace(question.Explanation))
            {
                AnsiConsole.MarkupLine($"[italic]{question.Explanation}[/]");
            }
            AnsiConsole.WriteLine();
            return true;
        }

        AnsiConsole.MarkupLine("[red]Invalid input. Please enter a number.[/]");
    }
}

void ShowSummary()
{
    var panel = new Panel($"You answered [green]{correct}[/] out of [yellow]{numberOfQuestions}[/] correctly!\nKeep spreading the cheer!")
    {
        Border = BoxBorder.Heavy,
        BorderStyle = new Style(Color.Gold1)
    };

    AnsiConsole.Write(panel);
}

static IReadOnlyList<TriviaQuestion> LoadQuestions()
{
    if (!File.Exists(dataFile))
    {
        throw new FileNotFoundException($"Could not find trivia data at '{dataFile}'.");
    }

    using var stream = File.OpenRead(dataFile);
    var questions = JsonSerializer.Deserialize<List<TriviaQuestion>>(stream);
    if (questions is null)
        return Array.Empty<TriviaQuestion>();

    return questions;
}

static string GetSeasonalString(string input, string evenColor = "red", string oddColor = "green")
{
    if (string.IsNullOrEmpty(input))
        return string.Empty;

    var sb = new StringBuilder(input.Length * 8);
    int visibleIndex = 0;

    foreach (var ch in input)
    {
        if (char.IsWhiteSpace(ch))
        {
            sb.Append($"[on white]{Markup.Escape(ch.ToString())}[/]");
            continue;
        }

        var baseColor = (visibleIndex % 2 == 0) ? evenColor : oddColor;
        var colorWithBackground = $"{baseColor} on white";
        sb.Append($"[{colorWithBackground}]{Markup.Escape(ch.ToString())}[/]");
        visibleIndex++;
    }

    return sb.ToString();
}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list, Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}