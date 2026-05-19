using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;

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
    private int enemyRow; //Grue's row
    private int enemyCol; // Grue's column
    private int exitRow;
    private int exitCol;
    private bool useLargeMap;
    private bool isChestOpen;
    private bool hasPlayerQuit;
    private bool isAdventureAlive;
    private bool isGruePursuing; //Grues only pursue the player after the chest is opened
    private bool hasReachedExit;
    private string lastDirection = string.Empty;

    private string lastActionMessage = "";
    private bool firstSceneShown = false;
    private readonly string welcomeMusicPath = @"c:\Users\mmpad\Downloads\nathanielthomasbrack-8-bit-loop-189494.mp3";
    private readonly string walkingSoundPath = @"c:\Users\mmpad\Downloads\blipSelect.wav";
    private readonly string deathSoundPath = @"c:\Users\mmpad\Downloads\make_more_sound-8-bit-video-game-lose-sound-version-1-145828.mp3";
    private readonly string pickupSoundPath = @"c:\Users\mmpad\Downloads\freesound_community-8-bit-powerup-6768.mp3";
    private Thread? welcomeMusicThread;
    private Dispatcher? welcomeMusicDispatcher;
    private AutoResetEvent? welcomeMusicStarted;

    private enum SoundType
    {
        Movement,
        Pickup,
        Death,
        Win,
        OpenChest,
        InvalidMove
    }

    // ============================
    // MAIN MENU
    // ============================

    public void ShowMainMenu()
    {
        StartWelcomeMusic();
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

        StopWelcomeMusic();
        Console.WriteLine("\nPress ENTER to exit...");
        Console.ReadLine();
        Environment.Exit(0);
    }

    private void StartWelcomeMusic()
    {
        try
        {
            if (!File.Exists(welcomeMusicPath))
                return;

            if (welcomeMusicThread != null && welcomeMusicThread.IsAlive)
                return;

            welcomeMusicStarted = new AutoResetEvent(false);

            welcomeMusicThread = new Thread(() =>
            {
                try
                {
                    welcomeMusicDispatcher = Dispatcher.CurrentDispatcher;
                    var player = new MediaPlayer();
                    player.Open(new Uri(welcomeMusicPath));
                    player.MediaEnded += (sender, args) => player.Position = TimeSpan.Zero;
                    player.Play();
                    welcomeMusicStarted?.Set();
                    Dispatcher.Run();
                    player.Close();
                }
                catch
                {
                    // Ignore playback errors.
                }
            });

            welcomeMusicThread.SetApartmentState(ApartmentState.STA);
            welcomeMusicThread.IsBackground = true;
            welcomeMusicThread.Start();

            welcomeMusicStarted?.WaitOne(1000);
        }
        catch
        {
            // Silently ignore if the environment doesn't support MP3 playback.
        }
    }

    private void StopWelcomeMusic()
    {
        try
        {
            if (welcomeMusicDispatcher != null)
            {
                welcomeMusicDispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                welcomeMusicDispatcher = null;
            }

            if (welcomeMusicThread != null)
            {
                welcomeMusicThread.Join(1000);
                welcomeMusicThread = null;
            }

            welcomeMusicStarted?.Dispose();
            welcomeMusicStarted = null;
        }
        catch
        {
            // Ignore stop errors.
        }
    }

    // ============================
    // START GAME
    // ============================

    public void Start()
    {
        StartFromFile("DungeonTemplate.txt");
    }

    public void StartFromFile(string dungeonFilePath)
    {
        StopWelcomeMusic();

        try
        {
            var (loadedDungeon, metadata) = DungeonLoader.Load(dungeonFilePath);
            dungeon = loadedDungeon;

            adventurer = new Adventurer();

            aRow = 1;
            aCol = 1;

            enemyRow = metadata.GrueRow; //Grues start position is loaded from file
            enemyCol = metadata.GrueCol; //Grues start position is loaded from file
            exitRow = metadata.ExitRow;
            exitCol = metadata.ExitCol;

            ClearItemLocations();
            PlaceRandomItemPositions();

            isChestOpen = false;
            hasPlayerQuit = false;
            isAdventureAlive = true;
            isGruePursuing = false; //Grues only pursue the player after the chest is opened
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

    private void ClearItemLocations()
    {
        foreach (var room in dungeon)
        {
            if (room != null)
            {
                room.SetLamp(false);
                room.SetKey(false);
                room.SetChest(false);
            }
        }
    }

    private void PlaceRandomItemPositions()
    {
        var traversableRooms = new List<(int row, int col)>();

        for (int row = 0; row < dungeon.GetLength(0); row++)
        {
            for (int col = 0; col < dungeon.GetLength(1); col++)
            {
                if (dungeon[row, col] != null)
                    traversableRooms.Add((row, col));
            }
        }

        var forbidden = new HashSet<(int row, int col)>
        {
            (aRow, aCol),
            (exitRow, exitCol),
            (enemyRow, enemyCol)
        };

        var candidates = traversableRooms.Where(room => !forbidden.Contains(room)).ToList();

        if (candidates.Count < 3)
            return;

        var lampTile = GetNearbyTraversableTile(candidates, 1)
            ?? GetNearbyTraversableTile(candidates, 2)
            ?? GetRandomTraversableTile(candidates);

        if (lampTile.HasValue)
        {
            dungeon[lampTile.Value.row, lampTile.Value.col].SetLamp(true);
            candidates.Remove(lampTile.Value);
        }

        var keyTile = GetRandomTraversableTile(candidates);
        if (keyTile.HasValue)
        {
            dungeon[keyTile.Value.row, keyTile.Value.col].SetKey(true);
            candidates.Remove(keyTile.Value);
        }

        var chestCandidates = candidates
            .Where(t => GetManhattanDistance(t, (enemyRow, enemyCol)) <= 3)
            .Where(t => GetManhattanDistance(t, (aRow, aCol)) >= 2)
            .ToList();

        var chestTile = GetRandomTraversableTile(chestCandidates);
        if (!chestTile.HasValue)
        {
            var farFromPlayer = candidates
                .Where(t => GetManhattanDistance(t, (aRow, aCol)) >= 2)
                .ToList();
            chestTile = GetRandomTraversableTile(farFromPlayer);
        }

        if (!chestTile.HasValue)
        {
            chestTile = GetRandomTraversableTile(candidates);
        }

        if (chestTile.HasValue)
        {
            dungeon[chestTile.Value.row, chestTile.Value.col].SetChest(true);
        }
    }

    private static int GetManhattanDistance((int row, int col) a, (int row, int col) b)
    {
        return Math.Abs(a.row - b.row) + Math.Abs(a.col - b.col);
    }

    private (int row, int col)? GetNearbyTraversableTile(List<(int row, int col)> candidates, int maxDistance)
    {
        var nearby = candidates
            .Where(t => Math.Abs(t.row - aRow) + Math.Abs(t.col - aCol) <= maxDistance)
            .ToList();

        if (nearby.Count == 0)
            return null;

        return nearby[random.Next(nearby.Count)];
    }

    private (int row, int col)? GetRandomTraversableTile(List<(int row, int col)> candidates)
    {
        if (candidates.Count == 0)
            return null;

        return candidates[random.Next(candidates.Count)];
    }

    private void PlaySound(SoundType sound)
    {
        try
        {
            bool playedClip = false;

            switch (sound)
            {
                case SoundType.Movement:
                    playedClip = PlaySoundClip(walkingSoundPath);
                    break;
                case SoundType.Pickup:
                    playedClip = PlaySoundClip(pickupSoundPath);
                    break;
                case SoundType.OpenChest:
                    playedClip = PlaySoundClip(pickupSoundPath);
                    break;
                case SoundType.Death:
                    playedClip = PlaySoundClip(deathSoundPath);
                    break;
                case SoundType.Win:
                    playedClip = false;
                    break;
                case SoundType.InvalidMove:
                    playedClip = false;
                    break;
            }

            if (!playedClip)
            {
                switch (sound)
                {
                    case SoundType.Movement:
                        Console.Beep(520, 25);
                        Console.Beep(620, 20);
                        break;
                    case SoundType.Pickup:
                        Console.Beep(880, 40);
                        Console.Beep(1040, 30);
                        break;
                    case SoundType.OpenChest:
                        Console.Beep(660, 35);
                        Console.Beep(820, 35);
                        Console.Beep(980, 45);
                        break;
                    case SoundType.Death:
                        Console.Beep(260, 70);
                        Console.Beep(220, 60);
                        Console.Beep(180, 50);
                        Console.Beep(140, 40);
                        break;
                    case SoundType.Win:
                        Console.Beep(880, 80);
                        Console.Beep(1040, 80);
                        Console.Beep(1240, 90);
                        Console.Beep(1500, 110);
                        Console.Beep(1760, 130);
                        break;
                    case SoundType.InvalidMove:
                        Console.Beep(320, 50);
                        Console.Beep(240, 40);
                        break;
                }
            }
        }
        catch
        {
            // Ignore beep or playback failures on unsupported environments.
        }
    }

    private bool PlaySoundClip(string soundFilePath)
    {
        if (string.IsNullOrEmpty(soundFilePath) || !File.Exists(soundFilePath))
            return false;

        PlayAudioFileAsync(soundFilePath);
        return true;
    }

    private void PlayAudioFileAsync(string soundFilePath)
    {
        var started = new AutoResetEvent(false);
        var thread = new Thread(() =>
        {
            try
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                var player = new MediaPlayer();

                player.MediaEnded += (sender, args) => dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                player.MediaFailed += (sender, args) => dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                player.Open(new Uri(soundFilePath));
                player.MediaOpened += (sender, args) =>
                {
                    player.Play();
                    started.Set();
                };

                Dispatcher.Run();
                player.Close();
            }
            catch
            {
                started.Set();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();

        started.WaitOne(500);
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
            PlaySound(SoundType.Movement);
        }
        else
        {
            Console.WriteLine("You cannot go north!");
            PlaySound(SoundType.InvalidMove);
        }
    }

    private void GoSouth(Room r)
    {
        if (r.HasSouth())
        {
            aRow += 1;
            lastDirection = GO_NORTH;
            PlaySound(SoundType.Movement);
        }
        else
        {
            Console.WriteLine("You cannot go south!");
            PlaySound(SoundType.InvalidMove);
        }
    }

    private void GoEast(Room r)
    {
        if (r.HasEast())
        {
            aCol += 1;
            lastDirection = GO_WEST;
            PlaySound(SoundType.Movement);
        }
        else
        {
            Console.WriteLine("You cannot go east!");
            PlaySound(SoundType.InvalidMove);
        }
    }

    private void GoWest(Room r)
    {
        if (r.HasWest())
        {
            aCol -= 1;
            lastDirection = GO_EAST;
            PlaySound(SoundType.Movement);
        }
        else
        {
            Console.WriteLine("You cannot go west!");
            PlaySound(SoundType.InvalidMove);
        }
    }

    private void GetLamp(Room r)
    {
        if (r.HasLamp())
        {
            lastActionMessage = "\u001b[33mYou got the lamp!\u001b[0m";
            adventurer.SetLamp(true);
            r.SetLamp(false);
            PlaySound(SoundType.Pickup);
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
            PlaySound(SoundType.Pickup);
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

                isChestOpen = true; //Grue starts pursuing the player as soon as the chest is opened
                isGruePursuing = true; // Grue is now pursuing the player
                PlaySound(SoundType.OpenChest);
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
            PlaySound(SoundType.Death);
            return;
        }

        if (aRow == exitRow && aCol == exitCol && isChestOpen)
        {
            hasReachedExit = true;
            PlaySound(SoundType.Win);
            return;
        }

        if (useLargeMap || isGruePursuing) //Grue only pursues the player on large maps
        {
            MoveGrueTowardsPlayer();//Grue moves towards the player after each action

            if (aRow == enemyRow && aCol == enemyCol)//If the Grue has moved onto the player's position, the player dies
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
        var nextStep = GetNextGrueStep();//Grue uses BFS to find the shortest path 
        if (nextStep.HasValue)
        {
            enemyRow = nextStep.Value.row;
            enemyCol = nextStep.Value.col;
        }
    }

    private (int row, int col)? GetNextGrueStep()
    {
        var start = (row: enemyRow, col: enemyCol);//Grue's current position
        var target = (row: aRow, col: aCol);//Grue's target is the player's current position
        var queue = new Queue<(int row, int col)>();//Grue's exploration queue for BFS
        var parent = new Dictionary<(int, int), (int, int)>();// To reconstruct the path 
        var visited = new bool[dungeon.GetLength(0), dungeon.GetLength(1)];// To keep track of visited rooms

        queue.Enqueue(start);//BFS starts here
        visited[start.row, start.col] = true;

        while (queue.Count > 0)//BFS loop
        {
            var current = queue.Dequeue();
            if (current == target)
                break;

            foreach (var next in GetNeighbors(current.row, current.col))//Explore neighbors
            {
                if (visited[next.row, next.col])
                    continue;

                visited[next.row, next.col] = true;
                parent[(next.row, next.col)] = (current.row, current.col);// Set parent to reconstruct path
                queue.Enqueue(next);
            }
        }

        if (!visited[target.row, target.col])
            return null;

        var step = target;
        while (parent.ContainsKey(step) && parent[step] != start)//Big-O O(V+E)
        {
            step = parent[step];
        }

        return step;
    }

    private IEnumerable<(int row, int col)> GetNeighbors(int row, int col)//Get valid neighboring rooms
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
            PlaySound(SoundType.Win);
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
