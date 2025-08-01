// <copyright file="VersionUpdateViewModel.cs" company="MaaAssistantArknights">
// Part of the MaaWpfGui project, maintained by the MaaAssistantArknights team (Maa Team)
// Copyright (C) 2021-2025 MaaAssistantArknights Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License v3.0 only as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY
// </copyright>

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using MaaWpfGui.Constants;
using MaaWpfGui.Extensions;
using MaaWpfGui.Helper;
using MaaWpfGui.Main;
using MaaWpfGui.Models;
using MaaWpfGui.Services;
using MaaWpfGui.States;
using MaaWpfGui.Utilities;
using MaaWpfGui.ViewModels.UserControl.Settings;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Semver;
using Serilog;
using Stylet;
using SearchOption = System.IO.SearchOption;

namespace MaaWpfGui.ViewModels.UI;

/// <summary>
/// The view model of version update.
/// </summary>
public class VersionUpdateViewModel : Screen
{
    private readonly RunningState _runningState;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionUpdateViewModel"/> class.
    /// </summary>
    public VersionUpdateViewModel()
    {
        _runningState = RunningState.Instance;
    }

    private static readonly ILogger _logger = Log.ForContext<VersionUpdateViewModel>();

    private static string AddContributorLink(string text)
    {
        /*
        //        "@ " -> "@ "
        //       "`@`" -> "`@`"
        //   "@MistEO" -> "[@MistEO](https://github.com/MistEO)"
        // "[@MistEO]" -> "[@MistEO]"
        */
        return Regex.Replace(text, @"([^\[`]|^)@([^\s]+)", "$1[@$2](https://github.com/$2)");
    }

    private readonly string _curVersion = Marshal.PtrToStringAnsi(MaaService.AsstGetVersion()) ?? "0.0.1";
    private string _latestVersion = string.Empty;

    private string _updateTag = ConfigurationHelper.GetGlobalValue(ConfigurationKeys.VersionName, string.Empty);

    /// <summary>
    /// Gets or sets the update tag.
    /// </summary>
    public string UpdateTag
    {
        get => _updateTag;
        set
        {
            SetAndNotify(ref _updateTag, value);
            ConfigurationHelper.SetGlobalValue(ConfigurationKeys.VersionName, value);
        }
    }

    private string _updateInfo = ConfigurationHelper.GetGlobalValue(ConfigurationKeys.VersionUpdateBody, string.Empty);

    // private static readonly MarkdownPipeline s_markdownPipeline = new MarkdownPipelineBuilder().UseXamlSupportedExtensions().Build();

    /// <summary>
    /// Gets or sets the update info.
    /// </summary>
    public string UpdateInfo
    {
        get
        {
            try
            {
                return AddContributorLink(_updateInfo);
            }
            catch
            {
                return _updateInfo;
            }
        }

        set
        {
            SetAndNotify(ref _updateInfo, value);
            ConfigurationHelper.SetGlobalValue(ConfigurationKeys.VersionUpdateBody, value);
        }
    }

    private string _updateUrl = string.Empty;

    /// <summary>
    /// Gets or sets the update URL.
    /// </summary>
    public string UpdateUrl
    {
        get => _updateUrl;
        set => SetAndNotify(ref _updateUrl, value);
    }

    private bool _isFirstBootAfterUpdate = Convert.ToBoolean(ConfigurationHelper.GetGlobalValue(ConfigurationKeys.VersionUpdateIsFirstBoot, bool.FalseString));

    /// <summary>
    /// Gets or sets a value indicating whether it is the first boot after updating.
    /// </summary>
    public bool IsFirstBootAfterUpdate
    {
        get => _isFirstBootAfterUpdate;
        set
        {
            SetAndNotify(ref _isFirstBootAfterUpdate, value);
            ConfigurationHelper.SetGlobalValue(ConfigurationKeys.VersionUpdateIsFirstBoot, value.ToString());
        }
    }

    private string _updatePackageName = ConfigurationHelper.GetGlobalValue(ConfigurationKeys.VersionUpdatePackage, string.Empty);

    /// <summary>
    /// Gets or sets the name of the update package.
    /// </summary>
    public string UpdatePackageName
    {
        get => _updatePackageName;
        set
        {
            SetAndNotify(ref _updatePackageName, value);
            ConfigurationHelper.SetGlobalValue(ConfigurationKeys.VersionUpdatePackage, value);
        }
    }

    /// <summary>
    /// Gets the OS architecture.
    /// </summary>
    private static string OsArchitecture => RuntimeInformation.OSArchitecture.ToString().ToLower();

    /// <summary>
    /// Gets a value indicating whether the OS is arm.
    /// </summary>
    public static bool IsArm => OsArchitecture.StartsWith("arm");

    /*
    private const string RequestUrl = "repos/MaaAssistantArknights/MaaRelease/releases";
    private const string StableRequestUrl = "repos/MaaAssistantArknights/MaaAssistantArknights/releases/latest";
    private const string MaaReleaseRequestUrlByTag = "repos/MaaAssistantArknights/MaaRelease/releases/tags/";
    private const string InfoRequestUrl = "repos/MaaAssistantArknights/MaaAssistantArknights/releases/tags/";
    */

    private const string MaaUpdateApi = "version/summary.json";

    private JObject? _latestJson;
    private JObject? _assetsObject;

    private string? _mirrorcDownloadUrl;
    private string? _mirrorcVersionName;
    private string? _mirrorcReleaseNote;

    /// <summary>
    /// 检查是否有已下载的更新包
    /// </summary>
    /// <returns>操作成功返回 <see langword="true"/>，反之则返回 <see langword="false"/>。</returns>
    public bool CheckAndUpdateNow()
    {
        if (UpdateTag == string.Empty || UpdatePackageName == string.Empty || !File.Exists(UpdatePackageName))
        {
            return false;
        }

        {
            using var toast = new ToastNotification(LocalizationHelper.GetString("NewVersionZipFileFoundTitle"));
            toast.AppendContentText(LocalizationHelper.GetString("NewVersionZipFileFoundDescDecompressing"))
                .AppendContentText(UpdateTag)
                .ShowUpdateVersion(row: 2);
        }

        string curDir = Directory.GetCurrentDirectory();
        string extractDir = Path.Combine(curDir, "NewVersionExtract"); // 新版本解压的路径
        string oldFileDir = Path.Combine(curDir, ".old");

        // 解压
        try
        {
            if (Directory.Exists(extractDir))
            {
                Directory.Delete(extractDir, true);
            }

            ZipFile.ExtractToDirectory(UpdatePackageName, extractDir);
        }
        catch (InvalidDataException)
        {
            File.Delete(UpdatePackageName);
            {
                using var toast = new ToastNotification(LocalizationHelper.GetString("NewVersionZipFileBrokenTitle"));
                toast.AppendContentText(LocalizationHelper.GetString("NewVersionZipFileBrokenDescFilename") + UpdatePackageName)
                    .AppendContentText(LocalizationHelper.GetString("NewVersionZipFileBrokenDescDeleted"))
                    .ShowUpdateVersion();
            }

            return false;
        }

        string removeListFile = Path.Combine(extractDir, "removelist.txt");
        string mirrorChyanChangeFile = Path.Combine(extractDir, "changes.json");
        bool isOTAPackage = File.Exists(removeListFile) || File.Exists(mirrorChyanChangeFile);
        string[] removeList = [];
        if (File.Exists(removeListFile))
        {
            removeList = File.ReadAllLines(removeListFile);
        }

        if (File.Exists(mirrorChyanChangeFile))
        {
            try
            {
                string json = File.ReadAllText(mirrorChyanChangeFile);
                var jObject = JObject.Parse(json);
                removeList = jObject["deleted"]?.ToObject<string[]>() ?? [];
            }
            catch (Exception e)
            {
                _logger.Error("parse mirrorChyan changes.json error: {EMessage}", e.Message);
            }
        }

        if (removeList.Length > 0)
        {
            foreach (string file in removeList)
            {
                string path = Path.Combine(curDir, file);
                if (!File.Exists(path))
                {
                    continue;
                }

                string moveTo = Path.Combine(oldFileDir, file);
                if (File.Exists(moveTo))
                {
                    DeleteFileWithBackup(moveTo);
                }
                else
                {
                    var dir = Path.GetDirectoryName(moveTo);
                    if (dir != null)
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                try
                {
                    File.Move(path, moveTo);
                }
                catch (Exception e)
                {
                    _logger.Error("move file error, path: {Path}, moveTo: {MoveTo}, error: {EMessage}", path, moveTo, e.Message);
                    throw;
                }
            }
        }
        else if (!isOTAPackage)
        {
            List<Task> deleteTasks = [];
            foreach (var dir in Directory.GetDirectories(extractDir))
            {
                deleteTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        FileSystem.DeleteDirectory(dir.Replace(extractDir, curDir), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    catch
                    {
                        _logger.Error("delete directory error, dir: {Dir}", dir);
                    }
                }));
            }

            Task.WaitAll([.. deleteTasks]);
        }

        Directory.CreateDirectory(oldFileDir);
        foreach (var dir in Directory.GetDirectories(extractDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dir.Replace(extractDir, curDir));
            Directory.CreateDirectory(dir.Replace(extractDir, oldFileDir));
        }

        // 复制新版本的所有文件到当前路径下
        foreach (var file in Directory.GetFiles(extractDir, "*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(file);

            // ReSharper disable once StringLiteralTypo
            if (fileName == "removelist.txt")
            {
                continue;
            }

            string curFileName = file.Replace(extractDir, curDir);
            try
            {
                if (File.Exists(curFileName))
                {
                    string moveTo = file.Replace(extractDir, oldFileDir);
                    if (File.Exists(moveTo))
                    {
                        DeleteFileWithBackup(moveTo);
                    }

                    File.Move(curFileName, moveTo);
                }

                File.Move(file, curFileName);
            }
            catch (Exception e)
            {
                _logger.Error("move file error, file name: {File}, error: {EMessage}", file, e.Message);
                throw;
            }
        }

        // 操作完了，把解压的文件删了
        Directory.Delete(extractDir, true);
        File.Delete(UpdatePackageName);

        // 保存更新信息，下次启动后会弹出已更新完成的提示
        UpdatePackageName = string.Empty;
        IsFirstBootAfterUpdate = true;
        return true;

        static void DeleteFileWithBackup(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                _logger.Error("delete file error, filePath: {FilePath}, error: {EMessage}, try to backup.", filePath, e.Message);
                int index = 0;
                string currentDate = DateTime.Now.ToString("yyyyMMddHHmm");
                string backupFilePath = $"{filePath}.{currentDate}.{index}";

                while (File.Exists(backupFilePath))
                {
                    index++;
                    backupFilePath = $"{filePath}.{currentDate}.{index}";
                }

                try
                {
                    File.Move(filePath, backupFilePath);
                }
                catch (Exception e1)
                {
                    _logger.Error("move file error, path: {FilePath}, moveTo: {BackupFilePath}, error: {E1Message}", filePath, backupFilePath, e1.Message);
                    throw;
                }
            }
        }
    }

    public enum CheckUpdateRetT
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        // ReSharper disable once InconsistentNaming
        OK,

        /// <summary>
        /// 未知错误
        /// </summary>
        UnknownError,

        /// <summary>
        /// 无需更新
        /// </summary>
        NoNeedToUpdate,

        /// <summary>
        /// 调试版本无需更新
        /// </summary>
        NoNeedToUpdateDebugVersion,

        /// <summary>
        /// 已经是最新版
        /// </summary>
        AlreadyLatest,

        /// <summary>
        /// 网络错误
        /// </summary>
        NetworkError,

        /// <summary>
        /// 获取信息失败
        /// </summary>
        FailedToGetInfo,

        /// <summary>
        /// 新版正在构建中
        /// </summary>
        NewVersionIsBeingBuilt,

        /// <summary>
        /// 只更新了游戏资源
        /// </summary>
        OnlyGameResourceUpdated,

        /// <summary>
        /// NoMirrorChyanCdk
        /// </summary>
        NoMirrorChyanCdk,
    }

    public enum AppUpdateSource
    {
        /// <summary>
        /// Maa API
        /// </summary>
        MaaApi,

        /// <summary>
        /// MirrorChyan
        /// </summary>
        MirrorChyan,
    }

    private bool _doNotShowUpdate = Convert.ToBoolean(ConfigurationHelper.GetGlobalValue(ConfigurationKeys.VersionUpdateDoNotShowUpdate, bool.FalseString));

    /// <summary>
    /// Gets or sets a value indicating whether to show the update.
    /// </summary>
    public bool DoNotShowUpdate
    {
        get => _doNotShowUpdate;
        set
        {
            SetAndNotify(ref _doNotShowUpdate, value);
            ConfigurationHelper.SetGlobalValue(ConfigurationKeys.VersionUpdateDoNotShowUpdate, value.ToString());
        }
    }

    /// <summary>
    /// 如果是在更新后第一次启动，显示ReleaseNote弹窗，否则检查更新并下载更新包。
    /// </summary>
    /// <returns>Task</returns>
    public async Task ShowUpdateOrDownload()
    {
        if (IsFirstBootAfterUpdate)
        {
            IsFirstBootAfterUpdate = false;
            if (!DoNotShowUpdate)
            {
                Instances.WindowManager.ShowWindow(this);
            }
        }
        else
        {
            if (!SettingsViewModel.VersionUpdateSettings.StartupUpdateCheck)
            {
                return;
            }

            if (!IsDebugVersion())
            {
                if (SettingsViewModel.VersionUpdateSettings.UpdateSource == "MirrorChyan" && string.IsNullOrEmpty(SettingsViewModel.VersionUpdateSettings.MirrorChyanCdk))
                {
                    _ = Task.Run(() =>
                        MessageBoxHelper.Show(
                            LocalizationHelper.GetString("MirrorChyanSelectedButNoCdk"),
                            "cdk is empty!",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning,
                            ok: LocalizationHelper.GetString("Ok")));
                }

                await VersionUpdateAndAskToRestartAsync();
                await ResourceUpdater.ResourceUpdateAndReloadAsync();
            }
            else
            {
                // await ResourceUpdater.CheckAndDownloadResourceUpdate();
                // 跑个空任务避免 async warning
                await Task.Run(() => { });
            }
        }
    }

    /// <summary>
    /// 检查更新并下载更新包，如果成功则提示重启。
    /// </summary>
    /// <returns>Task</returns>
    public async Task VersionUpdateAndAskToRestartAsync()
    {
        if (SettingsViewModel.VersionUpdateSettings.IsCheckingForUpdates)
        {
            return;
        }

        var ret = await CheckAndDownloadVersionUpdate();
        if (ret == CheckUpdateRetT.OK)
        {
            _ = AskToRestart();
        }
    }

    /// <summary>
    /// 检查更新，并下载更新包。
    /// </summary>
    /// <returns>操作成功返回 <see langword="true"/>，反之则返回 <see langword="false"/>。</returns>
    public async Task<CheckUpdateRetT> CheckAndDownloadVersionUpdate()
    {
        try
        {
            SettingsViewModel.VersionUpdateSettings.IsCheckingForUpdates = true;

            var (checkRet, source) = await CheckUpdate();

            if (checkRet != CheckUpdateRetT.OK)
            {
                return checkRet;
            }

            return source switch
            {
                AppUpdateSource.MaaApi => await HandleUpdateFromMaaApi(),
                AppUpdateSource.MirrorChyan => await HandleUpdateFromMirrorChyan(),
                _ => CheckUpdateRetT.UnknownError,
            };
        }
        finally
        {
            SettingsViewModel.VersionUpdateSettings.IsCheckingForUpdates = false;
        }
    }

    private async Task<CheckUpdateRetT> HandleUpdateFromMaaApi()
    {
        // 保存新版本的信息
        var name = _latestJson?["name"]?.ToString();
        UpdateTag = string.IsNullOrEmpty(name) ? (_latestJson?["tag_name"]?.ToString() ?? string.Empty) : name;
        SettingsViewModel.VersionUpdateSettings.NewVersionFoundInfo = $"{LocalizationHelper.GetString("NewVersionFoundTitle")}: {UpdateTag}";
        var body = _latestJson?["body"]?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(body))
        {
            var curHash = ComparableHash(_curVersion);
            var latestHash = ComparableHash(_latestVersion);

            if (curHash != null && latestHash != null)
            {
                body = $"**Full Changelog**: [{curHash} -> {latestHash}](https://github.com/MaaAssistantArknights/MaaAssistantArknights/compare/{curHash}...{latestHash})";
            }
        }

        UpdateInfo = body;
        UpdateUrl = _latestJson?["html_url"]?.ToString() ?? string.Empty;

        bool otaFound = _assetsObject != null;
        bool goDownload = otaFound && SettingsViewModel.VersionUpdateSettings.AutoDownloadUpdatePackage;

        ShowUpdateInfo(otaFound, LocalizationHelper.GetString("NewVersionFoundButtonGoWebpage"), true);

        UpdatePackageName = _assetsObject?["name"]?.ToString() ?? string.Empty;

        if (!goDownload || string.IsNullOrWhiteSpace(UpdatePackageName))
        {
            OutputDownloadProgress(string.Empty);
            return CheckUpdateRetT.NoNeedToUpdate;
        }

        if (_assetsObject == null)
        {
            return CheckUpdateRetT.FailedToGetInfo;
        }

        string? rawUrl = _assetsObject["browser_download_url"]?.ToString();
        var urls = new List<string>();

        if (SettingsViewModel.VersionUpdateSettings.UpdateSource == "Github" && !SettingsViewModel.VersionUpdateSettings.ForceGithubGlobalSource)
        {
            var mirrors = _assetsObject["mirrors"]?.ToObject<List<string>>();

            if (mirrors != null)
            {
                urls.AddRange(mirrors);
            }
        }

        // 负载均衡
        // var rand = new Random();
        // urls = urls.OrderBy(_ => rand.Next()).ToList();
        if (rawUrl != null)
        {
            urls.Add(rawUrl);
        }

        _logger.Information("Start test legacy download urls");

        // run latency test parallel
        var tasks = urls.ConvertAll(url => Instances.HttpService.HeadAsync(new Uri(url)));
        var latencies = await Task.WhenAll(tasks);

        var proxy = ConfigurationHelper.GetValue(ConfigurationKeys.UpdateProxy, string.Empty);
        var hasProxy = !string.IsNullOrEmpty(proxy);

        // select the fastest mirror
        _logger.Information("Selecting the fastest mirror:");
        var selected = 0;
        for (int i = 0; i < latencies.Length; i++)
        {
            // ReSharper disable once StringLiteralTypo
            var isInChina = urls[i].Contains("s3.maa-org.net") || urls[i].Contains("maa-ota.annangela.cn");

            if (latencies[i] < 0)
            {
                _logger.Warning("\turl: {CDNUrl} not available", urls[i]);
                continue;
            }

            _logger.Information("\turl: {CDNUrl}, legacy: {1:0.00}ms", urls[i], latencies[i]);

            if (hasProxy && isInChina)
            {
                // 如果设置了代理，国内镜像的延迟加上一个固定值
                latencies[i] += 6480;
            }

            if (latencies[selected] < 0 || (latencies[i] >= 0 && latencies[i] < latencies[selected]))
            {
                selected = i;
            }
        }

        if (latencies[selected] < 0)
        {
            _logger.Error("All mirrors are not available");
            OutputDownloadProgress(downloading: false, output: LocalizationHelper.GetString("NewVersionDownloadFailedTitle"));
            return CheckUpdateRetT.NetworkError;
        }

        _logger.Information("Selected mirror: {CDNUrl}", urls[selected]);

        var downloaded = await DownloadGithubAssets(urls[selected], _assetsObject);
        if (downloaded)
        {
            OutputDownloadProgress(downloading: false, output: LocalizationHelper.GetString("NewVersionDownloadCompletedTitle"));
        }
        else
        {
            OutputDownloadProgress(downloading: false, output: LocalizationHelper.GetString("NewVersionDownloadFailedTitle"));
            {
                using var toast = new ToastNotification(LocalizationHelper.GetString("NewVersionDownloadFailedTitle"));
                toast.AppendContentText(LocalizationHelper.GetString("NewVersionDownloadFailedDesc"))
                    .AddButton(LocalizationHelper.GetString("NewVersionFoundButtonGoWebpage"), ToastNotification.GetActionTagForOpenWeb(UpdateUrl))
                    .Show();
            }

            return CheckUpdateRetT.NoNeedToUpdate;
        }

        return CheckUpdateRetT.OK;

        string? ComparableHash(string version)
        {
            if (IsStdVersion(version) || IsBetaVersion(version))
            {
                return version;
            }

            if (!SemVersion.TryParse(version, SemVersionStyles.AllowLowerV, out var semVersion) ||
                !IsNightlyVersion(semVersion))
            {
                return null;
            }

            // v4.6.6-1.g{Hash}
            // v4.6.7-beta.2.8.g{Hash}
            var commitHash = semVersion.PrereleaseIdentifiers[^1].ToString();
            if (commitHash.StartsWith('g'))
            {
                commitHash = commitHash.Remove(0, 1);
            }

            return commitHash;
        }
    }

    private void ShowUpdateInfo(bool otaFound, string? text, bool globalSource)
    {
        bool goDownload = otaFound && SettingsViewModel.VersionUpdateSettings.AutoDownloadUpdatePackage;

        using var toast = new ToastNotification((otaFound ? LocalizationHelper.GetString("NewVersionFoundTitle") : LocalizationHelper.GetString("NewVersionFoundButNoPackageTitle")) + " : " + UpdateTag);
        if (goDownload)
        {
            OutputDownloadProgress(LocalizationHelper.GetString("NewVersionDownloadPreparing"), false, globalSource);
            toast.AppendContentText(globalSource
                ? LocalizationHelper.GetString("NewVersionFoundDescDownloadingWithGlobalSource")
                : LocalizationHelper.GetString("NewVersionFoundDescDownloadingWithMirrorChyan"));
        }

        if (!otaFound)
        {
            toast.AppendContentText(LocalizationHelper.GetString("NewVersionFoundButNoPackageDesc"));
        }

        int count = 0;
        foreach (var line in UpdateInfo.Split('\n'))
        {
            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            toast.AppendContentText(line);
            if (++count >= 10)
            {
                break;
            }
        }

        if (!string.IsNullOrEmpty(text))
        {
            toast.AddButton(text, ToastNotification.GetActionTagForOpenWeb(globalSource ? UpdateUrl : MaaUrls.MirrorChyanManualUpdate));
        }

        toast.ShowUpdateVersion();
    }

    private async Task<CheckUpdateRetT> HandleUpdateFromMirrorChyan()
    {
        if (string.IsNullOrEmpty(_mirrorcDownloadUrl))
        {
            return CheckUpdateRetT.FailedToGetInfo;
        }

        UpdateTag = _mirrorcVersionName ?? string.Empty;
        UpdateInfo = _mirrorcReleaseNote ?? string.Empty;
        SettingsViewModel.VersionUpdateSettings.NewVersionFoundInfo = $"{LocalizationHelper.GetString("NewVersionFoundTitle")}: {UpdateTag}";

        bool goDownload = SettingsViewModel.VersionUpdateSettings.AutoDownloadUpdatePackage;

        ShowUpdateInfo(true, LocalizationHelper.GetString("NewVersionFoundButtonGoWebpage"), false);

        if (!goDownload)
        {
            OutputDownloadProgress(string.Empty);
            return CheckUpdateRetT.NoNeedToUpdate;
        }

        UpdatePackageName = "MirrorChyanApp" + _mirrorcVersionName + ".zip";
        var downloaded = await DownloadFromMirrorChyan(_mirrorcDownloadUrl,
                    UpdatePackageName);

        if (downloaded)
        {
            OutputDownloadProgress(downloading: false, output: LocalizationHelper.GetString("NewVersionDownloadCompletedTitle"));
        }
        else
        {
            OutputDownloadProgress(downloading: false, output: LocalizationHelper.GetString("NewVersionDownloadFailedTitle"));
            {
                using var toast = new ToastNotification(LocalizationHelper.GetString("NewVersionDownloadFailedTitle"));
                toast.AppendContentText(LocalizationHelper.GetString("NewVersionDownloadFailedDesc"))
                     .Show();
            }

            return CheckUpdateRetT.NoNeedToUpdate;
        }

        AchievementTrackerHelper.Instance.Unlock(AchievementIds.MirrorChyanFirstUse);
        return CheckUpdateRetT.OK;
    }

    public async Task AskToRestart()
    {
        if (SettingsViewModel.VersionUpdateSettings.AutoInstallUpdatePackage)
        {
            await Bootstrapper.RestartAfterIdleAsync();
            return;
        }

        await _runningState.UntilIdleAsync(10000);

        var result = MessageBoxHelper.Show(
            LocalizationHelper.GetString("NewVersionDownloadCompletedDesc"),
            LocalizationHelper.GetString("NewVersionDownloadCompletedTitle"),
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question,
            ok: LocalizationHelper.GetString("Ok"),
            cancel: LocalizationHelper.GetString("ManualRestart"));
        if (result == MessageBoxResult.OK)
        {
            Bootstrapper.ShutdownAndRestartWithoutArgs();
        }
    }

    /// <summary>
    /// 检查更新。
    /// </summary>
    /// <returns>检查到更新返回 <see langword="true"/>，反之则返回 <see langword="false"/>。</returns>
    private async Task<(CheckUpdateRetT Ret, AppUpdateSource? Source)> CheckUpdate()
    {
        // 调试版不检查更新
        if (IsDebugVersion())
        {
            return (CheckUpdateRetT.NoNeedToUpdateDebugVersion, null);
        }

        if (SettingsViewModel.VersionUpdateSettings.UpdateSource == "MirrorChyan")
        {
            try
            {
                var ret = await CheckUpdateByMirrorChyan();
                if (ret is CheckUpdateRetT.OK or CheckUpdateRetT.AlreadyLatest)
                {
                    return (ret, AppUpdateSource.MirrorChyan);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to check update by MirrorChyan, rollback to maaApi");
            }
        }

        try
        {
            var ret = await CheckUpdateByMaaApi();
            return (ret, AppUpdateSource.MaaApi);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to check update by Maa API.");
            return (CheckUpdateRetT.FailedToGetInfo, AppUpdateSource.MaaApi);
        }
    }

    private async Task<CheckUpdateRetT> CheckUpdateByMaaApi()
    {
        JObject json = await Instances.MaaApiService.RequestMaaApiWithCache(MaaUpdateApi);

        if (json is null)
        {
            _logger.Error("Failed to get update info from Maa API.");
            return CheckUpdateRetT.FailedToGetInfo;
        }

        string versionType = SettingsViewModel.VersionUpdateSettings.VersionType switch
        {
            VersionUpdateSettingsUserControlModel.UpdateVersionType.Beta => "version/beta.json",
            VersionUpdateSettingsUserControlModel.UpdateVersionType.Nightly => "version/alpha.json",
            _ => "version/stable.json",
        };

        var latestVersion = json[versionType]?["version"]?.ToString();

        latestVersion ??= string.Empty;

        if (!NeedToUpdate(latestVersion))
        {
            return CheckUpdateRetT.AlreadyLatest;
        }

        return await GetVersionDetailsByMaaApi(versionType);
    }

    private async Task<CheckUpdateRetT> GetVersionDetailsByMaaApi(string versionType)
    {
        var json = await Instances.MaaApiService.RequestMaaApiWithCache(versionType);
        if (json is null)
        {
            return CheckUpdateRetT.FailedToGetInfo;
        }

        string? latestVersion = json["version"]?.ToString();
        if (string.IsNullOrEmpty(latestVersion))
        {
            return CheckUpdateRetT.FailedToGetInfo;
        }

        if (!NeedToUpdate(latestVersion))
        {
            return CheckUpdateRetT.AlreadyLatest;
        }

        _latestVersion = latestVersion;
        _latestJson = json["details"] as JObject;
        if (_latestJson == null)
        {
            return CheckUpdateRetT.FailedToGetInfo;
        }

        _assetsObject = null;

        JObject? fullPackage = null;

        var curVersionLower = _curVersion.ToLower();
        var latestVersionLower = _latestVersion.ToLower();
        foreach (var curAssets in ((JArray?)_latestJson["assets"])!)
        {
            string? name = curAssets["name"]?.ToString().ToLower();
            if (name == null)
            {
                continue;
            }

            if (IsArm ^ name.Contains("arm"))
            {
                continue;
            }

            if (!name.Contains("win"))
            {
                continue;
            }

            if (name.Contains($"maa-{latestVersionLower}-"))
            {
                fullPackage = curAssets as JObject;
            }

            // ReSharper disable once InvertIf
            if (name.Contains("ota") && name.Contains($"{curVersionLower}_{latestVersionLower}"))
            {
                _assetsObject = curAssets as JObject;
                break;
            }
        }

        if (_assetsObject == null && fullPackage != null)
        {
            _assetsObject = fullPackage;
            _logger.Warning("No OTA package found, but full package found.");
            using var toast = new ToastNotification(LocalizationHelper.GetString("NewVersionNoOtaPackage"));
            toast.Show(30);
            Instances.TaskQueueViewModel.AddLog(LocalizationHelper.GetString("NewVersionNoOtaPackage"), UiLogColor.Warning);
        }

        return CheckUpdateRetT.OK;
    }

    private async Task<CheckUpdateRetT> CheckUpdateByMirrorChyan()
    {
        var cdk = SettingsViewModel.VersionUpdateSettings.MirrorChyanCdk.Trim();
        var arch = IsArm ? "arm64" : "x64";
        string channel = SettingsViewModel.VersionUpdateSettings.VersionType switch
        {
            VersionUpdateSettingsUserControlModel.UpdateVersionType.Beta => "beta",
            VersionUpdateSettingsUserControlModel.UpdateVersionType.Nightly => "alpha",
            _ => "stable",
        };
        var spid = HardwareInfoUtility.GetMachineGuid().StableHash();

        var url = $"{MaaUrls.MirrorChyanAppUpdate}?current_version={_curVersion}&cdk={cdk}&user_agent=MaaWpfGui&os=win&arch={arch}&channel={channel}&sp_id={spid}";

        HttpResponseMessage? response = null;
        try
        {
            response = await Instances.HttpService.GetAsync(new(url), uriPartial: UriPartial.Path);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to send GET request to {Uri}", new Uri(url).GetLeftPart(UriPartial.Path));
            _logger.Information("current_version: {CurVersion}, cdk: {Mask}, arch: {Arch}, channel: {Channel}", _curVersion, cdk.Mask(), arch, channel);
        }

        if (response is null)
        {
            _logger.Error("mirrorc failed");
            return CheckUpdateRetT.NetworkError;
        }

        var jsonStr = await response.Content.ReadAsStringAsync();
        _logger.Information("{JsonStr}", jsonStr);
        JObject? data = null;
        try
        {
            data = (JObject?)JsonConvert.DeserializeObject(jsonStr);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to deserialize json");
        }

        if (data is null)
        {
            return CheckUpdateRetT.UnknownError;
        }

        var errorCode = data["code"]?.ToObject<Enums.MirrorChyanErrorCode>() ?? Enums.MirrorChyanErrorCode.Undivided;
        if (errorCode != Enums.MirrorChyanErrorCode.Success)
        {
            switch (errorCode)
            {
                case Enums.MirrorChyanErrorCode.KeyExpired:
                    ToastNotification.ShowDirect(LocalizationHelper.GetString("MirrorChyanCdkExpired"));
                    break;
                case Enums.MirrorChyanErrorCode.KeyInvalid:
                    ToastNotification.ShowDirect(LocalizationHelper.GetString("MirrorChyanCdkInvalid"));
                    AchievementTrackerHelper.Instance.Unlock(AchievementIds.MirrorChyanCdkError);
                    break;
                case Enums.MirrorChyanErrorCode.ResourceQuotaExhausted:
                    ToastNotification.ShowDirect(LocalizationHelper.GetString("MirrorChyanCdkQuotaExhausted"));
                    break;
                case Enums.MirrorChyanErrorCode.KeyMismatched:
                    ToastNotification.ShowDirect(LocalizationHelper.GetString("MirrorChyanCdkMismatched"));
                    break;
                case Enums.MirrorChyanErrorCode.InvalidParams:
                case Enums.MirrorChyanErrorCode.ResourceNotFound:
                case Enums.MirrorChyanErrorCode.InvalidOs:
                case Enums.MirrorChyanErrorCode.InvalidArch:
                case Enums.MirrorChyanErrorCode.InvalidChannel:
                case Enums.MirrorChyanErrorCode.Undivided:
                    ToastNotification.ShowDirect(data["msg"]?.ToString() ?? LocalizationHelper.GetString("GameResourceFailed"));
                    break;
            }

            return CheckUpdateRetT.UnknownError;
        }

        var version = data["data"]?["version_name"]?.ToString();
        if (string.IsNullOrEmpty(version))
        {
            return CheckUpdateRetT.UnknownError;
        }

        if (!NeedToUpdate(version))
        {
            return CheckUpdateRetT.AlreadyLatest;
        }

        if (data["data"]?["update_type"]?.ToObject<string>() == "full")
        {
            using var toast = new ToastNotification(LocalizationHelper.GetString("NewVersionNoOtaPackage"));
            toast.Show(30);
            _logger.Warning("No OTA package found, but full package found.");
            Instances.TaskQueueViewModel.AddLog(LocalizationHelper.GetString("NewVersionNoOtaPackage"), UiLogColor.Warning);
        }

        // 到这里已经确定有新版本了
        _logger.Information("New version found: {Version}", version);

        _mirrorcVersionName = version;
        _mirrorcReleaseNote = data["data"]?["release_note"]?.ToString();

        if (string.IsNullOrEmpty(cdk))
        {
            return CheckUpdateRetT.NoMirrorChyanCdk;
        }

        _mirrorcDownloadUrl = data["data"]?["url"]?.ToString();

        return CheckUpdateRetT.OK;
    }

    private bool NeedToUpdate(string latestVersion)
    {
        if (IsDebugVersion())
        {
            return false;
        }

        bool curParsed = SemVersion.TryParse(_curVersion, SemVersionStyles.AllowLowerV, out var curVersionObj);
        bool latestPared = SemVersion.TryParse(latestVersion, SemVersionStyles.AllowLowerV, out var latestVersionObj);
        if (curParsed && latestPared)
        {
            return curVersionObj.CompareSortOrderTo(latestVersionObj) < 0;
        }

        return string.CompareOrdinal(_curVersion, latestVersion) < 0;
    }

    /// <summary>
    /// 获取 GitHub Assets 对象对应的文件
    /// </summary>
    /// <param name="url">下载链接</param>
    /// <param name="assetsObject">Github Assets 对象</param>
    /// <returns>操作成功返回 true，反之则返回 false</returns>
    private static async Task<bool> DownloadGithubAssets(string url, JObject assetsObject)
    {
        try
        {
            return await Instances.HttpService.DownloadFileAsync(
                    new(url),
                    assetsObject["name"]!.ToString(),
                    assetsObject["content_type"]?.ToString())
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<bool> DownloadFromMirrorChyan(string url, string filename)
    {
        try
        {
            return await Instances.HttpService.DownloadFileAsync(
                    new(url), filename)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static ObservableCollection<LogItemViewModel>? _logItemViewModels;

    public static void OutputDownloadProgress(long value = 0, long maximum = 1, int len = 0, double ts = 1)
    {
        string progress = $"[{value / 1048576.0:F}MiB/{maximum / 1048576.0:F}MiB ({value * 100.0 / maximum:F}%)";

        double speedInKiBPerSecond = len / ts / 1024.0;

        var speedDisplay = speedInKiBPerSecond >= 1024
            ? $"{speedInKiBPerSecond / 1024.0:F} MiB/s"
            : $"{speedInKiBPerSecond:F} KiB/s";

        OutputDownloadProgress(progress + $" {speedDisplay}");
    }

    private static bool _globalSource = true;

    public static void OutputDownloadProgress(string output, bool downloading = true, bool? globalSource = null)
    {
        globalSource ??= _globalSource;
        _globalSource = globalSource.Value;
        if (_logItemViewModels == null)
        {
            try
            {
                _logItemViewModels = Instances.TaskQueueViewModel.LogItemViewModels;
            }
            catch
            {
                return;
            }

            if (_logItemViewModels == null)
            {
                return;
            }
        }

        string fullText;
        if (downloading)
        {
            string key = globalSource.Value ? "NewVersionFoundDescDownloadingWithGlobalSource" : "NewVersionFoundDescDownloadingWithMirrorChyan";
            fullText = LocalizationHelper.GetString(key) + "\n" + output;
        }
        else
        {
            fullText = output;
        }

        var log = new LogItemViewModel(fullText, UiLogColor.Download);

        Execute.OnUIThread(() =>
        {
            if (_logItemViewModels.Count > 0 && _logItemViewModels[0].Color == UiLogColor.Download)
            {
                if (!string.IsNullOrEmpty(output))
                {
                    _logItemViewModels[0] = log;
                }
                else
                {
                    _logItemViewModels.RemoveAt(0);
                }
            }
            else if (!string.IsNullOrEmpty(output))
            {
                _logItemViewModels.Clear();
                _logItemViewModels.Add(log);
            }
        });
    }

    public bool IsDebugVersion(string? version = null)
    {
        // return false;
        version ??= _curVersion;

        // match case 1: DEBUG VERSION
        // match case 2: v{Major}.{Minor}.{Patch}-{CommitDistance}-g{CommitHash}
        // match case 3: {CommitHash}
        return Regex.IsMatch(version, @"^(.*DEBUG.*|v\d+(\.\d+){1,3}-\d+-g[0-9a-f]{6,}|[^v][0-9a-f]{6,})$");
    }

    public bool IsStdVersion(string? version = null)
    {
        // 正式版：vX.X.X
        // DevBuild (CI)：yyyy-MM-dd-HH-mm-ss-{CommitHash[..7]}
        // DevBuild (Local)：yyyy-MM-dd-HH-mm-ss-{CommitHash[..7]}-Local
        // Release (Local Commit)：v.{CommitHash[..7]}-Local
        // Release (Local Tag)：{Tag}-Local
        // Debug (Local)：DEBUG VERSION
        // Script Compiled：c{CommitHash[..7]}
        version ??= _curVersion;

        if (IsDebugVersion(version))
        {
            return false;
        }

        if (version.StartsWith('c') || version.StartsWith("20") || version.Contains("Local"))
        {
            return false;
        }

        if (!SemVersion.TryParse(version, SemVersionStyles.AllowLowerV, out var semVersion))
        {
            return false;
        }

        return !semVersion.IsPrerelease;
    }

    public bool IsBetaVersion(string? version = null)
    {
        version ??= _curVersion;

        if (IsDebugVersion(version))
        {
            return false;
        }

        if (version.StartsWith('c') || version.StartsWith("20") || version.Contains("Local"))
        {
            return false;
        }

        if (!SemVersion.TryParse(version, SemVersionStyles.AllowLowerV, out var semVersion))
        {
            return false;
        }

        return semVersion.IsPrerelease && !IsNightlyVersion(semVersion);
    }

    public static bool IsNightlyVersion(SemVersion version)
    {
        if (!version.IsPrerelease)
        {
            return false;
        }

        // ReSharper disable once CommentTypo
        // v{Major}.{Minor}.{Patch}-{Prerelease}.{CommitDistance}.g{CommitHash}
        // v4.6.7-beta.2.1.g1234567
        // v4.6.8-5.g1234567
        var lastId = version.PrereleaseIdentifiers.LastOrDefault().ToString();
        return lastId.StartsWith('g') && lastId.Length >= 7;
    }
}
