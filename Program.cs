using MathGame;

//// -------------------------------------------------------
////          Global constants and variables   
//// -------------------------------------------------------

//// Position of higlighted menu's element
//int menuPosX = 0;
//int menuPosY = 0;

//// Position of highlighted item im menu "Change difficulty"
//int menuDifficultyPosX = 0;

//// Items of main menu
//string [] menuItems   = ["Start game", "Change difficulty (currently: easy)", "Show all results", "Exit"];

//// Itemd of main menu + names of difficulty levels
//string [] menuItemsEx = ["Start game", "Change difficulty (currently: easy)", "easy", "normal", "hard", "Show all results", "Exit"];

//// Index of currently and previously selected menu items 
//int activeMenuItem = 0;
//int prevMenuItem   = -1;

//// Actual level of difficulty: 0 - easy, 1 - normal, 2 - hard
//Levels difficulty = Levels.easy;

//// Number of game round
//int gameNumber = 0;

//// Time spent in actual round
//int totalSeconds = 0;
//int totalMinutes = 0;
//int totalHours   = 0;

//// True if menu to change level of difficulty activ, false otherweise
//bool inMenuDifficulty = false;

//// Offset of menu items from left border of console window
//const string indentMenuItem = "        ";

////
//// Exit from application and say "farewell"
////
//void Exit ()
//{
//   Console.Clear ();
//   Console.WriteLine ("\n\n       Good bye!");
//   Console.CursorVisible = true;
//}


////
//// Display results all game rounds
////
//void ShowAllResults ()
//{
//   Console.ForegroundColor = defaultForegroundColor;
//   Console.BackgroundColor = defaultBackgroundColor;
//   Console.Clear ();

//   if (results.Count <= 0)
//      Console.WriteLine ("\n Actual there is no results");
//   else
//   {
//      foreach (var res in results)
//         res.Display ();
//   }

//   Console.WriteLine ("\n Press any key to go to menu...");
//   Console.ReadKey ();

//   ShowGui ();
//}



GameMenu menu = new ();
menu.Run ();

