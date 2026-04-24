namespace MathGame
{
   //
   // Class to display of game's mmenu and control user's choice
   //
   public class GameMenu
   {
      // Game's settings
      Settings _settings = new ();

      // Colors to simulate higlights in menu
      ConsoleColor _defaultForegroundColor   = Console.ForegroundColor;
      ConsoleColor _defaultBackgroundColor   = Console.BackgroundColor;
      ConsoleColor _highlightForegroundColor = ConsoleColor.Black;
      ConsoleColor _highlightBackgroundColor = ConsoleColor.White;

      // Display main menu
      public void Display ()
      {
         DisplayGameDescription ();
      }

      // Creates title of game
      private void DisplayGameDescription ()
      {
         Console.ForegroundColor = _defaultForegroundColor;
         Console.BackgroundColor = _defaultBackgroundColor;

         Console.Clear ();

         string indent = new (' ', 20);
         string line = new ('─', 38);

         Console.ForegroundColor = ConsoleColor.Yellow;

         Console.WriteLine ();
         Console.WriteLine ("{0}┌{1}┐", indent, line);
         Console.WriteLine ("{0}│{1}MathGame{2}│", indent, new string (' ', 15), new string (' ', 15));
         Console.WriteLine ("{0}│{1}Test your mathematical skills{2}│", indent, new string (' ', 5), new string (' ', 4));
         Console.WriteLine ("{0}└{1}┘\n", indent, line);

         Console.ForegroundColor = _defaultForegroundColor;
      }
   }
}
