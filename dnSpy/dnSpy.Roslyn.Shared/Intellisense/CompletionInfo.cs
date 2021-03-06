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
using dnSpy.Roslyn.Shared.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace dnSpy.Roslyn.Shared.Intellisense {
	struct CompletionInfo {
		public CompletionService CompletionService { get; }
		public Document Document { get; }
		public SourceText SourceText { get; }
		public ITextSnapshot Snapshot { get; }

		CompletionInfo(CompletionService completionService, Document document, SourceText sourceText, ITextSnapshot snapshot) {
			CompletionService = completionService;
			Document = document;
			SourceText = sourceText;
			Snapshot = snapshot;
		}

		public static CompletionInfo? Create(ITextSnapshot snapshot) {
			if (snapshot == null)
				throw new ArgumentNullException(nameof(snapshot));
			var sourceText = snapshot.AsText();
			var document = sourceText.GetOpenDocumentInCurrentContextWithChanges();
			if (document == null)
				return null;
			var completionService = CompletionService.GetService(document);
			if (completionService == null)
				return null;
			return new CompletionInfo(completionService, document, sourceText, snapshot);
		}
	}
}
