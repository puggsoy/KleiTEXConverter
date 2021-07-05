/*
Copyright (C) 2021  puggsoy

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace KleiTEXConverter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string m_inputPath = null;
		private string m_outputPath = null;

		private TEXConverter TEXConverter = null;

		public MainWindow()
		{
			InitializeComponent();

			CheckCanConvert();
		}

		private void SelectInputFolder()
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog()
			{
				Title = "Select Input Folder",
				IsFolderPicker = true,
				EnsurePathExists = true
			};

			CommonFileDialogResult result = dialog.ShowDialog(this);
			
			if (result == CommonFileDialogResult.Ok)
			{
				string dir = dialog.FileName;

				if (!Directory.Exists(dir))
					MessageBox.Show(string.Format("Folder {0} doesn't exist!", dir), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

				m_inputPath = dir;

				InPathBox.Text = m_inputPath;

				CheckCanConvert();
			}
		}

		private void SelectOutputFolder()
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog()
			{
				Title = "Select Output Folder",
				IsFolderPicker = true,
				EnsurePathExists = true
			};

			CommonFileDialogResult result = dialog.ShowDialog(this);

			if (result == CommonFileDialogResult.Ok)
			{
				string dir = dialog.FileName;

				if (!Directory.Exists(dir))
					MessageBox.Show(string.Format("Folder {0} doesn't exist!", dir), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

				m_outputPath = dir;

				OutPathBox.Text = m_outputPath;

				CheckCanConvert();
			}
		}

		private void CheckCanConvert()
		{
			bool enabled = !string.IsNullOrEmpty(m_inputPath) && !string.IsNullOrEmpty(m_outputPath);
			ConvertButton.IsEnabled = enabled;
		}

		private async void ConvertTEXFiles()
		{
			string[] filePaths = Directory.GetFiles(m_inputPath, "*.tex");

			TEXConverter ??= new TEXConverter();

			LogBox.Text = "";

			foreach (string file in filePaths)
			{
				LogBox.Text += string.Format("Converting {0}...\n", file);
				LogBox.ScrollToEnd();

				try
				{
					string outPath = await Task.Run(() => ConvertFile(file));

					LogBox.Text += string.Format("Saved to {0}!\n", outPath);
					LogBox.ScrollToEnd();
				}
				catch (Exception e)
				{
					LogBox.Text += string.Format("Failed to convert! Error: {0}\n", e.ToString());
					LogBox.ScrollToEnd();
				}
			}

			LogBox.Text += "\nAll done!";
			LogBox.ScrollToEnd();
		}

		private string ConvertFile(string inPath)
		{
			FileStream stream = new FileStream(inPath, FileMode.Open);

			Bitmap bmp = TEXConverter.ConvertToBitmap(stream);

			string outPath = string.Format("{0}.png", Path.Combine(m_outputPath, Path.GetFileNameWithoutExtension(inPath)));

			bmp.Save(outPath, ImageFormat.Png);

			return outPath;
		}

		#region Control Callbacks
		private void OnSelectInputClick(object sender, RoutedEventArgs e)
		{
			SelectInputFolder();
		}

		private void OnSelectOutputClick(object sender, RoutedEventArgs e)
		{
			SelectOutputFolder();
		}
		private void OnConvertClick(object sender, RoutedEventArgs e)
		{
			ConvertTEXFiles();
		}
		#endregion
	}
}
