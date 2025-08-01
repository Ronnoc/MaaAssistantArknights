// <copyright file="GenericCombinedData.cs" company="MaaAssistantArknights">
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

using System.ComponentModel;
using Stylet;

namespace MaaWpfGui.Utilities.ValueType
{
    /// <summary>
    /// Generic combined data class.
    /// </summary>
    /// <typeparam name="TValueType">The type of value.</typeparam>
    public class GenericCombinedData<TValueType> : PropertyChangedBase
    {
        private string _name = string.Empty;

        /// <summary>
        /// Gets or sets the name displayed.
        /// </summary>
        public string Display
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        private TValueType _value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public TValueType Value
        {
            get => _value;
            set => SetAndNotify(ref _value, value);
        }
    }
}
