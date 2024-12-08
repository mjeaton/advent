using Spectre.Console;

Console.WriteLine();
Console.WriteLine();
AnsiConsole.MarkupLine($":christmas_tree: {getSeasonalString("A Christmas Tale, MadLib Style!")} :christmas_tree:");
Console.WriteLine();
Console.WriteLine();

const string nounPrompt = "Enter a noun:";
const string namePrompt = "Enter a name:";
const string pluralNounPrompt = "Enter a plural noun:";
const string adjectivePrompt = "Enter an adjective:";
const string placeInHousePrompt = "Enter a place in a house:";
const string greetingPrompt = "Enter a greeting:";
const string adverbPrompt = "Enter an adverb:";
const string vehiclePrompt = "Enter a vehicle:";
const string placePrompt = "Enter a place:";
const string phrasePrompt = "Enter a phrase:";

string WhatTheVillageIsCoveredIn = AnsiConsole.Ask<string>(greenOnWhiteText(nounPrompt), "Snow");
string ElfName = AnsiConsole.Ask<string>(redOnWhiteText(namePrompt), "Elf");
string ThingsElvesMake = AnsiConsole.Ask<string>(greenOnWhiteText(pluralNounPrompt), "toys");
string DescriptionOfChildren = AnsiConsole.Ask<string>(redOnWhiteText(adjectivePrompt), "good");
string Workshop = AnsiConsole.Ask<string>(greenOnWhiteText(placeInHousePrompt), "workshop");
string SantasGreeting = AnsiConsole.Ask<string>(redOnWhiteText(greetingPrompt), "Ho, ho, ho!");
string TargetOfGift = AnsiConsole.Ask<string>(greenOnWhiteText(nounPrompt), "child");
string ChildName = AnsiConsole.Ask<string>(redOnWhiteText(namePrompt), "Timmy");
string ElfFeeling = AnsiConsole.Ask<string>(greenOnWhiteText(adjectivePrompt), "excited");
string HowDidTheElfWork = AnsiConsole.Ask<string>(redOnWhiteText(adverbPrompt), "immediately");
string Material1 = AnsiConsole.Ask<string>(redOnWhiteText(nounPrompt), "wood");
string Material2 = AnsiConsole.Ask<string>(greenOnWhiteText(nounPrompt), "paint");
string Material3 = AnsiConsole.Ask<string>(redOnWhiteText(nounPrompt), "glitter");
string Tool = AnsiConsole.Ask<string>(greenOnWhiteText(nounPrompt), "hammer");
string ToyDescription = AnsiConsole.Ask<string>(redOnWhiteText(adjectivePrompt), "shiny");
string Toy = AnsiConsole.Ask<string>(greenOnWhiteText(nounPrompt), "train");
string Vehicle = AnsiConsole.Ask<string>(redOnWhiteText(vehiclePrompt), "sleigh");
string Destination = AnsiConsole.Ask<string>(greenOnWhiteText(placePrompt), "the night sky");
string ChristmasDescription = AnsiConsole.Ask<string>(redOnWhiteText(adjectivePrompt), "magical");
string CatchPhrase = AnsiConsole.Ask<string>(greenOnWhiteText(phrasePrompt), "Merry Christmas!");

Console.WriteLine();

string story = @$"Once upon a time, in a small village covered in [red on white]{WhatTheVillageIsCoveredIn}[/], there was a little elf named [red on white]{ElfName}[/]. [red on white]{ElfName}[/] was very excited because Christmas was just around the corner. Every year, [red on white]{ElfName}[/] and the other elves worked hard to make [red on white]{ThingsElvesMake}[/] for all the [red on white]{DescriptionOfChildren}[/] children around the world.

One day, Santa Claus called [red on white]{ElfName}[/] to his [red on white]{Workshop}[/]. ""[red on white]{SantasGreeting}[/], [red on white]{ElfName}[/]! I need your help with a special task,"" Santa said. ""We need to make a special toy for a {TargetOfGift} named [red on white]{ChildName}[/].""

[red on white]{ElfName}[/] was [red on white]{ElfFeeling}[/] and {HowDidTheElfWork} got to work. First, [red on white]{ElfName}[/] gathered all the materials: [red on white]{Material1}[/], [red on white]{Material2}[/], and [red on white]{Material3}[/]. Then, [red on white]{ElfName} used {Tool}[/] to put everything together. It was hard work, but [red on white]{ElfName}[/] didn't mind because it was for a very special child.

Finally, the toy was ready. It was a [red on white]{ToyDescription} {Toy}[/]. Santa was very pleased. ""Thank you, [red on white]{ElfName}[/]! This is perfect,"" Santa said. ""Now, let's get ready for Christmas Eve!""

On Christmas Eve, [red on white]{ElfName}[/] and the other elves loaded the [red on white]{Vehicle}[/] with all the toys. Santa climbed into the [red on white]{Vehicle}[/] and waved goodbye. ""[red on white underline]{CatchPhrase}[/]"" he shouted as the {Vehicle} took off into the night sky.

[red on white]{ElfName}[/] watched as Santa and the reindeer disappeared into [red on white]{Destination}[/]. It had been a [red on white]{ChristmasDescription}[/] Christmas, and [red on white]{ElfName}[/] couldn't wait to do it all again next year.

The End.";

var storyOutput = new Panel(new Markup(greenOnWhiteText(story)))
    .BorderColor(Color.Red);

AnsiConsole.Write(storyOutput);
Console.WriteLine();

AnsiConsole.MarkupLine($":christmas_tree: {getSeasonalString("Merry Christmas!")} :christmas_tree:");

string greenOnWhiteText(string input)
{
    return $"[green on white]{input}[/]";
}

string redOnWhiteText(string input)
{
    return $"[red on white]{input}[/]";
}

string getSeasonalString(string input)
{
    var inColor = "";
    for (int i = 0; i < input.Length; i++)
    {
        if (i % 2 == 0)
        {
            inColor += $"[red on white]{input[i]}[/]";
        }
        else
        {
            inColor += $"[green on white]{input[i]}[/]";
        }
    }
    return inColor;
}