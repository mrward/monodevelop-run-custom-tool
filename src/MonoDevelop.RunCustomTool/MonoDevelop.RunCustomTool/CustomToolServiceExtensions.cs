//
// CustomToolServiceExtensions.cs
//
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2010 Novell, Inc.
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
using System.Reflection;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Ide.CustomTools;
using MonoDevelop.Projects;

namespace MonoDevelop.RunCustomTool
{
	static class CustomToolServiceExtensions
	{
		static readonly MethodInfo updateMethod;

		static CustomToolServiceExtensions ()
		{
			RunCustomToolOnBuildEnabled = ConfigurationProperty.Create<bool> ("RunCustomTool.RunCustomToolOnBuildEnabled", true);
			updateMethod = FindUpdateMethod ();
		}

		public static ConfigurationProperty<bool> RunCustomToolOnBuildEnabled { get; private set; }

		public static IEnumerable<ProjectFile> GetFilesToProcess (Solution solution)
		{
			return solution.GetAllProjects ().SelectMany (project => GetFilesToProcess (project));
		}

		public static IEnumerable<ProjectFile> GetFilesToProcess (Project project)
		{
			return project.Files.Where (file => ShouldRunCustomTool (file));
		}

		public static async Task Update (IEnumerable<ProjectFile> files, bool force = true)
		{
			if (updateMethod == null) {
				await CustomToolService.Update (files, force);
				return;
			}

			IEnumerator<ProjectFile> fileEnumerator;

			var progressMonitor = IdeApp.Workbench.ProgressMonitors.GetToolOutputProgressMonitor (false, null); 
			if (files == null || !(fileEnumerator = files.GetEnumerator ()).MoveNext ()) {
				progressMonitor.ReportSuccess (GettextCatalog.GetString ("No custom tools found"));
				progressMonitor.Dispose ();
			} else {
				progressMonitor.BeginTask (GettextCatalog.GetString ("Running custom tools"), 1);
				await Update (progressMonitor, fileEnumerator, force);
			}
		}

		static MethodInfo FindUpdateMethod ()
		{
			var type = typeof (CustomToolService);
			var flags = BindingFlags.NonPublic | BindingFlags.Static;
			var parameterTypes = new Type [] {
				typeof (ProgressMonitor),
				typeof (IEnumerator<ProjectFile>),
				typeof (bool),
				typeof (int),
				typeof (int),
				typeof (int)
			};
			return type.GetMethod ("Update", flags, null, parameterTypes, null);
		}

		static Task Update (
			OutputProgressMonitor progressMonitor,
			IEnumerator<ProjectFile> files,
			bool force)
		{
			var parameters = new object [] {
				progressMonitor,
				files,
				force,
				0,
				0,
				0
			};
			var result = updateMethod.Invoke (null, parameters) as Task;
			if (result != null) {
				return result;
			}
			return Task.CompletedTask;
		}

		public static bool ShouldRunCustomTool (ProjectFile file)
		{
			if (file == null) {
				return false;
			}

			if ((file.Flags & ProjectItemFlags.DontPersist) != 0) {
				return false;
			}

			return !string.IsNullOrEmpty (file.Generator);
		}

		public static async Task RunCustomToolsBeforeBuild (Project project)
		{
			if (!RunCustomToolOnBuildEnabled) {
				return;
			}

			var files = GetFilesToProcess (project);
			if (!files.Any ()) {
				return;
			}

			await Update (files, false);
		}

		public static async Task RunCustomToolsBeforeBuild (Solution solution)
		{
			if (!RunCustomToolOnBuildEnabled) {
				return;
			}

			var files = GetFilesToProcess (solution);
			if (!files.Any ()) {
				return;
			}

			await Update (files, false);
		}
	}
}
