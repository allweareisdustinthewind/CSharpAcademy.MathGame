namespace MathGame
{
   public class GameLogic
   {
      Settings _settings = null;
      int _gameNumber = 0;
      int _totalSeconds = 0;
      int _totalMinutes = 0;
      int _totalHours = 0;

      ConsoleColor _defaultForegroundColor = Console.ForegroundColor;
      ConsoleColor _defaultBackgroundColor = Console.BackgroundColor;

      //// List of all results of game
      List<GameResult> _results = new ();

      Random _random = new ();
      System.Threading.Timer ?_timer = null;

      bool _pauseInTimer = false;

      HashSet<string> _questions = new ();

      public GameLogic (Settings settings)
      {
         _settings = settings;
      }

      public void PlayRound ()
      {
         if (_settings == null)
            return;

         _questions.Clear ();
         
         // Start with blank window
         Console.Clear ();

         int correctAnswers = 0;
         int wrongAnswers = 0;

         // Contains complete state of game round
         GameResult info = new (++_gameNumber, _settings.Difficulty);

         _totalSeconds = 0;
         _totalMinutes = 0;
         _totalHours = 0;

         for (int i = 0; i < Settings.MaxQuestions; ++i)
         {
            UpdateStatistic (i + 1, Settings.MaxQuestions, correctAnswers, wrongAnswers);

            string question = GenerateQuestion (out int result);
            string text = $"  {i + 1}.  {question}";
            Console.Write (text);

            if (_timer == null)
               _timer = new Timer (TimerCallback, null, 0, 1000);

            int answer = GetAnswer ();
            if (answer != result)
            {
               ++wrongAnswers;
               ReportWrongAnswer (result);
            }
            else
            {
               ++correctAnswers;
               ReportCorrectAnswer ();
            }

            info.AddResult (question, result, answer);
         }

         _timer?.Dispose ();
         _timer = null;

         info.EndGame ();
         _results.Add (info);

         UpdateStatistic (Settings.MaxQuestions, Settings.MaxQuestions, correctAnswers, wrongAnswers);

         Console.WriteLine ("\nPress any key to go to menu...");
         Console.ReadKey ();
      }

      //
      // Update count of questions with correct and wrong answers after receiving antwort from user
      //
      void UpdateStatistic (int curQuestion, int maxQuestions, int correctAnswers = 0, int wrongAnswers = 0)
      {
         var (x, y) = Console.GetCursorPosition ();
         Console.SetCursorPosition (0, 0);

         string indent = new (' ', 5);

         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine ($"\n               Question {curQuestion} of {maxQuestions}\n");

         Console.ForegroundColor = ConsoleColor.Green;
         Console.Write ($"{indent}Correct answers: {correctAnswers}   ");

         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine ($"Wrong answers: {wrongAnswers}");

         Console.ForegroundColor = ConsoleColor.Yellow;
         string line = new ('─', 37);
         Console.WriteLine ($"{indent}{line}\n");

         Console.ForegroundColor = _defaultForegroundColor;

         // For the first question we don't restore previous position in console, because after updating statistic
         // will be launched timer. Position of timer will be dealed separat, therefore actual position in console must not be changed 
         if (curQuestion > 1)
            Console.SetCursorPosition (x, y);
      }

      //
      // Updates timer in console. Will be launched by starting a new game in 1-second taсt
      //
      void TimerCallback (Object? _)
      {
         if (_pauseInTimer)
            return;

         ++_totalSeconds;

         // After each 60 seconds uppdate minutes and hours
         if (_totalSeconds > 59)
         {
            _totalSeconds = 0;
            ++_totalMinutes;

            if (_totalMinutes > 59)
               ++_totalHours;
         }

         string time = $"Time: {_totalHours:D2}:{_totalMinutes:D2}:{_totalSeconds:D2}";
         var (x, y) = Console.GetCursorPosition ();

         Console.CursorVisible = false;
         Console.SetCursorPosition (50, 1);

         var colorForeground = Console.ForegroundColor;
         var colorBackground = Console.BackgroundColor;

         Console.ForegroundColor = _defaultForegroundColor;
         Console.BackgroundColor = _defaultBackgroundColor;

         Console.Write (time);

         Console.ForegroundColor = colorForeground;
         Console.BackgroundColor = colorBackground;

         Console.SetCursorPosition (x, y);
         Console.CursorVisible = true;
      }

      //
      // Report, that answer is wrong
      //
      void ReportWrongAnswer (int correctAnswer)
      {
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine ($"      Answer is wrong. Correct answer: {correctAnswer}\n");
         Console.ForegroundColor = _defaultForegroundColor;
      }

      //
      // Report, that answer is correct
      //
      void ReportCorrectAnswer ()
      {
         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine ("      Answer is correct\n");
         Console.ForegroundColor = _defaultForegroundColor;
      }

      //
      // Received input from user and analyzes it for correctness
      //
      int GetAnswer ()
      {
         int val = 0;
         string error = string.Empty;

         var (posX, posY) = Console.GetCursorPosition ();

         for (; ; )
         {
            Console.CursorVisible = true;
            string? answer = Console.ReadLine ();

            // Not a number or negative number
            if (answer == null || !int.TryParse (answer, out val) || val < 0)
            {
               _pauseInTimer = true;

               if (val < 0)
                  error = " Number must be greater or equal zero ";
               else
                  error = " Incorrect format of input ";

               Console.ForegroundColor = ConsoleColor.White;
               Console.BackgroundColor = ConsoleColor.Red;
               Console.Write (error);
               Console.CursorVisible = false;

               System.Threading.Thread.Sleep (2000);
               var (_, y) = Console.GetCursorPosition ();
               Console.SetCursorPosition (0, y);

               Console.ForegroundColor = _defaultForegroundColor;
               Console.BackgroundColor = _defaultBackgroundColor;
               Console.Write (new string (' ', error.Length));

               Console.SetCursorPosition (posX, posY);
               if (answer != null)
               {
                  Console.Write (new string (' ', answer.Length));
                  Console.SetCursorPosition (posX, posY);
               }

               _pauseInTimer = false;

               continue;
            }

            break;
         }

         Console.CursorVisible = false;
         return val;
      }

      //
      // Create two operands for operation '+'
      //
      void GenerateOperandsForAddition (out int op1, out int op2)
      {
         op1 = 0;
         op2 = 0;

         switch (_settings.Difficulty)
         {
            case Settings.Level.Easy:
               op1 = _random.Next (1, 10);
               op2 = _random.Next (1, 10);
               break;

            case Settings.Level.Normal:
               op1 = _random.Next (10, 30);
               op2 = _random.Next (10, 30);
               break;

            case Settings.Level.Hard:
               op1 = _random.Next (50, 100);
               op2 = _random.Next (50, 100);
               break;
         }
      }

      //
      // Create two operands for operation '-'
      //
      void GenerateOperandsForSubtraction (out int op1, out int op2)
      {
         op1 = 0;
         op2 = 0;

         int from = 0;
         int to = 0;
         int minDifference = 0;

         switch (_settings.Difficulty)
         {
            case Settings.Level.Easy:
               from = 1;
               to = 10;
               minDifference = 1;
               break;

            case Settings.Level.Normal:
               from = 10;
               to = 30;
               minDifference = 7;
               break;

            case Settings.Level.Hard:
               from = 40;
               to = 150;
               minDifference = 13;
               break;
         }

         // Ensure, that op2 < op1 (prevent from getting negative results).
         // To make the game a bit more challlenged ensure, that two operands are not too near
         for (; ; )
         {
            op1 = _random.Next (from, to + 1);
            op2 = _random.Next (from, to + 1);
            if (op2 < op1 && op1 - op2 >= minDifference)
               break;
         }
      }

      //
      // Create two operands for operation 'x'
      //
      void GenerateOperandsForMultiplication (out int op1, out int op2)
      {
         op1 = 0;
         op2 = 0;

         int from = 0;
         int to = 0;

         switch (_settings.Difficulty)
         {
            case Settings.Level.Easy:
               from = 2;
               to = 10;
               break;

            case Settings.Level.Normal:
               from = 10;
               to = 21;
               break;

            case Settings.Level.Hard:
               from = 30;
               to = 51;
               break;
         }

         op1 = _random.Next (from, to);
         op2 = _random.Next (from, to);
      }

      //
      // Create two operands for operation '/'
      //
      void GenerateOperandsForDivision (out int op1, out int op2)
      {
         op1 = 0;
         op2 = 0;

         int from = 0;
         int to = 0;

         switch (_settings.Difficulty)
         {
            case Settings.Level.Easy:
               from = 4;
               to = 101;
               break;

            case Settings.Level.Normal:
               from = 51;
               to = 401;
               break;

            case Settings.Level.Hard:
               from = 101;
               to = 1001;
               break;
         }

         // Ensure, that opt1 is not a prime number, otherwise there is not a sufficient solution
         op1 = 3;
         while (IsPrimeNumber (op1))
         {
            op1 = _random.Next (from, to);
         }

         // Search for op2 suuch that op1 / op2 without rest
         for (; ; )
         {
            op2 = _random.Next (2, op1);
            if ((op1 % op2) == 0)
               break;
         }
      }

      //
      // True if value is a prime number
      //
      bool IsPrimeNumber (int val)
      {
         if (val <= 1)
            return false;

         if (val == 2)
            return true;

         int maxVal = (int) Math.Sqrt (val);
         for (int i = 2; i <= maxVal; ++i)
         {
            if (val % i == 0)
               return false;
         }

         return true;
      }

      //
      // Creates random mathematical question 
      //
      string GenerateQuestion (out int result)
      {
         result = 0;

         int operation = 0;
         if (_settings.Operation == "@")
            operation = _random.Next (0, 4);
         else operation = _settings.Operation switch
         {
            "+" => 0,
            "-" => 1,
            "x" => 2,
            _ => 3
         };

         for (; ; )
         {
            int op1 = 0;
            int op2 = 0;

            string operationSymbol = "";

            switch (operation)
            {
               case 0: // Operation '+'
                  GenerateOperandsForAddition (out op1, out op2);
                  operationSymbol = "+";
                  result = op1 + op2;
                  break;

               case 1: // Operation '-'
                  GenerateOperandsForSubtraction (out op1, out op2);
                  operationSymbol = "-";
                  result = op1 - op2;
                  break;

               case 2: // Operation 'x'
                  GenerateOperandsForMultiplication (out op1, out op2);
                  operationSymbol = "\u00d7";
                  result = op1 * op2;
                  break;

               case 3: // Operation '/'
                  GenerateOperandsForDivision (out op1, out op2);
                  operationSymbol = "/";
                  result = op1 / op2;
                  break;
            }

            string question = $"{op1} {operationSymbol} {op2} = ";
            if (_questions.Contains (question))
               continue;

            _questions.Add (question);
            return question;
         }
      }

      //
      // Display results all game rounds
      //
      public void ShowResults ()
      {
         Console.ForegroundColor = _defaultForegroundColor;
         Console.BackgroundColor = _defaultBackgroundColor;
         Console.Clear ();

         if (_results.Count <= 0)
            Console.WriteLine ("\n Actual there is no results");
         else
         {
            foreach (var res in _results)
               res.Display ();
         }

         Console.WriteLine ("\n Press any key to go to menu...");
         Console.ReadKey ();
      }
   }
}
