using System;

namespace HkmpVoiceChat.Server;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SettingAliasAttribute : Attribute {
    public string[] Aliases { get; private set; }

    public SettingAliasAttribute(params string[] aliases) {
        Aliases = aliases;
    }
}