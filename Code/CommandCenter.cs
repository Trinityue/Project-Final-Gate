using Godot;
using System;

public partial class CommandCenter : Node2D
{
    private LineEdit commandInput;
    private Button executeButton;
    private TextEdit outputBox;
    private float _previousTimeScale = 1f;

    public override void _Ready()
    {
        commandInput = GetNodeOrNull<LineEdit>("CommandInput");
        executeButton = GetNodeOrNull<Button>("ExecuteButton");
        outputBox = GetNodeOrNull<TextEdit>("OutputBox");

        if (commandInput == null)
            GD.PrintErr("CommandInput not found in CommandCenter scene. Bitte Node-Namen prüfen.");

        if (executeButton != null)
            executeButton.Pressed += OnExecutePressed;

        if (commandInput != null)
            commandInput.TextSubmitted += OnCommandSubmitted;
    }

    // Show the command center and focus the input; perform a full time-scale pause
    public void ShowWithFocus(bool show)
    {
        Visible = show;

        // Set global soft pause flag so game systems can respect it
        GameState.Paused = show;

        if (show)
        {
            // store and stop time
            try { _previousTimeScale = (float)Engine.TimeScale; } catch { _previousTimeScale = 1f; }
            try { Engine.TimeScale = 0f; } catch { }

            if (commandInput != null)
            {
                // Grab focus so keyboard input goes to the LineEdit
                commandInput.CallDeferred("grab_focus");
                try { commandInput.CaretColumn = commandInput.Text.Length; } catch { }
            }
        }
        else
        {
            // restore time
            try { Engine.TimeScale = _previousTimeScale; } catch { }
            GameState.Paused = false;
        }
    }

    private void OnCommandSubmitted(string text)
    {
        ExecuteCommand(text);
        commandInput.Text = "";
    }

    private void OnExecutePressed()
    {
        if (commandInput == null) return;
        ExecuteCommand(commandInput.Text);
        commandInput.Text = "";
    }

    private void PrintOutput(string line)
    {
        if (outputBox != null)
        {
            // append to TextEdit's Text and move caret to end
            outputBox.Text += line + "\n";
            // attempt to scroll to bottom by setting ScrollVertical
            outputBox.ScrollVertical = outputBox.GetLineCount();
        }
        else
        {
            GD.Print(line);
        }
    }

    private void ExecuteCommand(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        var parts = raw.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLower();

        try
        {
            switch (cmd)
            {
                case "help":
                    PrintOutput("Verfügbare Befehle: help, setmoney <value>, addmoney <value>, setmps <value>, settowercost <value>, sethealth <value>, setmaxhealth <value>, settowerchoice <1|2>");
                    break;
                case "setmoney":
                    if (parts.Length >= 2 && float.TryParse(parts[1], out float m))
                    {
                        MoneyManagment.Instance.Money = m;
                        MoneyManagment.Instance.UpdateLabel();
                        PrintOutput($"Money gesetzt: {m}");
                    }
                    else PrintOutput("Usage: setmoney <number>");
                    break;
                case "addmoney":
                    if (parts.Length >= 2 && float.TryParse(parts[1], out float a))
                    {
                        MoneyManagment.Instance.AddMoney((int)a);
                        PrintOutput($"Money hinzugefügt: {a}");
                    }
                    else PrintOutput("Usage: addmoney <number>");
                    break;
                case "setmps":
                    if (parts.Length >= 2 && float.TryParse(parts[1], out float mps))
                    {
                        MoneyManagment.Instance.MoneyPerSecond = mps;
                        PrintOutput($"MoneyPerSecond gesetzt: {mps}");
                    }
                    else PrintOutput("Usage: setmps <number>");
                    break;
                case "settowercost":
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int cost))
                    {
                        var gm = GetTree().Root.GetNodeOrNull<Node2D>("/root/Node2D");
                        var gameManager = GetTree().Root.GetNodeOrNull<GameManager>("Node2D");
                        if (gameManager != null)
                        {
                            gameManager.SetTowerCost(cost);
                            PrintOutput($"TowerCost gesetzt: {cost}");
                        }
                        else PrintOutput("GameManager nicht gefunden.");
                    }
                    else PrintOutput("Usage: settowercost <integer>");
                    break;
                case "sethealth":
                    if (parts.Length >= 2 && float.TryParse(parts[1], out float h))
                    {
                        var gameManager = GetTree().Root.GetNodeOrNull<GameManager>("Node2D");
                        if (gameManager != null)
                        {
                            gameManager.SetPlayerHealth(h);
                            PrintOutput($"Player_Health gesetzt: {h}");
                        }
                        else PrintOutput("GameManager nicht gefunden.");
                    }
                    else PrintOutput("Usage: sethealth <number>");
                    break;
                case "setmaxhealth":
                    if (parts.Length >= 2 && float.TryParse(parts[1], out float mh))
                    {
                        var gameManager = GetTree().Root.GetNodeOrNull<GameManager>("Node2D");
                        if (gameManager != null)
                        {
                            gameManager.SetPlayerMaxHealth(mh);
                            PrintOutput($"Player_Max_Health gesetzt: {mh}");
                        }
                        else PrintOutput("GameManager nicht gefunden.");
                    }
                    else PrintOutput("Usage: setmaxhealth <number>");
                    break;
                case "settowerchoice":
                case "settowerchoiche":
                    if (parts.Length >= 2 && float.TryParse(parts[1], out float tc))
                    {
                        var gameManager = GetTree().Root.GetNodeOrNull<GameManager>("Node2D");
                        if (gameManager != null)
                        {
                            gameManager.SetTowerChoice(tc);
                            PrintOutput($"towerchoice gesetzt: {tc}");
                        }
                        else PrintOutput("GameManager nicht gefunden.");
                    }
                    else PrintOutput("Usage: settowerchoice <1|2>");
                    break;
                case "placingdebug":
                    if (parts.Length >= 2)
                    {
                        var val = parts[1].ToLower();
                        if (val != "true" && val != "false")
                        {
                            PrintOutput("Usage: placingdebug <true|false>");
                            break;
                        }
                        bool pd = val == "true";

                        var gm = GameManager.Instance;
                        if (gm != null)
                        {
                            gm.PlacingDebug = pd;
                            PrintOutput($"PlacingDebug gesetzt: {pd}");
                        }
                        else
                        {
                            PrintOutput("GameManager nicht gefunden.");
                        }
                    }
                    else PrintOutput("Usage: placingdebug <true|false>");
                    break;
                default:
                    PrintOutput("Unbekannter Befehl. Tipp: help");
                    break;
            }
        }
        catch (Exception ex)
        {
            PrintOutput("Fehler beim Ausführen des Befehls: " + ex.Message);
        }
    }
}
