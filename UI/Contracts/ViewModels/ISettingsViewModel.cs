﻿namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface ISettingsViewModel
{
    string[] AskTypes { get; set; }
    void Initialize();
}