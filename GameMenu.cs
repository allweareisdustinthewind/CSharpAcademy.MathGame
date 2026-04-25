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

      public MenuItem (string name, int posX = 0, int posY = 0, MenuItem ?prevItem = null, MenuItem ?nextItem = null, CallActionHandler? handler = null)
      {
         Name = name;
         PrevItem = prevItem;
         NextItem = nextItem;
         PosX = posX;
         PosY = posY;
         CallAction = handler;
      }

      // Connect actual item with a new item 
      public MenuItem ConnectWith (string name, int posX, int posY, CallActionHandler ?handler = null)
      {
         MenuItem item = new (name, posX, posY);
         item.PrevItem = this;
         NextItem = item;
         item.CallAction = handler;

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
      MenuItem?_startGameItem = null;
      MenuItem? _settingsItem = null;
      MenuItem? _showResultItem = null;
      MenuItem? _settingsBlockBegin = null;
      MenuItem? _settingsBlockEnd = null;
      MenuItem? _curItem = null;

      int _posShowResult = 0;

      bool isSettingsShown = false;

      // Display main menu
      void Display ()
      {
         DisplayGameDescription ();
         
         MenuItem ?curItem = _startGameItem;
         do
         {
            Console.SetCursorPosition (curItem.PosX, curItem.PosY);
            Console.Write (curItem.Name);
            curItem = curItem.NextItem;
         }
         while (curItem != _startGameItem);

         _startGameItem?.SetState (MenuItem.State.Highlighted);
         _curItem = _startGameItem;
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
         if (_startGameItem != null)
            return;

         // Offset of standard menu items from left border of console window
         const int offsetMainMenu = 8;
         const int offsetSettingsMenu = 4;

         var (x, y) = Console.GetCursorPosition ();
         x += offsetMainMenu;

         _startGameItem = new ("Start game", x, y, handler: ActionStartGame);

         _settingsItem   = _startGameItem.ConnectWith ("Settings", x, ++y, ActionSettings);
         _showResultItem = _settingsItem.ConnectWith ("Show all results", x, ++y, ActionShowResults);
         _posShowResult = y;

         var exitItem    = _showResultItem.ConnectWith ("Exit", x, ++y, ActionExit);

         // Connect item "Exit" with "Start game" to make a loop by selecting
         exitItem.NextItem = _startGameItem;
         _startGameItem.PrevItem = exitItem;

         int posX = x + offsetSettingsMenu;
         int posY = _posShowResult;

         _settingsBlockBegin = new ("Difficulty: ", posX, posY);
         _settingsBlockBegin.PrevItem = _settingsItem;

         var curItem = _settingsBlockBegin;
         curItem = curItem.ConnectWith ("easy  ",   posX += curItem.Name.Length, posY, ActionChangeDifficulty);
         curItem = curItem.ConnectWith ("normal  ", posX += curItem.Name.Length, posY, ActionChangeDifficulty);
         curItem = curItem.ConnectWith ("hard  ",   posX += curItem.Name.Length, posY, ActionChangeDifficulty);

         posX = x + offsetSettingsMenu;
         MenuItem itemMode = new ("Operation will be set: ", posX, ++posY);
         itemMode.PrevItem = curItem;
         curItem.NextItem = itemMode;

         itemMode = itemMode.ConnectWith ("randomly  ",     posX += itemMode.Name.Length, posY, ActionChangeMode);
         itemMode = itemMode.ConnectWith ("as following:  ", posX += itemMode.Name.Length, posY, ActionChangeMode);

         MenuItem itemOper = new ("+ : addition", posX, ++posY, handler: ActionChangeOperation);
         itemOper.PrevItem = itemMode;
         itemMode.NextItem = itemOper;

         itemOper = itemOper.ConnectWith ("- : subtraction",    posX, ++posY, ActionChangeOperation);
         itemOper = itemOper.ConnectWith ("x : multiplication", posX, ++posY, ActionChangeOperation);
         itemOper = itemOper.ConnectWith ("/ : division",       posX, ++posY, ActionChangeOperation);

         _settingsBlockEnd = itemOper;
         _settingsBlockEnd.NextItem = _showResultItem;
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
         isSettingsShown = !isSettingsShown;
         if (isSettingsShown)
         {
            ShiftMenuResult (_settingsBlockEnd.PosY + 1);

            _settingsItem.NextItem = _settingsBlockBegin;
            _showResultItem.PrevItem = _settingsBlockEnd;
         }
         else
         {
            ShiftMenuResult (_posShowResult);

            _settingsItem.NextItem = _showResultItem;
            _showResultItem.PrevItem = _settingsItem;
         }

         Display ();

         return false;
      }

      void ShiftMenuResult (int offset)
      {
         var item = _showResultItem;
         do
         {
            item.PosY = offset++;
            item = item.NextItem;
         }
         while (item != _startGameItem);
      }

      bool ActionChangeDifficulty ()
      {
         return false;
      }

      bool ActionChangeMode ()
      {
         return false;
      }

      bool ActionChangeOperation ()
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

      public void Run ()
      {
         Display ();

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
