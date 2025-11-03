using System;
using System.Text;
using Spectre.Console;
using System.Threading.Tasks;

namespace test;

static class Program
{
    static async Task Main(string[] args)
    {
        // Usage: dotnet run -- --all-on   (or -a)
        var allOn = false;
        foreach (var a in args)
        {
            if (a == "--all-on" || a == "-a")
            {
                allOn = true;
                break;
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
        var bulbStates = new bool[desiredBulbCount];

        // Initialize bulbs randomly (or all on if flag set)
        for (int i = 0; i < desiredBulbCount; i++)
        {
            bulbStates[i] = allOn ? true : rand.NextDouble() > 0.5;
        }

        // Hide the cursor for smoother display and restore it when the user cancels (Ctrl+C)
        Console.CancelKeyPress += (_, _) => Console.CursorVisible = true;
        try { Console.CursorVisible = false; } catch { /* best-effort */ }

        // Helper: convert hex color (#RRGGBB) to RGB tuple
        static (int r, int g, int b) HexToRgb(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return (r, g, b);
        }

        // If all-on is requested, render once and don't continuously clear/redraw
        if (allOn)
        {
            var sbAnsi = new StringBuilder();
            sbAnsi.Append("  ");
            var renderBulbCount = ComputeRenderBulbCount(desiredBulbCount);
            for (int i = 0; i < renderBulbCount; i++)
            {
                var pair = colorPairs[i % colorPairs.Length];
                var bulbColorHex = bulbStates[i] ? pair.on : pair.off;
                var (r, g, b) = HexToRgb(bulbColorHex);
                // 24-bit foreground color
                sbAnsi.Append($"\u001b[38;2;{r};{g};{b}m");
                sbAnsi.Append('●');
                sbAnsi.Append("\u001b[0m");

                if (i < renderBulbCount - 1)
                {
                    var (cr, cg, cb) = HexToRgb("#00A000");
                    sbAnsi.Append($"\u001b[38;2;{cr};{cg};{cb}m");
                    sbAnsi.Append("───");
                    sbAnsi.Append("\u001b[0m");
                }
            }

            // Pad to avoid leftover characters from previous content when possible
            try
            {
                var width = Console.WindowWidth;
                var expectedLength = Math.Max(0, 2 + renderBulbCount + Math.Max(0, (renderBulbCount - 1) * 3));
                if (width > expectedLength)
                {
                    sbAnsi.Append(new string(' ', width - expectedLength));
                }

                // Position at start of current line
                try { Console.SetCursorPosition(0, Console.CursorTop); } catch { /* best-effort */ }
            }
            catch
            {
                try { Console.Write("\r"); } catch { /* best-effort */ }
            }

            // Single write of raw ANSI sequence
            Console.Write(sbAnsi.ToString());
            Console.Out.Flush();

            // Wait indefinitely (until user Ctrl+C) without redrawing to prevent flicker
            await Task.Delay(-1);
        }

        // Animation mode: render initial full line then update only changed bulbs in-place to minimize flicker.
        var renderCount = ComputeRenderBulbCount(desiredBulbCount);

        // Render initial full markup line
        var initialMarkup = new StringBuilder();
        initialMarkup.Append("  ");
        for (int i = 0; i < renderCount; i++)
        {
            var pair = colorPairs[i % colorPairs.Length];
            var bulbColor = bulbStates[i] ? pair.on : pair.off;
            initialMarkup.Append($"[{bulbColor}]●[/]");
            if (i < renderCount - 1)
            {
                initialMarkup.Append(cord);
            }
        }

        // Pad to clear leftovers
        try
        {
            var width = Console.WindowWidth;
            var visibleLen = Math.Max(0, 2 + renderCount + Math.Max(0, (renderCount - 1) * 3));
            if (width > visibleLen) initialMarkup.Append(new string(' ', width - visibleLen));
        }
        catch { }

        // Write once
        AnsiConsole.Markup(initialMarkup.ToString());
        Console.Out.Flush();

        // Keep a copy of previous states for change detection
        var prevStates = new bool[desiredBulbCount];
        Array.Copy(bulbStates, prevStates, desiredBulbCount);

        while (true)
        {
            // Recompute render count in case terminal was resized
            var newRenderCount = ComputeRenderBulbCount(desiredBulbCount);
            if (newRenderCount != renderCount)
            {
                // Re-render full line on resize
                renderCount = newRenderCount;
                var full = new StringBuilder();
                full.Append("  ");
                for (int i = 0; i < renderCount; i++)
                {
                    var pair = colorPairs[i % colorPairs.Length];
                    var bulbColor = bulbStates[i] ? pair.on : pair.off;
                    full.Append($"[{bulbColor}]●[/]");
                    if (i < renderCount - 1) full.Append(cord);
                }

                try
                {
                    var width = Console.WindowWidth;
                    var visibleLen = Math.Max(0, 2 + renderCount + Math.Max(0, (renderCount - 1) * 3));
                    if (width > visibleLen) full.Append(new string(' ', width - visibleLen));
                }
                catch { }

                try { Console.SetCursorPosition(0, Console.CursorTop); } catch { }
                AnsiConsole.Markup(full.ToString());
                Console.Out.Flush();
                Array.Copy(bulbStates, prevStates, desiredBulbCount);
            }

            // Update each bulb state occasionally; when a bulb changes, write that single character in-place.
            for (int i = 0; i < renderCount; i++)
            {
                if (!allOn && rand.NextDouble() < changeProbabilityPerFrame)
                {
                    bulbStates[i] = !bulbStates[i];

                    // If changed, write only that bulb at the correct column
                    if (bulbStates[i] != prevStates[i])
                    {
                        var pair = colorPairs[i % colorPairs.Length];
                        var bulbColorHex = bulbStates[i] ? pair.on : pair.off;
                        var (r, g, b) = HexToRgb(bulbColorHex);

                        // Compute column for bulb: 2 leading spaces + i * 4
                        var col = 2 + i * 4;
                        try { Console.SetCursorPosition(col, Console.CursorTop); } catch { try { Console.Write("\r"); } catch { } }

                        // Write 24-bit colored dot and reset color. Using Console.Write avoids Spectre clearing.
                        Console.Write($"\u001b[38;2;{r};{g};{b}m●\u001b[0m");
                        Console.Out.Flush();

                        prevStates[i] = bulbStates[i];
                    }
                }
            }

            // Small delay between frames
            await Task.Delay(allOn ? 500 : 300);
        }
    }
}