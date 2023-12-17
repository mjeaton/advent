using System.Text.Json;
using Spectre.Console;

var table = new Table
{
    Title = new TableTitle("People in Space!")
};

table.AddColumn("Person");
table.AddColumn("Craft");

var client = new HttpClient();
HttpResponseMessage response = await client.GetAsync("http://api.open-notify.org/astros.json");
if (response.IsSuccessStatusCode)
{
    string content = await response.Content.ReadAsStringAsync();
    var peopleInSpace = JsonSerializer.Deserialize<Payload>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    foreach(var person in peopleInSpace!.People)
    {
        table.AddRow(person.Name, person.Craft);
    }

    AnsiConsole.Write(table);
    Console.WriteLine($"There are currently {peopleInSpace!.Number} in space!");
}


public class Payload
{
  public string Message { get; set; } = string.Empty;
  public int Number { get; set; }
  public List<Person> People { get; set; } = new();
}

public class Person
{
  public string Name { get; set; } = string.Empty;
  public string Craft { get; set; } = string.Empty;
}