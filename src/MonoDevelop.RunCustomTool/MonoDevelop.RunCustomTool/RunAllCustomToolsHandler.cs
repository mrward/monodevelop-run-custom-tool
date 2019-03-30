//
// RunAllCustomToolsCommandHandler.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace MonoDevelop.RunCustomTool
{
	class RunAllCustomToolsHandler : CommandHandler
	{
		protected override void Run ()
		{
			var files = GetFilesToProcess ().ToList ();
			CustomToolServiceExtensions.Update (files, true).Ignore ();
		}

		static IEnumerable<ProjectFile> GetFilesToProcess ()
		{
			var item = IdeApp.ProjectOperations.CurrentSelectedItem;
			if (item is Solution solution) {
				return GetFilesToProcess (solution);
			} else if (item is Project project) {
				return GetFilesToProcess (project);
			}
			return Enumerable.Empty <ProjectFile> ();
		}

		static IEnumerable<ProjectFile> GetFilesToProcess (Project project)
		{
			return project.Files.Where (file => ShouldRunCustomTool (file));
		}

		static IEnumerable<ProjectFile> GetFilesToProcess (Solution solution)
		{
			return solution.GetAllProjects ().SelectMany (project => GetFilesToProcess (project));
		}

		static bool ShouldRunCustomTool (ProjectFile file)
		{
			if ((file.Flags & ProjectItemFlags.DontPersist) != 0) {
				return false;
			}

			return !string.IsNullOrEmpty (file.Generator);
		}
	}
}
