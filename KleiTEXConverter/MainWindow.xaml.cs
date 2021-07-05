using KleiLib;
using Microsoft.WindowsAPICodePack.Dialogs;
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

			// TODO: Remove this when releasing!
			m_inputPath = "E:\\Development\\C#\\KleiTEXConverter\\warly";
			InPathBox.Text = m_inputPath;
			m_outputPath = "E:\\Development\\C#\\KleiTEXConverter\\warlyout";
			OutPathBox.Text = m_outputPath;

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
			string[] filePaths = Directory.GetFiles(m_inputPath);

			TEXConverter ??= new TEXConverter();

			LogBox.Text = "";

			foreach (string file in filePaths)
			{
				LogBox.Text += string.Format("Converting {0}...\n", file);
				LogBox.ScrollToEnd();

				string outPath = await Task.Run(() => ConvertFile(file));

				LogBox.Text += string.Format("Saved to {0}!\n", outPath);
				LogBox.ScrollToEnd();
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
