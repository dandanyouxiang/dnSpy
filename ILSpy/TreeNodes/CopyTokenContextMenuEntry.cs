﻿/*
    Copyright (C) 2014-2015 de4dot@gmail.com

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

using System.Windows;
using dnlib.DotNet;

namespace ICSharpCode.ILSpy.TreeNodes {
	abstract class CopyTokenContextMenuEntryBase : IContextMenuEntry
	{
		public abstract bool IsVisible(TextViewContext context);

		protected IMDTokenProvider GetReference(TextViewContext context)
		{
			if (context.Reference != null)
				return context.Reference.Reference as IMDTokenProvider;
			if (context.SelectedTreeNodes != null && context.SelectedTreeNodes.Length != 0 &&
					context.SelectedTreeNodes[0] is ITokenTreeNode) {
				return ((ITokenTreeNode)context.SelectedTreeNodes[0]).MDTokenProvider;
			}

			return null;
		}

		public bool IsEnabled(TextViewContext context)
		{
			return true;
		}

		public abstract void Execute(TextViewContext context);

		protected void Execute(IMDTokenProvider member)
		{
			if (member == null)
				return;

			Clipboard.SetText(string.Format("0x{0:X8}", member.MDToken.Raw));
		}
	}

	[ExportContextMenuEntryAttribute(Header = "_Copy Token", Order = 310, Category = "Tokens")]
	class CopyTokenContextMenuEntry : CopyTokenContextMenuEntryBase
	{
		public override bool IsVisible(TextViewContext context)
		{
			return GetReference(context) != null;
		}

		public override void Execute(TextViewContext context)
		{
			var obj = GetReference(context);
			if (obj != null)
				Execute(obj);
		}
	}

	[ExportContextMenuEntryAttribute(Header = "Copy _Definition Token", Order = 320, Category = "Tokens")]
	class CopyDefinitionTokenContextMenuEntry : CopyTokenContextMenuEntryBase
	{
		public override bool IsVisible(TextViewContext context)
		{
			var obj = GetReference(context);
			return obj is IMemberRef && !(obj is IMemberDef);
		}

		public override void Execute(TextViewContext context)
		{
			var obj = GetReference(context);
			if (obj != null) {
				var member = MainWindow.ResolveReference(obj);
				if (member == null)
					MainWindow.Instance.ShowMessageBox("Could not resolve member definition");
				else
					Execute(member);
			}
		}
	}
}
