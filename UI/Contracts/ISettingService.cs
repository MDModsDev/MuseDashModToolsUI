﻿using System.Threading.Tasks;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ISettingService
{
    public Setting Settings { get; }
    Task InitializeSettings();
    Task<bool> OnChoosePath();
}