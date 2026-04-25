using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MathGame
{
   //
   // Single item of game menu
   // 
   internal class MenuItem
   {
      // Display name without indention
      public string Name { get; set; }

      // Previous element in menu
      public MenuItem ?PrevItem { get; set; }

      // Next element in menu
      public MenuItem ?NextItem { get; set; }

      // Position of item im console
      public int PosX { get; set; }

      public int PosY { get; set; }

      public delegate bool CallActionHandler ();
      public CallActionHandler CallAction;

      // Colors to simulate higlights in menu
      static ConsoleColor _defaultForegroundColor   = Console.ForegroundColor;
      static ConsoleColor _defaultBackgroundColor   = Console.BackgroundColor;
      static ConsoleColor _highlightForegroundColor = ConsoleColor.Black;
      static ConsoleColor _highlightBackgroundColor = ConsoleColor.White;

      public enum State
      { 
         Highlighted,
         Normal
      }

      public MenuItem (string name, int posX = 0, int posY = 0, MenuItem ?prevItem = null, MenuItem ?nextItem = null)
      {
         Name = name;
         PrevItem = prevItem;
         NextItem = nextItem;
         PosX = posX;
         PosY = posY;
      }

      // Connect actual item with a new item 
      public MenuItem ConnectWith (string name, int posX, int posY)
      {
         MenuItem item = new (name, posX, posY);
         item.PrevItem = this;
         NextItem = item;

         return item;
      }

      // Change state of item - highlight / normal
      public void SetState (State state)
      {
         Console.SetCursorPosition (PosX, PosY);

         if (state == State.Highlighted)
         {
            Console.BackgroundColor = _highlightBackgroundColor;
            Console.ForegroundColor = _highlightForegroundColor;
            Console.Write (Name);

            Console.BackgroundColor = _defaultBackgroundColor;
            Console.ForegroundColor = _defaultForegroundColor;
         }
         else
         {
            Console.BackgroundColor = _defaultBackgroundColor;
            Console.ForegroundColor = _defaultForegroundColor;
            Console.Write (Name);
         }
      }
   }

   //
   // Class to display of game's mmenu and control user's choice
   //
   public class GameMenu
   {
      // Game's settings
      Settings _settings = new ();

      // Root of menu
      MenuItem ?_root = null;

      MenuItem? _curItem = null;

      // Display main menu
      public void Display ()
      {
         DisplayGameDescription ();
         
         MenuItem ?curItem = _root;
         do
         {
            Console.SetCursorPosition (curItem.PosX, curItem.PosY);
            Console.Write (curItem.Name);
            curItem = curItem.NextItem;
         }
         while (curItem != _root);

         _root?.SetState (MenuItem.State.Highlighted);
         _curItem = _root;

         MenuLoop ();
      }

      // Creates title of game
      private void DisplayGameDescription ()
      {
         Console.Clear ();

         string indent = new (' ', 20);
         string line = new ('─', 38);

         ConsoleColor color = Console.ForegroundColor;
         Console.ForegroundColor = ConsoleColor.Yellow;

         Console.WriteLine ();
         Console.WriteLine ("{0}┌{1}┐", indent, line);
         Console.WriteLine ("{0}│{1}MathGame{2}│", indent, new string (' ', 15), new string (' ', 15));
         Console.WriteLine ("{0}│{1}Test your mathematical skills{2}│", indent, new string (' ', 5), new string (' ', 4));
         Console.WriteLine ("{0}└{1}┘\n", indent, line);

         Console.ForegroundColor = color;

         CreateMenuStructure ();
      }

      // Fill internal data with names and positions of all menu items
      void CreateMenuStructure ()
      {
         if (_root != null)
            return;

         // Offset of standard menu items from left border of console window
         const int offsetMenu = 8;

         var (x, y) = Console.GetCursorPosition ();
         x += offsetMenu;

         _root = new ("Start game", x, y);
         _root.CallAction = ActionStartGame;

         Tuple <string, MenuItem.CallActionHandler> [] items = 
         [
            new Tuple <string, MenuItem.CallActionHandler> ("Settings", ActionSettings),
            new Tuple <string, MenuItem.CallActionHandler> ("Show all results", ActionShowResults),
            new Tuple <string, MenuItem.CallActionHandler> ("Exit", ActionExit)
         ];

         MenuItem curItem = _root;
         foreach (var (item, action) in items)
         {
            curItem = curItem.ConnectWith (item, x, ++y);
            curItem.CallAction = action;
         }

         // Connect item "Exit" with "Start game" to make a loop by selecting
         curItem.NextItem = _root;
         _root.PrevItem = curItem;
      }

      void SelectItem (MenuItem ?item)
      {
         _curItem?.SetState (MenuItem.State.Normal);
         _curItem = item;
         _curItem?.SetState (MenuItem.State.Highlighted);
      }

      bool ActionStartGame ()
      {
         return false;
      }

      bool ActionSettings ()
      {
         return false;
      }

      bool ActionShowResults ()
      {
         return false;
      }

      bool ActionExit ()
      {
         return true;
      }

      void MenuLoop ()
      {
         Console.CursorVisible = false;

         bool exitGame = false;
         while (!exitGame)
         {
            var key = Console.ReadKey (true).Key;
            switch (key)
            {
               case ConsoleKey.UpArrow:
                  SelectItem (_curItem?.PrevItem);
                  break;

               case ConsoleKey.DownArrow:
                  SelectItem (_curItem?.NextItem);
                  break;

               case ConsoleKey.Escape:
                  exitGame = true;
                  break;

               case ConsoleKey.Enter:
               case ConsoleKey.Spacebar:
                  exitGame = _curItem?.CallAction () ?? false;
                  break;
            }
         }
      }
   }
}
