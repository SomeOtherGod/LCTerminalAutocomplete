using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine.InputSystem;
using static TerminalApi.Events.Events;
using System.Collections.Generic;
using LethalCompanyInputUtils.Api;

namespace LCTerminalAutocomplete
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("atomic.terminalapi", MinimumDependencyVersion: "1.3.0")]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", MinimumDependencyVersion: "0.4.2")]
    public partial class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "hox.lcterminalautocomplete", modName = "Terminal Autocomplete", modVersion = "1.0.4";
        private Terminal Terminal;
        private Keybinds _keybinds;
        private string[] _commands;
        private string[] _customCommands;
        private int _userInput = 0;
        private int _index = 0;
        private void Awake()
        {
            Logger.LogInfo($"Plugin Terminal Autocomplete is loaded!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            TerminalStarted += OnTerminalStarted;
            TerminalExited += OnTerminalExited;
            TerminalBeginUsing += OnTerminalBeginUsing;
            TerminalParsedSentence += OnTerminalSubmit;
            TerminalTextChanged += OnTerminalTextChange;
            _customCommands = ((BaseUnityPlugin)this).Config.Bind<string[]>("Custom Commands", "Commands", new string[] { "" }, "Place your commands here").Value;
        }
        private void OnTerminalTextChange(object sender, TerminalTextChangedEventArgs e)
        {
            if (_userInput <= 0)
            {
                _commands[0] = e.CurrentInputText;
                _index = 0;
            }
            _userInput -= 1;
        }

        private void OnTerminalSubmit(object sender, TerminalParseSentenceEventArgs e)
        {
            _commands[0] = "";
        }

        private void OnTerminalExited(object sender, TerminalEventArgs e)
        {
            _keybinds.PrevTerminalKey.performed -= OnLeftArrowPerformed;
            _keybinds.PrevTerminalKey.Disable();

            _keybinds.NextTerminalKey.performed -= OnRightArrowPerformed;
            _keybinds.NextTerminalKey.Disable();
        }

        private void OnTerminalBeginUsing(object sender, TerminalEventArgs e)
        {
            _keybinds.PrevTerminalKey.Enable();
            _keybinds.PrevTerminalKey.performed += OnLeftArrowPerformed;

            _keybinds.NextTerminalKey.Enable();
            _keybinds.NextTerminalKey.performed += OnRightArrowPerformed;
        }

        private void OnTerminalStarted(object sender, TerminalEventArgs e)
        {
            _keybinds.PrevTerminalKey = new InputAction("LeftArrow", 0, "<Keyboard>/leftarrow", "Press");
            _keybinds.NextTerminalKey = new InputAction("RightArrow", 0, "<Keyboard>/rightarrow", "Press");

            Terminal = TerminalApi.TerminalApi.Terminal;
            Logger.LogInfo($"|Plugin Terminal Autocomplete loaded keywors|");
            TerminalKeyword[] keywords = Terminal.terminalNodes.allKeywords;

            List<string> commands = new List<string>(){""};
            for (int i = 1; i < keywords.Length; i++)
            {
                Logger.LogInfo($"{keywords[i].name}");
                commands.Add(keywords[i].name);
            }
            Logger.LogInfo($"|Plugin Terminal Autocomplete loaded keywors|");
            _commands = commands.ToArray();
        }

        private void OnRightArrowPerformed(InputAction.CallbackContext context)
        {
            if (Terminal.terminalInUse)
            {
                for (int i = 1+_index; i < _commands.Length-1; i++)
                {
                    if (_commands[i].Substring(0, _commands[0].Length).ToUpper() == _commands[0].ToUpper() || _commands[0].Length == 0)
                    {
                        _index = i;
                        string command = _commands[i];
                        SetTerminalText(command);
                        return;
                    }
                }
            }
        }

        private void OnLeftArrowPerformed(InputAction.CallbackContext context)
        {
            if (Terminal.terminalInUse)
            {
                for (int i = _index-1; i >= 0; i--)
                {
                    if (_commands[i].Substring(0, _commands[0].Length).ToUpper() == _commands[0].ToUpper() || _commands[0].Length == 0)
                    {
                        _index = i;
                        string command = _commands[i];
                        SetTerminalText(command);
                        return;
                    }
                }
            }
        }

        private void SetTerminalText(string text)
        {
            _userInput = 2;
            Terminal.TextChanged(TerminalApi.TerminalApi.Terminal.currentText.Substring(0, TerminalApi.TerminalApi.Terminal.currentText.Length - TerminalApi.TerminalApi.Terminal.textAdded) + text);
            Terminal.screenText.text = TerminalApi.TerminalApi.Terminal.currentText;
            Terminal.textAdded = text.Length;
        }
    }
    public class Keybinds : LcInputActions
    {
        [InputAction("<Keyboard>/leftArrow", Name = "Previous Command (AC)")]
        public InputAction PrevTerminalKey { get; set; }

        [InputAction("<Keyboard>/rightArrow", Name = "Next Command (AC)")]
        public InputAction NextTerminalKey { get; set; }
    }
}
