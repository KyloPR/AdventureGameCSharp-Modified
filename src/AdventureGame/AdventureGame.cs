using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventureGame;

public class AdventureGame
{
    // ============================
    // CONSTANTS
    // ============================

    public readonly string GO_NORTH = "W";
    public readonly string GO_SOUTH = "S";
    public readonly string GO_EAST  = "D";
    public readonly string GO_WEST  = "A";
    public readonly string GET_LAMP = "L";
    public readonly string GET_KEY  = "K";
    public readonly string OPEN_CHEST = "O";
    public readonly string QUIT = "Q";

    // ============================
    // GAME STATE
    // ============================

    private Adventurer adventurer;
    private Room[,] dungeon;
    private readonly Random random = new Random();
    private int aRow;
    private int aCol;
    private int enemyRow;
    private int enemyCol;
    private int exitRow;
    private int exitCol;
    private bool useLargeMap;
    private bool isChestOpen;
    private bool hasPlayerQuit;
    private bool isAdventureAlive;
    private bool isGruePursuing;
    private bool hasReachedExit;
    private string lastDirection = string.Empty;

    private string lastActionMessage = "";
    private bool firstSceneShown = false;

    // ============================
    // MAIN MENU
    // ============================

    public void ShowMainMenu()
    {
        Console.Clear();

       Console.WriteLine("\u001b[32m" + @"
                                                                                       
                                                                                       
██     ██ ▄▄▄▄▄ ▄▄     ▄▄▄▄  ▄▄▄  ▄▄   ▄▄ ▄▄▄▄▄   ██████ ▄▄▄                           
██ ▄█▄ ██ ██▄▄  ██    ██▀▀▀ ██▀██ ██▀▄▀██ ██▄▄      ██  ██▀██                          
 ▀██▀██▀  ██▄▄▄ ██▄▄▄ ▀████ ▀███▀ ██   ██ ██▄▄▄     ██  ▀███▀                          
                                                                                       
                                                                                       
                                                                                       
▄████▄ ▄▄▄▄  ▄▄ ▄▄ ▄▄▄▄▄ ▄▄  ▄▄ ▄▄▄▄▄▄ ▄▄ ▄▄ ▄▄▄▄  ▄▄▄▄▄    ▄████   ▄▄▄  ▄▄   ▄▄ ▄▄▄▄▄ 
██▄▄██ ██▀██ ██▄██ ██▄▄  ███▄██   ██   ██ ██ ██▄█▄ ██▄▄    ██  ▄▄▄ ██▀██ ██▀▄▀██ ██▄▄  
██  ██ ████▀  ▀█▀  ██▄▄▄ ██ ▀██   ██   ▀███▀ ██ ██ ██▄▄▄    ▀███▀  ██▀██ ██   ██ ██▄▄▄ 
" + "\u001b[0m");


        Console.WriteLine();
        Console.WriteLine("              [1] Start Game");
        Console.WriteLine("              [2] Quit Game");
        Console.WriteLine();
        Console.Write("              Select an option: ");

        string choice = Console.ReadLine()?.Trim() ?? "";

      if (choice == "1")
				{
					 StartFromFile("DungeonTemplate.txt");
				}
        else if (choice == "2")
        {
            ShowQuitScreen();
        }
        else
        {
            Console.WriteLine("\nInvalid option. Press ENTER to continue...");
            Console.ReadLine();
            ShowMainMenu();
        }
    }

    private void ShowQuitScreen()
    {
        Console.Clear();
        Console.WriteLine("\u001b[31m" + @"
                                                                                             
                                                        ▄▄                                   
 ▄████   ▄▄▄  ▄▄   ▄▄ ▄▄▄▄▄   ▄████▄ ▄▄ ▄▄ ▄▄▄▄▄ ▄▄▄▄   ██                                   
██  ▄▄▄ ██▀██ ██▀▄▀██ ██▄▄    ██  ██ ██▄██ ██▄▄  ██▄█▄  ██                                   
 ▀███▀  ██▀██ ██   ██ ██▄▄▄   ▀████▀  ▀█▀  ██▄▄▄ ██ ██  ▄▄                                   
                                                                                             
                                                                                             
                                                                                             
██  ██ ▄▄▄  ▄▄ ▄▄    ▄▄▄  ▄▄ ▄▄ ▄▄ ▄▄▄▄▄▄   ▄▄▄▄▄▄ ▄▄ ▄▄ ▄▄▄▄▄    ▄▄▄▄  ▄▄▄  ▄▄   ▄▄ ▄▄▄▄▄   
 ▀██▀ ██▀██ ██ ██   ██▀██ ██ ██ ██   ██       ██   ██▄██ ██▄▄    ██ ▄▄ ██▀██ ██▀▄▀██ ██▄▄    
  ██  ▀███▀ ▀███▀   ▀███▀ ▀███▀ ██   ██       ██   ██ ██ ██▄▄▄   ▀███▀ ██▀██ ██   ██ ██▄▄▄ ▄ 
                       ▀▀
" + "\u001b[0m");

        Console.WriteLine("\nPress ENTER to exit...");
        Console.ReadLine();
        Environment.Exit(0);
    }

    // ============================
    // START GAME
    // ============================

    public void StartFromFile(string dungeonFilePath)
    {
        try
        {
            var (loadedDungeon, metadata) = DungeonLoader.Load(dungeonFilePath);
            dungeon = loadedDungeon;

            adventurer = new Adventurer();

            aRow = 1;
            aCol = 1;

            enemyRow = metadata.GrueRow;
            enemyCol = metadata.GrueCol;
            exitRow = metadata.ExitRow;
            exitCol = metadata.ExitCol;

            isChestOpen = false;
            hasPlayerQuit = false;
            isAdventureAlive = true;
            isGruePursuing = false;
            hasReachedExit = false;
            lastDirection = string.Empty;
            lastActionMessage = "";
            firstSceneShown = false;

            string input;

            do
            {
                ShowScene();

                do
                {
                    ShowInputOptions();
                    input = GetInput();
                }
                while (!IsValidInput(input));

                ProcessInput(input);
                UpdateGameState();
            }
            while (!IsGameOver());

            ShowGameOverScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR LOADING DUNGEON:");
            Console.WriteLine(ex.ToString());
        }
    }

    // ============================
    // SCENE RENDERING
    // ============================

    private void ShowScene()
    {
        if (firstSceneShown)
            Console.Clear();

        if (!string.IsNullOrEmpty(lastActionMessage))
        {
            Console.WriteLine(lastActionMessage);
            Console.WriteLine();
        }

        ShowMap();

        var r = dungeon[aRow, aCol];
        if (r != null)
            Console.WriteLine(r.GetDescription());
        else
            Console.WriteLine("You are in a void. Something is wrong.");

        firstSceneShown = true;
    }

    private void ShowMap()
    {
        Console.WriteLine("Map:");
        int roomHeight = 6;

        for (int row = 0; row < dungeon.GetLength(0); row++)
        {
            for (int line = 0; line < roomHeight; line++)
            {
                for (int col = 0; col < dungeon.GetLength(1); col++)
                {
                    PrintRoomLine(row, col, line);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("Legend: player = white stickman, G = red Grue, L=Lamp, K=Key, C=Chest");
    }

    private void PrintRoomLine(int row, int col, int line)
    {
        var originalColor = Console.ForegroundColor;
        var lineText = GetRoomLineText(row, col, line, out ConsoleColor? color);

        if (color.HasValue)
        {
            Console.ForegroundColor = color.Value;
        }

        Console.Write(lineText);
        Console.ForegroundColor = originalColor;
    }

    private string GetRoomLineText(int row, int col, int line, out ConsoleColor? color)
    {
        color = null;
        var room = dungeon[row, col];

        if (room == null)
        {
            if (line == 0 || line == 5)
                return "+-----+";
            return "|#####|";
        }

        if (line == 0 || line == 5)
            return "+-----+";

        if (line == 1)
        {
            if (row == aRow && col == aCol)
            {
                color = ConsoleColor.White;
                return "|  0  |";
            }

            if (row == enemyRow && col == enemyCol)
            {
                color = ConsoleColor.Red;
                return "|  G  |";
            }

            var markers = string.Empty;
            if (room.HasLamp()) markers += 'L';
            if (room.HasKey()) markers += 'K';
            if (room.HasChest()) markers += 'C';
            return markers.Length == 0 ? "|     |" : $"| {markers.PadRight(3)} |";
        }

        if (line == 2)
        {
            if (row == aRow && col == aCol)
            {
                color = ConsoleColor.White;
                return "| \\|/ |";
            }

            if (row == enemyRow && col == enemyCol)
            {
                color = ConsoleColor.Red;
                return "| /G\\ |";
            }

            return "|     |";
        }

        if (line == 3)
        {
            if (row == aRow && col == aCol)
            {
                color = ConsoleColor.White;
                return "| / \\ |";
            }

            if (row == enemyRow && col == enemyCol)
            {
                color = ConsoleColor.Red;
                return "| / \\ |";
            }

            return "|     |";
        }

        return "|     |";
    }

    // ============================
    // INPUT
    // ============================

    private void ShowInputOptions()
    {
        string options = ""
        + $"GO NORTH [{GO_NORTH}] | GO EAST [{GO_EAST}] | GET LAMP [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
        + $"GO SOUTH [{GO_SOUTH}] | GO WEST [{GO_WEST}] | GET KEY  [{GET_KEY}] | QUIT       [{QUIT}]\n"
        + $"> ";

        Console.Write(options);
    }

    private string GetInput()
    {
        var input = Console.ReadLine();
        if (input == null)
            return QUIT;

        return input.ToUpper();
    }

    private bool IsValidInput(string input)
    {
        string[] validInputs = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };

        if (!validInputs.Contains(input))
        {
            Console.WriteLine("ERROR: Invalid input. Please try again.");
            return false;
        }

        return true;
    }

    private bool IsMovementInput(string input)
    {
        return input == GO_NORTH || input == GO_SOUTH || input == GO_EAST || input == GO_WEST;
    }

    // ============================
    // ACTIONS
    // ============================

    private void ProcessInput(string input)
    {
        Room r = dungeon[aRow, aCol];

        if (!adventurer.HasLamp() && !r.IsLit() && IsMovementInput(input) && input != lastDirection)
        {
            Console.WriteLine("You got eaten alive by the Grue!");
            isAdventureAlive = false;
        }
        else if (input == GO_NORTH)
        {
            GoNorth(r);
        }
        else if (input == GO_SOUTH)
        {
            GoSouth(r);
        }
        else if (input == GO_EAST)
        {
            GoEast(r);
        }
        else if (input == GO_WEST)
        {
            GoWest(r);
        }
        else if (input == GET_LAMP)
        {
            GetLamp(r);
        }
        else if (input == GET_KEY)
        {
            GetKey(r);
        }
        else if (input == OPEN_CHEST)
        {
            OpenChest(r);
        }
        else
        {
            Quit();
        }
    }

    private void GoNorth(Room r)
    {
        if (r.HasNorth())
        {
            aRow -= 1;
            lastDirection = GO_SOUTH;
        }
        else
        {
            Console.WriteLine("You cannot go north!\a");
        }
    }

    private void GoSouth(Room r)
    {
        if (r.HasSouth())
        {
            aRow += 1;
            lastDirection = GO_NORTH;
        }
        else
        {
            Console.WriteLine("You cannot go south!\a");
        }
    }

    private void GoEast(Room r)
    {
        if (r.HasEast())
        {
            aCol += 1;
            lastDirection = GO_WEST;
        }
        else
        {
            Console.WriteLine("You cannot go east!\a");
        }
    }

    private void GoWest(Room r)
    {
        if (r.HasWest())
        {
            aCol -= 1;
            lastDirection = GO_EAST;
        }
        else
        {
            Console.WriteLine("You cannot go west!\a");
        }
    }

    private void GetLamp(Room r)
    {
        if (r.HasLamp())
        {
            lastActionMessage = "\u001b[33mYou got the lamp!\u001b[0m";
            adventurer.SetLamp(true);
            r.SetLamp(false);
        }
        else
        {
            lastActionMessage = "\u001b[33mThere is no lamp in this room.\u001b[0m";
        }
    }

    private void GetKey(Room r)
    {
        if (r.HasKey())
        {
            lastActionMessage = "\u001b[33mYou got the key!\u001b[0m";
            adventurer.SetKey(true);
            r.SetKey(false);
        }
        else
        {
            lastActionMessage = "\u001b[33mThere is no key in this room.\u001b[0m";
        }
    }

    private void OpenChest(Room r)
    {
        if (r.HasChest())
        {
            if (adventurer.HasKey())
            {
                lastActionMessage =
                    "\u001b[33mYou opened the treasure chest!\u001b[0m\n" +
                    "\u001b[31m🔥 THE GRUE HAS AWAKENED! RUN! 🔥\u001b[0m";

                isChestOpen = true;
                isGruePursuing = true;
            }
            else
            {
                lastActionMessage = "\u001b[33mYou do not have the key!\u001b[0m";
            }
        }
        else
        {
            lastActionMessage = "\u001b[33mThere is no chest in this room.\u001b[0m";
        }
    }

    private void Quit()
    {
        hasPlayerQuit = true;
    }

    // ============================
    // GAME STATE
    // ============================

    private void UpdateGameState()
    {
        if (!isAdventureAlive || hasPlayerQuit || hasReachedExit)
            return;

        if (aRow == enemyRow && aCol == enemyCol)
        {
            Console.WriteLine("The Grue has found you!");
            isAdventureAlive = false;
            return;
        }

        if (aRow == exitRow && aCol == exitCol && isChestOpen)
        {
            hasReachedExit = true;
            return;
        }

        if (useLargeMap || isGruePursuing)
        {
            MoveGrueTowardsPlayer();

            if (aRow == enemyRow && aCol == enemyCol)
            {
                Console.WriteLine("The Grue has found you!");
                isAdventureAlive = false;
            }
        }
    }

    private bool IsGameOver()
    {
        return hasPlayerQuit || !isAdventureAlive || hasReachedExit;
    }

    // ============================
    // GRUE MOVEMENT (BFS)
    // ============================

    private void MoveGrueTowardsPlayer()
    {
        var nextStep = GetNextGrueStep();
        if (nextStep.HasValue)
        {
            enemyRow = nextStep.Value.row;
            enemyCol = nextStep.Value.col;
        }
    }

    private (int row, int col)? GetNextGrueStep()
    {
        var start = (row: enemyRow, col: enemyCol);
        var target = (row: aRow, col: aCol);
        var queue = new Queue<(int row, int col)>();
        var parent = new Dictionary<(int, int), (int, int)>();
        var visited = new bool[dungeon.GetLength(0), dungeon.GetLength(1)];

        queue.Enqueue(start);
        visited[start.row, start.col] = true;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target)
                break;

            foreach (var next in GetNeighbors(current.row, current.col))
            {
                if (visited[next.row, next.col])
                    continue;

                visited[next.row, next.col] = true;
                parent[(next.row, next.col)] = (current.row, current.col);
                queue.Enqueue(next);
            }
        }

        if (!visited[target.row, target.col])
            return null;

        var step = target;
        while (parent.ContainsKey(step) && parent[step] != start)
        {
            step = parent[step];
        }

        return step;
    }

    private IEnumerable<(int row, int col)> GetNeighbors(int row, int col)
    {
        var room = dungeon[row, col];
        if (room == null) yield break;

        if (room.HasNorth() && row - 1 >= 0 && dungeon[row - 1, col] != null && dungeon[row - 1, col].HasSouth())
            yield return (row - 1, col);

        if (room.HasSouth() && row + 1 < dungeon.GetLength(0) && dungeon[row + 1, col] != null && dungeon[row + 1, col].HasNorth())
            yield return (row + 1, col);

        if (room.HasEast() && col + 1 < dungeon.GetLength(1) && dungeon[row, col + 1] != null && dungeon[row, col + 1].HasWest())
            yield return (row, col + 1);

        if (room.HasWest() && col - 1 >= 0 && dungeon[row, col - 1] != null && dungeon[row, col - 1].HasEast())
            yield return (row, col - 1);
    }

    // ============================
    // GAME OVER SCREEN (ASCII)
    // ============================

    private void ShowGameOverScreen()
    {
        Console.Clear();

        if (hasReachedExit)
        {
            Console.WriteLine("\u001b[33m" + @"
                                                                                               
                                                                                           ▄▄ 
▄█████  ▄▄▄  ▄▄  ▄▄  ▄▄▄▄ ▄▄▄▄   ▄▄▄ ▄▄▄▄▄▄ ▄▄ ▄▄ ▄▄     ▄▄▄ ▄▄▄▄▄▄ ▄▄  ▄▄▄  ▄▄  ▄▄  ▄▄▄▄  ██ 
██     ██▀██ ███▄██ ██ ▄▄ ██▄█▄ ██▀██  ██   ██ ██ ██    ██▀██  ██   ██ ██▀██ ███▄██ ███▄▄  ██ 
▀█████ ▀███▀ ██ ▀██ ▀███▀ ██ ██ ██▀██  ██   ▀███▀ ██▄▄▄ ██▀██  ██   ██ ▀███▀ ██ ▀██ ▄▄██▀  ▄▄ 
                                                                                              
                                                                                              
                                                                ▄▄                            
██  ██ ▄▄▄  ▄▄ ▄▄   ██████  ▄▄▄▄  ▄▄▄▄  ▄▄▄  ▄▄▄▄  ▄▄▄▄▄ ▄▄▄▄   ██                            
 ▀██▀ ██▀██ ██ ██   ██▄▄   ███▄▄ ██▀▀▀ ██▀██ ██▄█▀ ██▄▄  ██▀██  ██                            
  ██  ▀███▀ ▀███▀   ██▄▄▄▄ ▄▄██▀ ▀████ ██▀██ ██    ██▄▄▄ ████▀  ▄▄ 
" + "\u001b[0m");
            Console.WriteLine("\nPress ENTER to return to menu...");
            Console.ReadLine();
            ShowMainMenu();
            return;
        }

        if (!isAdventureAlive)
        {
            Console.WriteLine("\u001b[31m" + @"
                                                                                           
                                                        ▄▄                                
 ▄████   ▄▄▄  ▄▄   ▄▄ ▄▄▄▄▄   ▄████▄ ▄▄ ▄▄ ▄▄▄▄▄ ▄▄▄▄   ██                                
██  ▄▄▄ ██▀██ ██▀▄▀██ ██▄▄    ██  ██ ██▄██ ██▄▄  ██▄█▄  ██                                
 ▀███▀  ██▀██ ██   ██ ██▄▄▄   ▀████▀  ▀█▀  ██▄▄▄ ██ ██  ▄▄                                
                                                                                          
                                                                                          
                                                                                       ▄▄ 
 ▄████  ▄▄▄▄  ▄▄ ▄▄ ▄▄▄▄▄   ▄█████  ▄▄▄  ▄▄ ▄▄  ▄▄▄▄ ▄▄ ▄▄ ▄▄▄▄▄▄   ██  ██ ▄▄▄  ▄▄ ▄▄  ██ 
██  ▄▄▄ ██▄█▄ ██ ██ ██▄▄    ██     ██▀██ ██ ██ ██ ▄▄ ██▄██   ██      ▀██▀ ██▀██ ██ ██  ██ 
 ▀███▀  ██ ██ ▀███▀ ██▄▄▄   ▀█████ ██▀██ ▀███▀ ▀███▀ ██ ██   ██       ██  ▀███▀ ▀███▀  ▄▄ 
                                                                                          
" + "\u001b[0m");
            Console.WriteLine("\nPress ENTER to return to menu...");
            Console.ReadLine();
            ShowMainMenu();
            return;
        }

        if (hasPlayerQuit)
        {
            ShowQuitScreen();
            return;
        }

        Console.WriteLine("Game Over!");
        Console.WriteLine("\nPress ENTER to return to menu...");
        Console.ReadLine();
        ShowMainMenu();
    }
}
