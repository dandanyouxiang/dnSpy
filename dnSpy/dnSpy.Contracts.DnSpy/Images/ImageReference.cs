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

using System;
using System.Reflection;

namespace dnSpy.Contracts.Images {
	/// <summary>
	/// Image reference
	/// </summary>
	public struct ImageReference {
		/// <summary>
		/// true if it's the default instance
		/// </summary>
		public bool IsDefault => Assembly == null;

		/// <summary>
		/// Assembly of image
		/// </summary>
		public Assembly Assembly { get; }

		/// <summary>
		/// Name of image
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="assembly">Assembly of image</param>
		/// <param name="name">Name of image</param>
		public ImageReference(Assembly assembly, string name) {
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			this.Assembly = assembly;
			this.Name = name;
		}
	}
}
