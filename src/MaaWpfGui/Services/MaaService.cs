// <copyright file="MaaService.cs" company="MaaAssistantArknights">
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

using System;
using System.Runtime.InteropServices;
using MaaWpfGui.Main;
using Newtonsoft.Json.Linq;
using AsstHandle = System.IntPtr;
using AsstInstanceOptionKey = System.Int32;
using AsstTaskId = System.Int32;

namespace MaaWpfGui.Services
{
    public static class MaaService
    {
        public delegate void CallbackDelegate(int msg, IntPtr jsonBuffer, IntPtr customArg);

        public delegate void ProcCallbackMsg(AsstMsg msg, JObject details);

        [DllImport("MaaCore.dll")]
        public static extern AsstHandle AsstCreateEx(CallbackDelegate callback, IntPtr customArg);

        [DllImport("MaaCore.dll")]
        public static extern void AsstDestroy(AsstHandle handle);

        [DllImport("MaaCore.dll")]
        public static extern unsafe bool AsstSetInstanceOption(AsstHandle handle, AsstInstanceOptionKey key, byte* value);

        [DllImport("MaaCore.dll")]
        public static extern bool AsstSetStaticOption(AsstStaticOptionKey key, [MarshalAs(UnmanagedType.LPUTF8Str)]string value);

        [DllImport("MaaCore.dll")]
        public static extern unsafe bool AsstLoadResource(byte* dirname);

        [DllImport("MaaCore.dll")]
        public static extern unsafe bool AsstConnect(AsstHandle handle, byte* adbPath, byte* address, byte* config);

        [DllImport("MaaCore.dll")]
        public static extern unsafe AsstTaskId AsstAppendTask(AsstHandle handle, byte* type, byte* taskParams);

        [DllImport("MaaCore.dll")]
        public static extern unsafe bool AsstSetTaskParams(AsstHandle handle, AsstTaskId id, byte* taskParams);

        [DllImport("MaaCore.dll")]
        public static extern bool AsstStart(AsstHandle handle);

        [DllImport("MaaCore.dll")]
        public static extern bool AsstRunning(AsstHandle handle);

        [DllImport("MaaCore.dll")]
        public static extern bool AsstStop(AsstHandle handle);

        [DllImport("MaaCore.dll")]
        public static extern unsafe Int32 AsstAsyncScreencap(AsstHandle handle, bool block);

        [DllImport("MaaCore.dll")]
        public static extern unsafe ulong AsstGetImage(AsstHandle handle, byte* buff, ulong buffSize);

        [DllImport("MaaCore.dll")]
        public static extern unsafe ulong AsstGetImageBgr(AsstHandle handle, byte* buff, ulong buffSize);

        [DllImport("MaaCore.dll")]
        public static extern ulong AsstGetNullSize();

        [DllImport("MaaCore.dll")]
        public static extern IntPtr AsstGetVersion();

        [DllImport("MaaCore.dll")]
        public static extern bool AsstBackToHome(AsstHandle handle);

        [DllImport("MaaCore.dll")]
        public static extern unsafe void AsstSetConnectionExtras(byte* name, byte* extras);
    }

    public enum AsstTaskType : byte
    {
        /// <summary>
        /// 开始唤醒。
        /// </summary>
        StartUp = 0,

        /// <summary>
        /// 关闭明日方舟
        /// </summary>
        CloseDown,

        /// <summary>
        /// 刷理智
        /// </summary>
        Fight,

        /// <summary>
        /// 领取奖励
        /// </summary>
        Award,

        /// <summary>
        /// 信用商店
        /// </summary>
        Mall,

        /// <summary>
        /// 基建
        /// </summary>
        Infrast,

        /// <summary>
        /// 招募
        /// </summary>
        Recruit,

        /// <summary>
        /// 肉鸽
        /// </summary>
        Roguelike,

        /// <summary>
        /// 自动战斗
        /// </summary>
        Copilot,

        /// <summary>
        /// 自动战斗-保全ver
        /// </summary>
        SSSCopilot,

        /// <summary>
        /// 单步任务（目前仅支持战斗）
        /// </summary>
        SingleStep,

        /// <summary>
        /// 视频识别
        /// </summary>
        VideoRecognition,

        /// <summary>
        /// 仓库识别
        /// </summary>
        Depot,

        /// <summary>
        /// 干员识别
        /// </summary>
        OperBox,

        /// <summary>
        /// 生息演算
        /// </summary>
        Reclamation,

        /// <summary>
        /// 自定义任务
        /// </summary>
        Custom,
    }
}
