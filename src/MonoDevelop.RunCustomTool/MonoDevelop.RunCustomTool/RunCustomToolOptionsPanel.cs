//
// RunCustomToolOptionsPanel.cs
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

using MonoDevelop.Components;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Dialogs;

namespace MonoDevelop.RunCustomTool
{
	class RunCustomToolOptionsPanel : OptionsPanel
	{
		Gtk.CheckButton runCustomToolOnBuildCheckButton;

		public override Control CreatePanelWidget ()
		{
			var vbox = new Gtk.VBox ();
			vbox.Spacing = 6;

			var restoreSectionLabel = new Gtk.Label (GetBoldMarkup (GettextCatalog.GetString ("Run Custom Tool")));
			restoreSectionLabel.UseMarkup = true;
			restoreSectionLabel.Xalign = 0;
			vbox.PackStart (restoreSectionLabel, false, false, 0);

			runCustomToolOnBuildCheckButton = new Gtk.CheckButton (GettextCatalog.GetString ("Run custom tools on build"));
			runCustomToolOnBuildCheckButton.TooltipText = GettextCatalog.GetString ("Custom tools will be run when the project or solution is built.");
			runCustomToolOnBuildCheckButton.Active = CustomToolServiceExtensions.RunCustomToolOnBuildEnabled.Value;
			runCustomToolOnBuildCheckButton.BorderWidth = 10;
			vbox.PackStart (runCustomToolOnBuildCheckButton, false, false, 0);

			vbox.ShowAll ();

			return vbox;
		}

		public override void ApplyChanges ()
		{
			CustomToolServiceExtensions.RunCustomToolOnBuildEnabled.Value = runCustomToolOnBuildCheckButton.Active;
		}

		static string GetBoldMarkup (string text)
		{
			return "<b>" + text + "</b>";
		}
	}
}
