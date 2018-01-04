namespace RoslynSyntaxSearch.Code
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;

	/// <summary>
	/// This class implements the tool window exposed by this package and hosts a user control.
	/// </summary>
	/// <remarks>
	/// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
	/// usually implemented by the package implementer.
	/// <para>
	/// This class derives from the ToolWindowPane class provided from the MPF in order to use its
	/// implementation of the IVsUIElementPane interface.
	/// </para>
	/// </remarks>
	[Guid("9f58bbdd-d352-45a0-be86-b1ad1499f9a7")]
	public class SyntaxSearchWindow : ToolWindowPane
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxSearchWindow"/> class.
		/// </summary>
		public SyntaxSearchWindow() : base(null)
		{
			this.Caption = "Syntax Search";

			// This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
			// we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
			// the object returned by the Content property.
			var control = new SyntaxSearchWindowControl();
			control.DataContext = new SyntaxSearchWindowViewModel();
			this.Content = control;
		}
	}
}
