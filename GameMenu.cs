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

      public MenuItem GetNextSelectable ()
      {
         MenuItem item = NextItem;
         while (item != null && item.CallAction == null)
            item = item.NextItem;

         return item;
      }

      public MenuItem GetPrevSelectable ()
      {
         MenuItem item = PrevItem;
         while (item != null && item.CallAction == null)
            item = item.PrevItem;

         return item;
      }

      public void Activate (bool selectOnScreen = true)
      {
         if (Name.Contains ("(*)"))
            return;

         Name = Name.Replace ("( )", "(*)");

         Console.SetCursorPosition (PosX, PosY);
         if (selectOnScreen)
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

      public void Deactivate ()
      {
         if (Name.Contains ("( )"))
            return;

         Name = Name.Replace ("(*)", "( )");

         Console.BackgroundColor = _defaultBackgroundColor;
         Console.ForegroundColor = _defaultForegroundColor;

         Console.SetCursorPosition (PosX, PosY);
         Console.Write (Name);
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
      
      MenuItem? _itemEasy = null;
      MenuItem? _itemNormal = null;
      MenuItem? _itemHard = null;

      MenuItem? _itemModeRandom = null;
      MenuItem? _itemModeFix = null;

      MenuItem? _itemOpAdd = null;
      MenuItem? _itemOpSub = null;
      MenuItem? _itemOpMul = null;
      MenuItem? _itemOpDiv = null;

      MenuItem? _curItem = null;

      int _posShowResult = 0;

      bool _isSettingsShown = false;
      GameLogic _logic = null;

      public GameMenu ()
      {
         _logic = new (_settings);
      }

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

         _curItem?.SetState (MenuItem.State.Highlighted);
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

         _itemEasy   = _settingsBlockBegin.ConnectWith ("(*) easy ", posX += _settingsBlockBegin.Name.Length, posY, ActionChangeDifficulty);
         _itemNormal = _itemEasy.ConnectWith ("( ) normal ", posX += _itemEasy.Name.Length, posY, ActionChangeDifficulty);
         _itemHard   = _itemNormal.ConnectWith ("( ) hard ",   posX += _itemNormal.Name.Length, posY, ActionChangeDifficulty);

         posX = x + offsetSettingsMenu;
         MenuItem itemMode = new ("Operation will be set: ", posX, ++posY);
         itemMode.PrevItem = _itemHard;
         _itemHard.NextItem = itemMode;

         _itemModeRandom = itemMode.ConnectWith ("(*) randomly ", posX += itemMode.Name.Length, posY, ActionChangeMode);
         _itemModeFix = _itemModeRandom.ConnectWith ("( ) as following:", posX += _itemModeRandom.Name.Length, posY);

         _itemOpAdd = new ("( ) addition", posX, ++posY, handler: ActionChangeOperation);
         _itemOpAdd.PrevItem = _itemModeFix;
         _itemModeFix.NextItem = _itemOpAdd;

         _itemOpSub = _itemOpAdd.ConnectWith ("( ) subtraction",    posX, ++posY, ActionChangeOperation);
         _itemOpMul = _itemOpSub.ConnectWith ("( ) multiplication", posX, ++posY, ActionChangeOperation);
         _itemOpDiv = _itemOpMul.ConnectWith ("( ) division",       posX, ++posY, ActionChangeOperation);

         _settingsBlockEnd = _itemOpDiv;
         _settingsBlockEnd.NextItem = _showResultItem;

         _curItem = _startGameItem;
      }

      void SelectItem (MenuItem ?item)
      {
         _curItem?.SetState (MenuItem.State.Normal);
         _curItem = item;
         _curItem?.SetState (MenuItem.State.Highlighted);
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

      void DeactivateItems (MenuItem [] items)
      { 
         foreach (var item in items)
            item.Deactivate ();
      }

      bool ActionStartGame ()
      {
         _logic?.PlayRound ();

         _curItem = _startGameItem;

         if (_isSettingsShown)
            ActionSettings ();
         else
            Display ();

         return false;
      }

      bool ActionSettings ()
      {
         _isSettingsShown = !_isSettingsShown;
         if (_isSettingsShown)
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

         _curItem = _settingsItem;

         Display ();

         return false;
      }

      bool ActionChangeDifficulty ()
      {
         if (_curItem == _itemEasy)
         {
            DeactivateItems ([_itemNormal, _itemHard]);
            _settings.Difficulty = Settings.Level.Easy;
         }
         else if (_curItem == _itemNormal)
         {
            DeactivateItems ([_itemEasy, _itemHard]);
            _settings.Difficulty = Settings.Level.Normal;
         }
         else
         {
            DeactivateItems ([_itemEasy, _itemNormal]);
            _settings.Difficulty = Settings.Level.Hard;
         }

         _curItem.Activate ();

         return false;
      }

      bool ActionChangeMode ()
      {
         DeactivateItems ([_itemModeFix, _itemOpAdd, _itemOpSub, _itemOpMul, _itemOpDiv]);
         _curItem.Activate ();
         _settings.Operation = "@";

         return false;
      }

      bool ActionChangeOperation ()
      {
         if (_curItem == _itemOpAdd)
         {
            DeactivateItems ([_itemModeRandom, _itemOpSub, _itemOpMul, _itemOpDiv]);
            _settings.Operation = "+";
         }
         else if (_curItem == _itemOpSub)
         {
            DeactivateItems ([_itemModeRandom, _itemOpAdd, _itemOpMul, _itemOpDiv]);
            _settings.Operation = "-";
         }
         else if (_curItem == _itemOpMul)
         {
            DeactivateItems ([_itemModeRandom, _itemOpAdd, _itemOpSub, _itemOpDiv]);
            _settings.Operation = "x";
         }
         else
         {
            DeactivateItems ([_itemModeRandom, _itemOpAdd, _itemOpSub, _itemOpMul]);
            _settings.Operation = "/";
         }

         _curItem.Activate ();
         _itemModeFix.Activate (false /*selectOnScreen*/);

         return false;
      }

      bool ActionShowResults ()
      {
         _logic?.ShowResults ();

         _curItem = _startGameItem;

         if (_isSettingsShown)
            ActionSettings ();
         else
            Display ();

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
               case ConsoleKey.LeftArrow:
                  SelectItem (_curItem?.GetPrevSelectable ());
                  break;

               case ConsoleKey.DownArrow:
               case ConsoleKey.RightArrow:
                  SelectItem (_curItem?.GetNextSelectable ());
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
