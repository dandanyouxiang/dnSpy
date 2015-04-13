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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using ICSharpCode.ILSpy.TreeNodes;
using dnlib.DotNet;

namespace ICSharpCode.ILSpy.AsmEditor
{
	sealed class DeleteNamespaceCommand : IUndoCommand
	{
		[ExportContextMenuEntry(Category = "AsmEd",
								Order = 240)]//TODO: Update Order
		sealed class ContextMenuentry : TreeNodeContextMenu
		{
			public override bool IsVisible(TextViewContext context)
			{
				return base.IsVisible(context) &&
					DeleteNamespaceCommand.CanExecute(context.SelectedTreeNodes);
			}

			public override void Execute(TextViewContext context)
			{
				DeleteNamespaceCommand.Execute(context.SelectedTreeNodes);
			}

			public override void Initialize(TextViewContext context, MenuItem menuItem)
			{
				SetHeader(context, menuItem, "Delete Namespace", "Delete Namespaces");
			}
		}

		[ExportMainMenuCommand(Menu = "_Edit",
							MenuCategory = "AsmEd",
							MenuOrder = 2100)]//TODO: Set menu order
		sealed class MainMenuEntry : TreeNodeMainMenu, IMainMenuCommandInitialize
		{
			protected override bool CanExecuteInternal(ILSpyTreeNode[] nodes)
			{
				return DeleteNamespaceCommand.CanExecute(nodes);
			}

			protected override void ExecuteInternal(ILSpyTreeNode[] nodes)
			{
				DeleteNamespaceCommand.Execute(nodes);
			}

			public void Initialize(MenuItem menuItem)
			{
				SetHeader(menuItem, "Delete Namespace", "Delete Namespaces");
			}
		}

		static bool CanExecute(ILSpyTreeNode[] nodes)
		{
			return nodes != null &&
				nodes.All(a => a is NamespaceTreeNode);
		}

		static void Execute(ILSpyTreeNode[] nodes)
		{
			if (!CanExecute(nodes))
				return;

			UndoCommandManager.Instance.Add(new DeleteNamespaceCommand(nodes));
		}

		public struct DeleteModelNodes
		{
			ModuleInfo[] infos;

			class ModuleInfo
			{
				public readonly ModuleDef Module;
				public readonly TypeDef[] Types;
				public readonly int[] Indexes;

				public ModuleInfo(ModuleDef module, int count)
				{
					this.Module = module;
					this.Types = new TypeDef[count];
					this.Indexes = new int[count];
				}
			}

			public void Delete(ILSpyTreeNode[] nodes)
			{
				Debug.Assert(infos == null);
				if (infos != null)
					throw new InvalidOperationException();

				infos = new ModuleInfo[nodes.Length];

				for (int i = 0; i < infos.Length; i++) {
					var node = (NamespaceTreeNode)nodes[i];
					var module = ILSpyTreeNode.GetModule(node);
					Debug.Assert(module != null);
					if (module == null)
						throw new InvalidOperationException();

					var info = new ModuleInfo(module, node.Children.Count);
					infos[i] = info;

					for (int j = 0; j < node.Children.Count; j++) {
						var typeNode = (TypeTreeNode)node.Children[j];
						int index = module.Types.IndexOf(typeNode.TypeDefinition);
						Debug.Assert(index >= 0);
						if (index < 0)
							throw new InvalidOperationException();
						module.Types.RemoveAt(index);
						info.Types[j] = typeNode.TypeDefinition;
						info.Indexes[j] = index;
					}
				}
			}

			public void Restore(ILSpyTreeNode[] nodes)
			{
				Debug.Assert(infos != null);
				if (infos == null)
					throw new InvalidOperationException();
				Debug.Assert(infos.Length == nodes.Length);
				if (infos.Length != nodes.Length)
					throw new InvalidOperationException();

				for (int i = infos.Length - 1; i >= 0; i--) {
					var info = infos[i];

					for (int j = info.Types.Length - 1; j >= 0; j--)
						info.Module.Types.Insert(info.Indexes[j], info.Types[j]);
				}

				infos = null;
			}
		}

		DeletableNodes nodes;
		DeleteModelNodes modelNodes;

		public IEnumerable<ILSpyTreeNode> TreeNodes {
			get { return nodes.Nodes; }
		}

		public DeleteNamespaceCommand(ILSpyTreeNode[] nodes)
		{
			if (!CanExecute(nodes))
				throw new ArgumentException();
			this.nodes = new DeletableNodes(nodes);
			this.modelNodes = new DeleteModelNodes();
		}

		public string Description {
			get { return nodes.Count == 1 ? "Delete namespace" : string.Format("Delete {0} namespaces", nodes.Count); }
		}

		public void Execute()
		{
			modelNodes.Delete(nodes.Nodes);
			nodes.Delete();
		}

		public void Undo()
		{
			nodes.Restore();
			modelNodes.Restore(nodes.Nodes);
		}
	}

	sealed class MoveNamespaceTypesToEmptypNamespaceCommand : IUndoCommand
	{
		[ExportContextMenuEntry(Header = "Move Types to Empty Namespace",
								Category = "AsmEd",
								Order = 240)]//TODO: Update Order
		sealed class ContextMenuentry : TreeNodeContextMenu
		{
			public override bool IsVisible(TextViewContext context)
			{
				return base.IsVisible(context) &&
					MoveNamespaceTypesToEmptypNamespaceCommand.CanExecute(context.SelectedTreeNodes);
			}

			public override void Execute(TextViewContext context)
			{
				MoveNamespaceTypesToEmptypNamespaceCommand.Execute(context.SelectedTreeNodes);
			}
		}

		[ExportMainMenuCommand(Header = "Move Types to Empty Namespace",
							   Menu = "_Edit",
							   MenuCategory = "AsmEd",
							   MenuOrder = 2100)]//TODO: Set menu order
		sealed class MainMenuEntry : TreeNodeMainMenu
		{
			protected override bool CanExecuteInternal(ILSpyTreeNode[] nodes)
			{
				return MoveNamespaceTypesToEmptypNamespaceCommand.CanExecute(nodes);
			}

			protected override void ExecuteInternal(ILSpyTreeNode[] nodes)
			{
				MoveNamespaceTypesToEmptypNamespaceCommand.Execute(nodes);
			}
		}

		static bool CanExecute(ILSpyTreeNode[] nodes)
		{
			return nodes != null &&
				nodes.All(a => a is NamespaceTreeNode) &&
				nodes.Any(a => ((NamespaceTreeNode)a).Name != string.Empty) &&
				nodes.IsInSameModule() &&
				nodes[0].Parent.Children.Any(a => a is NamespaceTreeNode && ((NamespaceTreeNode)a).Name == string.Empty);
		}

		static void Execute(ILSpyTreeNode[] nodes)
		{
			if (!CanExecute(nodes))
				return;

			UndoCommandManager.Instance.Add(new MoveNamespaceTypesToEmptypNamespaceCommand(nodes));
		}

		public MoveNamespaceTypesToEmptypNamespaceCommand(ILSpyTreeNode[] nodes)
		{
			if (!CanExecute(nodes))
				throw new ArgumentException();
			nodes = nodes.Where(a => ((NamespaceTreeNode)a).Name != string.Empty).ToArray();
			Debug.Assert(nodes.Length > 0);
			this.nodes = new DeletableNodes(nodes);
			this.nsTarget = GetTarget();
		}

		public string Description {
			get { return "Move Types to Empty Namespace"; }
		}

		readonly NamespaceTreeNode nsTarget;
		DeletableNodes nodes;
		ModelInfo[] infos;

		public IEnumerable<ILSpyTreeNode> TreeNodes {
			get {
				yield return nsTarget;
				foreach (var n in nodes.Nodes)
					yield return n;
			}
		}

		class ModelInfo
		{
			public UTF8String[] Namespaces;
			public DeletableNodes TypeNodes;
		}

		NamespaceTreeNode GetTarget()
		{
			return nodes.Nodes.Length == 0 ? null : (NamespaceTreeNode)nodes.Nodes[0].Parent.Children.First(a => a is NamespaceTreeNode && ((NamespaceTreeNode)a).Name == string.Empty);
		}

		public void Execute()
		{
			Debug.Assert(infos == null);
			if (infos != null)
				throw new InvalidOperationException();

			nodes.Delete();

			infos = new ModelInfo[nodes.Count];
			for (int i = 0; i < infos.Length; i++) {
				var nsNode = (NamespaceTreeNode)nodes.Nodes[i];

				var info = new ModelInfo();
				infos[i] = info;
				info.Namespaces = new UTF8String[nsNode.Children.Count];
				info.TypeNodes = new DeletableNodes(nsNode.Children.Cast<TypeTreeNode>());
				info.TypeNodes.Delete();

				for (int j = 0; j < info.Namespaces.Length; j++) {
					var typeNode = (TypeTreeNode)info.TypeNodes.Nodes[j];
					info.Namespaces[j] = typeNode.TypeDefinition.Namespace;
					typeNode.TypeDefinition.Namespace = UTF8String.Empty;
					nsTarget.Append(typeNode);
				}
			}
		}

		public void Undo()
		{
			Debug.Assert(infos != null);
			if (infos == null)
				throw new InvalidOperationException();

			for (int i = infos.Length - 1; i >= 0; i--) {
				var info = infos[i];

				for (int j = info.Namespaces.Length - 1; j >= 0; j--) {
					var typeNode = nsTarget.RemoveLast();
					bool b = info.TypeNodes.Nodes[j] == typeNode;
					Debug.Assert(b);
					if (!b)
						throw new InvalidOperationException();
					typeNode.TypeDefinition.Namespace = info.Namespaces[j];
				}

				info.TypeNodes.Restore();
			}

			nodes.Restore();

			infos = null;
		}
	}
}