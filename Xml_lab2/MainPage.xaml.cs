using Xml_lab2.ViewModels;

namespace Xml_lab2
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel _viewModel = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        private async Task<string> PickFileAsync(string title, string[] extensions)
        {
            Dictionary<DevicePlatform, IEnumerable<string>> types = new Dictionary<DevicePlatform, IEnumerable<string>>();
            types[DevicePlatform.WinUI] = extensions;

            PickOptions options = new PickOptions();
            options.PickerTitle = title;
            options.FileTypes = new FilePickerFileType(types);

            FileResult result = await FilePicker.Default.PickAsync(options);
            if (result == null)
            {
                return null;
            }
            return result.FullPath;
        }

        private async void OpenXml_Clicked(object sender, EventArgs e)
        {
            try
            {
                string path = await PickFileAsync("Оберіть XML-файл", new string[] { ".xml" });
                if (path != null)
                {
                    _viewModel.LoadXml(path);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private async void OpenXsl_Clicked(object sender, EventArgs e)
        {
            try
            {
                string path = await PickFileAsync("Оберіть XSL-файл", new string[] { ".xsl", ".xslt" });
                if (path != null)
                {
                    _viewModel.LoadXsl(path);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private async void Search_Clicked(object sender, EventArgs e)
        {
            try
            {
                _viewModel.ExecuteSearch();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private void Clear_Clicked(object sender, EventArgs e)
        {
            _viewModel.ClearData();
        }

        private async void Transform_Clicked(object sender, EventArgs e)
        {
            try
            {
                string htmlPath = _viewModel.TransformToHtml();
                bool open = await DisplayAlert("Трансформація успішна", "Файл збережено: " + htmlPath + "\nВідкрити його?", "Так", "Ні");
                if (open)
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest("HTML", new ReadOnlyFile(htmlPath)));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private async void SaveXml_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!_viewModel.HasXml)
                {
                    await DisplayAlert("Помилка", "Спочатку відкрийте XML-файл.", "OK");
                    return;
                }

                string name = await DisplayPromptAsync("Збереження", "Назва файлу (без розширення):", "OK", "Скасувати", null, -1, null, "filtered");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    string path = _viewModel.GetDefaultSavePath(name, ".xml");
                    if (File.Exists(path) && !_viewModel.IsCurrentXml(path))
                    {
                        bool overwrite = await DisplayAlert("Збереження", "Файл " + Path.GetFileName(path) + " вже існує. Замінити його?", "Так", "Ні");
                        if (!overwrite)
                        {
                            return;
                        }
                    }
                    _viewModel.SaveXml(path);
                    await DisplayAlert("Збереження", "Файл збережено: " + path, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private async void OpenFromDrive_Clicked(object sender, EventArgs e)
        {
            try
            {
                string type = await DisplayActionSheet("Що відкрити з Google Drive?", "Скасувати", null, "XML-файл", "XSL-файл");
                if (type != "XML-файл" && type != "XSL-файл")
                {
                    return;
                }

                bool isXsl = type == "XSL-файл";
                string extension = ".xml";
                if (isXsl)
                {
                    extension = ".xsl";
                }

                Dictionary<string, string> files = await _viewModel.GetDriveFilesAsync(extension);
                if (files.Count == 0)
                {
                    await DisplayAlert("Google Drive", "На Google Drive немає файлів " + extension, "OK");
                    return;
                }

                string[] names = new string[files.Count];
                files.Keys.CopyTo(names, 0);

                string chosen = await DisplayActionSheet("Оберіть файл", "Скасувати", null, names);
                if (chosen == null || !files.ContainsKey(chosen))
                {
                    return;
                }

                await _viewModel.OpenFromDriveAsync(files[chosen], chosen, isXsl);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        private async void SaveToDrive_Clicked(object sender, EventArgs e)
        {
            try
            {
                string filteredXml = "Відфільтровані дані (XML)";
                string filteredHtml = "Відфільтровані дані (HTML)";
                string sourceXml = "Поточний XML-файл";
                string sourceXsl = "Поточний XSL-файл";

                string choice = await DisplayActionSheet("Що зберегти на Google Drive?", "Скасувати", null, filteredXml, filteredHtml, sourceXml, sourceXsl);

                string savedName = null;
                if (choice == filteredXml || choice == filteredHtml)
                {
                    string format = "XML";
                    if (choice == filteredHtml)
                    {
                        format = "HTML";
                    }

                    string name = await DisplayPromptAsync("Google Drive", "Назва файлу (без розширення):", "OK", "Скасувати", null, -1, null, "filtered");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        return;
                    }
                    savedName = await _viewModel.SaveFilteredToDriveAsync(format, name);
                }
                else if (choice == sourceXml)
                {
                    savedName = await _viewModel.UploadSourceFileAsync(false);
                }
                else if (choice == sourceXsl)
                {
                    savedName = await _viewModel.UploadSourceFileAsync(true);
                }

                if (savedName != null)
                {
                    await DisplayAlert("Google Drive", "Файл " + savedName + " збережено на Google Drive.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        public bool IsExiting { get; private set; }

        public async Task AskExitAsync()
        {
            bool answer = await DisplayAlert("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
            if (answer)
            {
                IsExiting = true;
                _viewModel.ExitApplication();
            }
        }

        private async void Exit_Clicked(object sender, EventArgs e)
        {
            await AskExitAsync();
        }
    }
}
