﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using dnSpy.Contracts.Settings.Dialog;

namespace dnSpy.Contracts.Decompiler {
	/// <summary>
	/// Decompiler settings shown in the Decompiler tab
	/// </summary>
	public interface IDecompilerSettingsTab {
		/// <summary>
		/// Gets the order, eg. <see cref="DecompilerConstants.ORDER_DECOMPILER_SETTINGS_ILSPY_CSHARP"/>
		/// </summary>
		double Order { get; }

		/// <summary>
		/// Gets the name shown in the combobox, eg. "C# / VB" or "IL"
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the UI object
		/// </summary>
		object UIObject { get; }

		/// <summary>
		/// Called when the dialog box has been closed
		/// </summary>
		/// <param name="saveSettings">true to save the settings, false to cancel any changes</param>
		/// <param name="appRefreshSettings">Used if <paramref name="saveSettings"/> is true. Add
		/// anything that needs to be refreshed, eg. re-decompile code</param>
		void OnClosed(bool saveSettings, IAppRefreshSettings appRefreshSettings);
	}
}
