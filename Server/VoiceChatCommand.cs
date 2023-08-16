using System.Reflection;
using Hkmp.Api.Command.Server;

namespace HkmpVoiceChat.Server;

public class VoiceChatCommand : IServerCommand {
    /// <inheritdoc />
    public string Trigger => "/voicechat";

    /// <inheritdoc />
    public string[] Aliases => new[] { "/vc" };

    /// <inheritdoc />
    public bool AuthorizedOnly => true;

    private readonly ServerSettings _settings;

    public VoiceChatCommand(ServerSettings settings) {
        _settings = settings;
    }

    /// <inheritdoc />
    public void Execute(ICommandSender commandSender, string[] args) {
        void SendUsage() {
            commandSender.SendMessage($"Invalid usage: {Trigger} <set>");
        }

        if (args.Length < 2) {
            SendUsage();
            return;
        }

        var action = args[1];
        if (action == "set") {
            HandleSet(commandSender, args);
        } else {
            SendUsage();
        }
    }

    /// <summary>
    /// Handle the set sub-command.
    /// </summary>
    /// <param name="commandSender">The command sender that executed this command.</param>
    /// <param name="args">A string array containing the arguments for this command. The first argument is
    /// the command trigger or alias.</param>
    private void HandleSet(ICommandSender commandSender, string[] args) {
        if (args.Length < 3) {
            commandSender.SendMessage($"Invalid usage: {Trigger} set [setting name]");
            return;
        }

        var settingName = args[2];

        var propertyInfos = typeof(ServerSettings).GetProperties();

        PropertyInfo settingProperty = null;
        foreach (var prop in propertyInfos) {
            // Check if the property equals the setting name given as argument ignoring capitalization
            // Also ignore the auto property, because it can't change value without extra behaviour
            if (prop.Name.ToLower().Equals(settingName.ToLower().Replace("_", ""))) {
                settingProperty = prop;
                break;
            }
        }

        if (settingProperty == null || !settingProperty.CanRead) {
            commandSender.SendMessage($"Could not find setting with name: {settingName}");
            return;
        }

        if (args.Length < 4) {
            // User did not provide value to write setting, so we print the value
            var currentValue = settingProperty.GetValue(_settings);

            commandSender.SendMessage($"Setting '{settingName}' currently has value: {currentValue}");
            return;
        }

        if (!settingProperty.CanWrite) {
            commandSender.SendMessage($"Could not change value of setting with name: {settingName} (non-writable)");
            return;
        }

        var newValueString = args[3];
        object newValueObject;

        if (settingProperty.PropertyType == typeof(int)) {
            if (!int.TryParse(newValueString, out var newValueInt)) {
                commandSender.SendMessage("Please provide an integer value for this setting");
                return;
            }

            newValueObject = newValueInt;
        } else if (settingProperty.PropertyType == typeof(bool)) {
            if (!bool.TryParse(newValueString, out var newValueBool)) {
                commandSender.SendMessage("Please provide a boolean value for this setting");
                return;
            }

            newValueObject = newValueBool;
        } else {
            commandSender.SendMessage(
                $"Could not change value of setting with name: {settingName} (unhandled type)");
            return;
        }

        settingProperty.SetValue(_settings, newValueObject);

        commandSender.SendMessage($"Changed setting '{settingName}' to: {newValueObject}");

        _settings.SaveToFile();
    }
}