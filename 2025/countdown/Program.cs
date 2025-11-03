using System;
using System.Text;
using Spectre.Console;
using System.Threading.Tasks;

namespace countdown;

static class Program
{
    static async Task Main(string[] args)
    {
        // Usage: dotnet run -- --all-on   (or -a)
        var allOn = false;
        // Default vertical gap lines between bulbs and message
        var gapLines = 3;

        // Parse args: support --all-on / -a and --gap=N or -g N
        for (int ai = 0; ai < args.Length; ai++)
        {
            var a = args[ai];
            if (a == "--all-on" || a == "-a")
            {
                allOn = true;
                continue;
            }

            if (a.StartsWith("--gap=", StringComparison.OrdinalIgnoreCase))
            {
                var seg = a.Substring("--gap=".Length);
                if (int.TryParse(seg, out var g)) gapLines = Math.Max(0, g);
                continue;
            }

            if (a == "-g" && ai + 1 < args.Length)
            {
                if (int.TryParse(args[ai + 1], out var g)) gapLines = Math.Max(0, g);
                ai++; // consumed
                continue;
            }
        }

        // Fixed repeating sequence with (offColor, onColor) pairs
        var colorPairs = new (string off, string on)[]
        {
            ("#800000", "#FF0000"), // red
            ("#006400", "#00FF00"), // green
            ("#CC8400", "#FFA500"), // orange
            ("#00008B", "#0000FF"), // blue
            ("#CCCC00", "#FFFF00"), // yellow
        };

        const int desiredBulbCount = 30;
        var cord = "[#00A000]───[/]"; // green cord segment between bulbs
        var rand = new Random();


        // Helper: compute how many bulbs fit on one line without wrapping.
        // Each bulb visually takes 1 char, each cord segment is 3 chars ("───"), plus 2 leading spaces.
        static int ComputeRenderBulbCount(int desired)
        {
            try
            {
                var width = Console.WindowWidth;
                // totalDisplayed = 2 + bulbs + (bulbs-1)*3 = 4*bulbs - 1 <= width
                var max = Math.Max(1, (width + 1) / 4);
                return Math.Min(desired, max);
            }
            catch
            {
                // If Console.WindowWidth isn't available (e.g., redirected), just return desired
                return desired;
            }
        }

        // Smooth flicker state: bulbs only toggle occasionally
        const double changeProbabilityPerFrame = 0.08; // chance per bulb per frame to toggle
        var bulbStatesTop = new bool[desiredBulbCount];
        var bulbStatesBottom = new bool[desiredBulbCount];

        // Initialize bulbs randomly (or all on if flag set)
        for (int i = 0; i < desiredBulbCount; i++)
        {
            bulbStatesTop[i] = allOn ? true : rand.NextDouble() > 0.5;
            bulbStatesBottom[i] = allOn ? true : rand.NextDouble() > 0.5;
        }

        // Hide the cursor for smoother display and restore it when the user cancels (Ctrl+C)
        Console.CancelKeyPress += (_, _) => Console.CursorVisible = true;
        try { Console.CursorVisible = false; } catch { /* best-effort */ }

        // Track the row where bulbs are rendered so per-bulb updates write to the correct line.
        var bulbRow = 0;
        try { bulbRow = Console.CursorTop; } catch { bulbRow = 0; }

        // Helper: convert hex color (#RRGGBB) to RGB tuple
        static (int r, int g, int b) HexToRgb(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return (r, g, b);
        }

        // Helper: write a FigletText using the provided FigletFont and Spectre.Color for reliable colored output
        static void WriteFigletWithHexColor(FigletFont? font, string text, string hexColor)
        {
            var (r, g, b) = HexToRgb(hexColor);
            var useFont = font ?? FigletFont.Default;
            try
            {
                var color = new Spectre.Console.Color((byte)r, (byte)g, (byte)b);
                AnsiConsole.Write(new FigletText(useFont, text).Color(color));
            }
            catch
            {
                // fallback to plain uncolored rendering
                try { AnsiConsole.Write(new FigletText(useFont, text)); } catch { }
            }
        }

        // Helper: days until next Christmas (Dec 25)
        static int DaysUntilChristmas(DateTime now)
        {
            var year = now.Year;
            var christmas = new DateTime(year, 12, 25);
            if (now.Date > christmas.Date)
            {
                christmas = new DateTime(year + 1, 12, 25);
            }
            return (christmas.Date - now.Date).Days;
        }

        // Helper: build plain days lines for figlet rendering
        static (string daysLine, string untilLine) BuildDaysParts()
        {
            var days = DaysUntilChristmas(DateTime.Now);
            var daysLine = days == 0 ? "Today!" : $"{days} day{(days == 1 ? "" : "s")}";
            var untilLine = "until Christmas";
            return (daysLine, untilLine);
        }


        // If all-on is requested, render once and don't continuously clear/redraw
        if (allOn)
        {
            // On all-on, clear and draw top bulbs, figlet message, then bottom bulbs
            Console.Clear();
            bulbRow = 0;
            var renderBulbCount = ComputeRenderBulbCount(desiredBulbCount);

            // Attempt to load bundled FIGlet font (fonts/christmas.flf). Fall back to default if missing or invalid.
            FigletFont? figletFont = null;
            try
            {
                var fontPath = System.IO.Path.Combine(AppContext.BaseDirectory, "fonts", "christmas.flf");
                if (!System.IO.File.Exists(fontPath))
                {
                    // also try relative to source path (when running from project dir)
                    fontPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fonts", "christmas.flf");
                }

                if (System.IO.File.Exists(fontPath))
                {
                    figletFont = FigletFont.Load(fontPath);
                }
            }
            catch
            {
                figletFont = null;
            }

            if (figletFont == null) figletFont = FigletFont.Default;

            // Draw top bulbs (raw ANSI for colors)
            var topSb = new StringBuilder();
            topSb.Append("  ");
            for (int i = 0; i < renderBulbCount; i++)
            {
                var pair = colorPairs[i % colorPairs.Length];
                var bulbColorHex = bulbStatesTop[i] ? pair.on : pair.off;
                var (r, g, b) = HexToRgb(bulbColorHex);
                topSb.Append($"\u001b[38;2;{r};{g};{b}m");
                topSb.Append('●');
                topSb.Append("\u001b[0m");
                if (i < renderBulbCount - 1)
                {
                    var (cr, cg, cb) = HexToRgb("#00A000");
                    topSb.Append($"\u001b[38;2;{cr};{cg};{cb}m───\u001b[0m");
                }
            }
            Console.SetCursorPosition(0, bulbRow);
            Console.Write(topSb.ToString());

            // Gap lines
            for (int g = 0; g < gapLines; g++) Console.WriteLine();

            // Figlet message: days line and until line (use loaded font)
            var (daysLine, untilLine) = BuildDaysParts();
            // Color days line gold, 'until' line green
            WriteFigletWithHexColor(figletFont, daysLine, "#FFD700");
            WriteFigletWithHexColor(figletFont, untilLine, "#00A000");

            // Gap lines
            for (int g = 0; g < gapLines; g++) Console.WriteLine();

            // Bottom bulbs
            var bottomSb = new StringBuilder();
            bottomSb.Append("  ");
            for (int i = 0; i < renderBulbCount; i++)
            {
                var pair = colorPairs[i % colorPairs.Length];
                var bulbColorHex = bulbStatesBottom[i] ? pair.on : pair.off;
                var (r, g, b) = HexToRgb(bulbColorHex);
                bottomSb.Append($"\u001b[38;2;{r};{g};{b}m");
                bottomSb.Append('●');
                bottomSb.Append("\u001b[0m");
                if (i < renderBulbCount - 1)
                {
                    var (cr, cg, cb) = HexToRgb("#00A000");
                    bottomSb.Append($"\u001b[38;2;{cr};{cg};{cb}m───\u001b[0m");
                }
            }
            Console.Write(bottomSb.ToString());

            Console.Out.Flush();

            // Wait indefinitely (until user Ctrl+C)
            await Task.Delay(-1);
        }

        // Animation mode: render initial full block (top bulbs line, blank lines, message, blank lines, bottom bulbs line), then update bulbs in-place
        var renderCount = ComputeRenderBulbCount(desiredBulbCount);

        // Build top bulbs markup line
        var topBulbsLine = new StringBuilder();
        topBulbsLine.Append("  ");
        for (int i = 0; i < renderCount; i++)
        {
            var pair = colorPairs[i % colorPairs.Length];
            var bulbColor = bulbStatesTop[i] ? pair.on : pair.off;
            topBulbsLine.Append($"[{bulbColor}]●[/]");
            if (i < renderCount - 1)
            {
                topBulbsLine.Append(cord);
            }
        }

        // Build bottom bulbs markup line
        var bottomBulbsLine = new StringBuilder();
        bottomBulbsLine.Append("  ");
        for (int i = 0; i < renderCount; i++)
        {
            var pair = colorPairs[i % colorPairs.Length];
            var bulbColor = bulbStatesBottom[i] ? pair.on : pair.off;
            bottomBulbsLine.Append($"[{bulbColor}]●[/]");
            if (i < renderCount - 1)
            {
                bottomBulbsLine.Append(cord);
            }
        }

        // Write top bulbs line
        try { Console.SetCursorPosition(0, bulbRow); } catch { }
        AnsiConsole.Markup(topBulbsLine.ToString());

        // Write blank lines gap and then the days label
        for (int g = 0; g < gapLines; g++) Console.WriteLine();
        var messageRow = bulbRow + 1 + gapLines;

        // Render figlet message (days + until) at current cursor position (messageRow)
        var (initialDaysLine, initialUntilLine) = BuildDaysParts();
        // Load font if available (reuse same logic as above)
        FigletFont? runtimeFont = null;
        try
        {
            var fontPath = System.IO.Path.Combine(AppContext.BaseDirectory, "fonts", "christmas.flf");
            if (!System.IO.File.Exists(fontPath)) fontPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fonts", "christmas.flf");
            if (System.IO.File.Exists(fontPath)) runtimeFont = FigletFont.Load(fontPath);
        }
        catch { runtimeFont = null; }
        if (runtimeFont == null) runtimeFont = FigletFont.Default;
        WriteFigletWithHexColor(runtimeFont, initialDaysLine, "#FFD700");
        WriteFigletWithHexColor(runtimeFont, initialUntilLine, "#00A000");

        // Write gap lines and bottom bulbs; compute bottomBulbRow from current cursor so figlet height is handled correctly
        for (int g = 0; g < gapLines; g++) Console.WriteLine();
        var bottomBulbRow = 0;
        try { bottomBulbRow = Console.CursorTop; } catch { bottomBulbRow = messageRow + 1 + gapLines; }
        try { Console.SetCursorPosition(0, bottomBulbRow); } catch { }
        AnsiConsole.Markup(bottomBulbsLine.ToString());

        Console.Out.Flush();

        // Keep a copy of previous states for change detection
        var prevStatesTop = new bool[desiredBulbCount];
        var prevStatesBottom = new bool[desiredBulbCount];
        Array.Copy(bulbStatesTop, prevStatesTop, desiredBulbCount);
        Array.Copy(bulbStatesBottom, prevStatesBottom, desiredBulbCount);

        // Track last rendered days count to optionally force redraw if it changes (only once per day)
        var lastDaysCount = DaysUntilChristmas(DateTime.Now);

        while (true)
        {
            // Recompute render count in case terminal was resized
            var newRenderCount = ComputeRenderBulbCount(desiredBulbCount);
            if (newRenderCount != renderCount)
            {
                // Full re-render on resize: clear and redraw everything. Simpler and avoids leftover artifacts.
                renderCount = newRenderCount;
                Console.Clear();
                bulbRow = 0;

                // Top bulbs
                var topSb = new StringBuilder();
                topSb.Append("  ");
                for (int i = 0; i < renderCount; i++)
                {
                    var pair = colorPairs[i % colorPairs.Length];
                    var bulbColor = bulbStatesTop[i] ? pair.on : pair.off;
                    var (r, g, b) = HexToRgb(bulbColor);
                    topSb.Append($"\u001b[38;2;{r};{g};{b}m●\u001b[0m");
                    if (i < renderCount - 1)
                    {
                        var (cr, cg, cb) = HexToRgb("#00A000");
                        topSb.Append($"\u001b[38;2;{cr};{cg};{cb}m───\u001b[0m");
                    }
                }
                Console.SetCursorPosition(0, bulbRow);
                Console.Write(topSb.ToString());

                // Gap lines
                for (int g = 0; g < gapLines; g++) Console.WriteLine();

                // Figlet message
                var (daysLineNow, untilLineNow) = BuildDaysParts();
                // Use the same runtime font if available; default FigletFont.Default will be used by FigletText if null.
                FigletFont? reFont = null;
                try
                {
                    var fontPath = System.IO.Path.Combine(AppContext.BaseDirectory, "fonts", "christmas.flf");
                    if (!System.IO.File.Exists(fontPath)) fontPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fonts", "christmas.flf");
                    if (System.IO.File.Exists(fontPath)) reFont = FigletFont.Load(fontPath);
                }
                catch { reFont = null; }
                if (reFont == null) reFont = FigletFont.Default;
                WriteFigletWithHexColor(reFont, daysLineNow, "#FFD700");
                WriteFigletWithHexColor(reFont, untilLineNow, "#00A000");

                // Gap lines
                for (int g = 0; g < gapLines; g++) Console.WriteLine();

                // Bottom bulbs
                var bottomSb = new StringBuilder();
                bottomSb.Append("  ");
                for (int i = 0; i < renderCount; i++)
                {
                    var pair = colorPairs[i % colorPairs.Length];
                    var bulbColor = bulbStatesBottom[i] ? pair.on : pair.off;
                    var (r, g, b) = HexToRgb(bulbColor);
                    bottomSb.Append($"\u001b[38;2;{r};{g};{b}m●\u001b[0m");
                    if (i < renderCount - 1)
                    {
                        var (cr, cg, cb) = HexToRgb("#00A000");
                        bottomSb.Append($"\u001b[38;2;{cr};{cg};{cb}m───\u001b[0m");
                    }
                }
                Console.Write(bottomSb.ToString());

                Console.Out.Flush();
                Array.Copy(bulbStatesTop, prevStatesTop, desiredBulbCount);
                Array.Copy(bulbStatesBottom, prevStatesBottom, desiredBulbCount);
                lastDaysCount = DaysUntilChristmas(DateTime.Now);
            }

            // If the days label changed (new day), re-render message line
            if (DaysUntilChristmas(DateTime.Now) != lastDaysCount)
            {
                // Re-render the figlet message (clear and redraw whole block for simplicity)
                Console.Clear();
                bulbRow = 0;

                // Top bulbs
                var topSb = new StringBuilder();
                topSb.Append("  ");
                for (int i = 0; i < renderCount; i++)
                {
                    var pair = colorPairs[i % colorPairs.Length];
                    var bulbColor = bulbStatesTop[i] ? pair.on : pair.off;
                    topSb.Append($"[{bulbColor}]●[/]");
                    if (i < renderCount - 1)
                    {
                        topSb.Append(cord);
                    }
                }
                Console.SetCursorPosition(0, bulbRow);
                Console.Write(topSb.ToString());

                for (int g = 0; g < gapLines; g++) Console.WriteLine();

                var (daysLineNow, untilLineNow) = BuildDaysParts();
                // On day-change re-render also use bundled font if available
                FigletFont? dayChangeFont = null;
                try
                {
                    var fontPath = System.IO.Path.Combine(AppContext.BaseDirectory, "fonts", "christmas.flf");
                    if (!System.IO.File.Exists(fontPath)) fontPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fonts", "christmas.flf");
                    if (System.IO.File.Exists(fontPath)) dayChangeFont = FigletFont.Load(fontPath);
                }
                catch { dayChangeFont = null; }
                if (dayChangeFont == null) dayChangeFont = FigletFont.Default;
                WriteFigletWithHexColor(dayChangeFont, daysLineNow, "#FFD700");
                WriteFigletWithHexColor(dayChangeFont, untilLineNow, "#00A000");

                for (int g = 0; g < gapLines; g++) Console.WriteLine();

                // Bottom bulbs
                var bottomSb = new StringBuilder();
                bottomSb.Append("  ");
                for (int i = 0; i < renderCount; i++)
                {
                    var pair = colorPairs[i % colorPairs.Length];
                    var bulbColor = bulbStatesBottom[i] ? pair.on : pair.off;
                    bottomSb.Append($"[{bulbColor}]●[/]");
                    if (i < renderCount - 1)
                    {
                        bottomSb.Append(cord);
                    }
                }
                Console.Write(bottomSb.ToString());

                Console.Out.Flush();
                Array.Copy(bulbStatesTop, prevStatesTop, desiredBulbCount);
                Array.Copy(bulbStatesBottom, prevStatesBottom, desiredBulbCount);
                lastDaysCount = DaysUntilChristmas(DateTime.Now);
            }


            // Update each bulb state occasionally; when a bulb changes, write that single character in-place.
            for (int i = 0; i < renderCount; i++)
            {
                if (!allOn && rand.NextDouble() < changeProbabilityPerFrame)
                {
                    // top row toggle
                    bulbStatesTop[i] = !bulbStatesTop[i];

                    // If changed, write only that bulb at the correct column on the bulb row
                    if (bulbStatesTop[i] != prevStatesTop[i])
                    {
                        var pair = colorPairs[i % colorPairs.Length];
                        var bulbColorHex = bulbStatesTop[i] ? pair.on : pair.off;
                        var (r, g, b) = HexToRgb(bulbColorHex);

                        // Compute column for bulb: 2 leading spaces + i * 4
                        var col = 2 + i * 4;
                        try { Console.SetCursorPosition(col, bulbRow); } catch { try { Console.Write("\r"); } catch { } }

                        // Write 24-bit colored dot and reset color. Using Console.Write avoids Spectre clearing.
                        Console.Write($"\u001b[38;2;{r};{g};{b}m●\u001b[0m");
                        Console.Out.Flush();

                        prevStatesTop[i] = bulbStatesTop[i];
                    }
                }

                if (!allOn && rand.NextDouble() < changeProbabilityPerFrame)
                {
                    // bottom row toggle
                    bulbStatesBottom[i] = !bulbStatesBottom[i];

                    // If changed, write only that bulb at the correct column on the bottom bulb row
                    if (bulbStatesBottom[i] != prevStatesBottom[i])
                    {
                        var pair = colorPairs[i % colorPairs.Length];
                        var bulbColorHex = bulbStatesBottom[i] ? pair.on : pair.off;
                        var (r, g, b) = HexToRgb(bulbColorHex);

                        // Compute column for bulb: 2 leading spaces + i * 4
                        var col = 2 + i * 4;
                        try { Console.SetCursorPosition(col, bottomBulbRow); } catch { try { Console.Write("\r"); } catch { } }

                        // Write 24-bit colored dot and reset color. Using Console.Write avoids Spectre clearing.
                        Console.Write($"\u001b[38;2;{r};{g};{b}m●\u001b[0m");
                        Console.Out.Flush();

                        prevStatesBottom[i] = bulbStatesBottom[i];
                    }
                }
            }

            // Small delay between frames
            await Task.Delay(allOn ? 500 : 300);
        }
    }
}