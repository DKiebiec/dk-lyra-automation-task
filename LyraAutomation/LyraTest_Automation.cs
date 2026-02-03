using NUnit.Framework;
using AltTester.AltTesterSDK.Driver;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

public class LyraAutomationTests
{
    private AltDriver altDriver;

    // Maps
    private const string TestMap = "L_FiringRange_WP";
    private const string ExpanseMap = "L_Expanse";
    private const string ConvolutionMap = "L_Convolution_Blockout";
    private const string MenuMap = "L_LyraFrontEnd";

    // Player definition
    private const string PlayerPath = "//*[contains(@name,Hero)]";
    private const string PlayerControllerPath = "//*[contains(@name,LyraPlayerController)]";

    [OneTimeSetUp]
    public void SetUp()
    {
        altDriver = new AltDriver();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        StopAim(); // in case we forgot to call it off after a test
        altDriver.Stop();
    }

    private static readonly string[] PlayableMaps =
    {
        ExpanseMap,
        ConvolutionMap,
        TestMap,
        //MenuMap //uncomment for fail state
    };

    [TestCaseSource(nameof(PlayableMaps))]
    public void CheckAvailableMaps(string mapName)
    {
        LoadMap(mapName);

        string currentLevel = altDriver.GetCurrentScene();

        PlayerSpawned();

        Assert.That(currentLevel, Is.EqualTo(mapName), $"Expected level {mapName} but got {currentLevel}");
        Console.WriteLine($"Player loaded to {mapName} successfully.");
    }
 
    private readonly Coordinate launchUp = new Coordinate(370f, 1250f, 93f);
    private readonly Coordinate launchForwards = new Coordinate(0f, 1250f, 93f);
    
    [Test]
    public void TeleportPlayerToLaunchUp()
    {
        LoadMap(TestMap);

        TeleportPlayerToCoordinate(launchUp);
        SetPlayerRotation(0f, 90f, 0f);
        AssertPlayerAtCoordinate(launchUp, 5f);
        altDriver.PressKey(AltKeyCode.W, duration: 1);
        Thread.Sleep(500);

        float minZ = 150f; // minimum height difference
        float z = ParseLocationString(Player().CallComponentMethod<string>("Actor", "K2_GetActorLocation", "", new object[] { })).Z;

        Assert.That(z, Is.GreaterThanOrEqualTo(minZ), $"Player Z too low: {z} < {minZ}");
        Console.WriteLine($"Upwards-launching pad functionality verified.");

    }

    [Test]
    public void TeleportPlayerToLaunchForwards()
    {
        LoadMap(TestMap);

        TeleportPlayerToCoordinate(launchForwards);
        SetPlayerRotation(0f, 90f, 0f);
        AssertPlayerAtCoordinate(launchForwards, 5f);

        float minY = 150f; // tolerance
        float startY = ParseLocationString(Player().CallComponentMethod<string>("Actor", "K2_GetActorLocation", "", new object[] { })).Y;

        altDriver.PressKey(AltKeyCode.W, duration: 1);

        Thread.Sleep(500);
        float endY = ParseLocationString(Player().CallComponentMethod<string>("Actor", "K2_GetActorLocation", "", new object[] { })).Y;

        Assert.That(endY, Is.GreaterThanOrEqualTo(startY+minY), $"Player did not travel far enough: {endY} < {startY+minY}");
        Console.WriteLine($"Forwards-launching pad functionality verified.");

    }
   
    [Test]
    public void AimingTest()
    {
        LoadMap(TestMap);
        // aim at random bot smoothly and walk around
        var bot = GetRandomBot();
        string botName = bot.name;
        AimPlayerAtTarget(botName, true);
        altDriver.PressKey(AltKeyCode.D, duration: 3);
        StopAim();
        Thread.Sleep(500);

        // teleport to lower area and walk around, focusing on spots on the map, both with point-look and smooth look
        TeleportPlayerToCoordinate(launchForwards);
        AssertPlayerAtCoordinate(launchForwards, 5f);
        altDriver.PressKey(AltKeyCode.A, duration: 1);
        AimPlayerAtTarget("(X=320,Y=-888,Z=795)");
        altDriver.PressKey(AltKeyCode.D, duration: 1);
        AimPlayerAtTarget("(X=320,Y=-888,Z=795)", true);
        altDriver.PressKey(AltKeyCode.A, duration: 3);
        AimPlayerAtTarget("(X=1320,Y=1288,Z=795)", true);
        altDriver.PressKey(AltKeyCode.D, duration: 4);
        StopAim();

        var bot2 = GetRandomBot();
        string botName2 = bot2.name;
        AimPlayerAtTarget(botName2, true);
        Thread.Sleep(500);

    }
    
    /// helper functions
    private void LoadMap(string mapName, int waitTime = 5000)
    {
        altDriver.LoadScene(mapName);
        Thread.Sleep(waitTime);
        altDriver.WaitForCurrentSceneToBe(mapName);
    }

    private void PlayerSpawned()
    {
        Player();
    }

    private AltObject Player(bool isMainPlayer = true)
    {
        var players = altDriver.FindObjects(By.PATH, PlayerPath);
        foreach (var player in players)
        {
            var isBotControlled = player.CallComponentMethod<bool>("Pawn", "IsBotControlled", "", new object[] {});
            if (isMainPlayer ? !isBotControlled : isBotControlled)
                return player;
        }

        throw new Exception("Failed to find player.");
    }

    private class Coordinate
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Coordinate(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"(X={X},Y={Y},Z={Z})";
        }

        // This converts the coordinate to a "FVector-like" format for UE calls
        public string ToFVector()
        {
            // AltTester usually accepts the same string format as ToString()
            return $"(X={X},Y={Y},Z={Z})";
        }
    }

    private AltObject GetRandomBot()
    {
        var allPlayers = altDriver.FindObjects(By.PATH, PlayerPath);
        var bots = new List<AltObject>();

        foreach (var obj in allPlayers)
        {
            bool isBotControlled = obj.CallComponentMethod<bool>("Pawn", "IsBotControlled", "", new object[] { });

            // Only include bots (not the main player)
            if (isBotControlled)
                bots.Add(obj);
        }

        if (bots.Count == 0)
            throw new Exception("No bots found in the scene.");

        // Pick a random bot
        Random rnd = new Random();
        int index = rnd.Next(bots.Count);

        return bots[index];
    }

    private void TeleportPlayerToCoordinate(Coordinate coordinate)
    {

        // Get the player AltObject
        var player = Player();

        // Set player location using UE's K2_SetActorLocation
        player.CallComponentMethod<string>(
            "Actor",
            "K2_SetActorLocation",
            "",
            new object[] { coordinate.ToString(), false, null, true }
        );

        Thread.Sleep(500);

        Console.WriteLine($"Teleporting the Player to {coordinate} ...");
    }

    private void AssertPlayerAtCoordinate(Coordinate expected, float tolerance = 5f)
    {
        var player = Player();

        // Get current location string from UE
        string locationString = player.CallComponentMethod<string>("Actor", "K2_GetActorLocation", "", new object[] {});

        // Parse string into Coordinate
        Coordinate actual = ParseLocationString(locationString);

        // Assert each axis is within tolerance
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(tolerance), $"X coordinate mismatch: expected {expected.X}, got {actual.X}");
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(tolerance), $"Y coordinate mismatch: expected {expected.Y}, got {actual.Y}");
        Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(tolerance), $"Z coordinate mismatch: expected {expected.Z}, got {actual.Z}");

        Console.WriteLine($"Confirmed the Player at location {actual}");
    }

    // --------------------------
    // Helper: parse UE location string "(X=...,Y=...,Z=...)"
    // --------------------------
    private Coordinate ParseLocationString(string locationString)
    {
        // Remove parentheses
        locationString = locationString.Replace("(", "").Replace(")", "");

        var parts = locationString.Split(',');

        float x = float.Parse(parts[0].Split('=')[1]);
        float y = float.Parse(parts[1].Split('=')[1]);
        float z = float.Parse(parts[2].Split('=')[1]);

        return new Coordinate(x, y, z);
    }

    private void SetPlayerRotation(float pitch = 0f, float yaw = 90f, float roll = 0f)
    {
        var playerController = altDriver.FindObject(By.PATH, PlayerControllerPath);

        // Format rotation string exactly like UE expects
        string rotationString = $"(Pitch={pitch},Yaw={yaw},Roll={roll})";

        Console.WriteLine($"Setting player rotation to {rotationString}");

        // Apply rotation via PlayerController
        playerController.CallComponentMethod<string>(
            "Controller",
            "SetControlRotation",
            "",
            new object[] { rotationString }
        );

        Thread.Sleep(500); // allow rotation to apply
    }

    // Helper: Aiming at a target
    // Given string will check if target is a valid entity, otherwise check if it is a point in 3D space
    // Continuous option tracks the target, otherwise looks at the target once

    private void AimPlayerAtTarget(string target, bool continuous = false, float interpSpeed = 15f)
    {
        StopAim(); // stop previous tracking

        var player = Player();
        if (player == null) return;

        AltObject targetObject = null;
        Coordinate targetPos = null;

        Console.WriteLine($"Aiming at {target}");

        // Try resolve actor first
        try { targetObject = altDriver.FindObjectWhichContains(By.NAME, target); } catch { }

        if (targetObject != null && targetObject.enabled)
        {
            if (continuous) // follow target
            {
                player.CallComponentMethod<string>(
                    "AimTracker",
                    "StartTrackingActor",
                    "",
                    new object[] { targetObject, interpSpeed, true }
                );
            }
            else // look at target only once
            {
                string targetLocStr = targetObject.CallComponentMethod<string>("Actor", "K2_GetActorLocation", "", new object[] { });
                targetPos = ParseLocationString(targetLocStr);

                player.CallComponentMethod<string>(
                    "AimTracker",
                    "AimAtLocation",
                    "",
                    new object[] { targetPos.ToFVector(), interpSpeed, false }
                );
            }
            return;
        }

        // Fallback: world coordinate
        if (!TryParseCoordinate(target, out targetPos)) return;

        player.CallComponentMethod<string>(
            "AimTracker",
            "AimAtLocation",
            "",
            new object[] { targetPos.ToFVector(), interpSpeed, continuous }
        );
    }

    private void StopAim()
    {
        var player = Player();
        try
        {
            player.CallComponentMethod<string>("AimTracker", "StopTracking", "", new object[] { });
        }
        catch
        {
            Console.WriteLine($"AimTracker not found on '{player.name}'");
        }
    }

    // check if string is a coordinate
    private bool TryParseCoordinate(string value, out Coordinate coordinate)
    {
        coordinate = null;

        try
        {
            if (!value.StartsWith("(") || !value.Contains("X="))
                return false;

            coordinate = ParseLocationString(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}