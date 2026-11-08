using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xml_lab2.Cloud;
using Xml_lab2.Logging;
using Xml_lab2.Models;
using Xml_lab2.Services;
using Xml_lab2.Strategies;

namespace Xml_lab2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _xmlFilePath;
        private string _xslFilePath;
        private ObservableCollection<SportRecord> _sportsRecords = new ObservableCollection<SportRecord>();
        private IXmlAnalyzerStrategy _analyzerStrategy;
        private Dictionary<string, List<string>> _valuesByAttribute = new Dictionary<string, List<string>>();

        private LocalFileManager _localFileManager = new LocalFileManager();
        private HtmlTransformer _transformer = new HtmlTransformer();
        private Logger _logger = Logger.GetInstance();

        private string _selectedAnalyzer = "SAX";
        private string _selectedAttribute;
        private string _selectedAttributeValue;
        private string _searchKeyword = "";
        private string _statusText = "Відкрийте XML-файл";

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SportRecord> SportsRecords
        {
            get { return _sportsRecords; }
        }

        public List<string> Analyzers { get; } = new List<string> { "SAX", "DOM", "LINQ to XML" };

        private List<string> _attributeNames = new List<string>();
        private List<string> _attributeValues = new List<string>();

        public List<string> AttributeNames
        {
            get { return _attributeNames; }
            set
            {
                _attributeNames = value;
                OnPropertyChanged();
            }
        }

        public List<string> AttributeValues
        {
            get { return _attributeValues; }
            set
            {
                _attributeValues = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAnalyzer
        {
            get { return _selectedAnalyzer; }
            set
            {
                _selectedAnalyzer = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAttribute
        {
            get { return _selectedAttribute; }
            set
            {
                _selectedAttribute = value;
                OnPropertyChanged();
                FillAttributeValues();
            }
        }

        public string SelectedAttributeValue
        {
            get { return _selectedAttributeValue; }
            set
            {
                _selectedAttributeValue = value;
                OnPropertyChanged();
            }
        }

        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public string FilesInfo
        {
            get
            {
                string xmlName = "—";
                string xslName = "—";
                if (_xmlFilePath != null)
                {
                    xmlName = Path.GetFileName(_xmlFilePath);
                }
                if (_xslFilePath != null)
                {
                    xslName = Path.GetFileName(_xslFilePath);
                }
                return "XML: " + xmlName + "    XSL: " + xslName;
            }
        }

        public bool HasXml
        {
            get { return _xmlFilePath != null; }
        }

        public MainViewModel()
        {
        }

        public void LoadXml(string path)
        {
            XDocument doc = XDocument.Load(path);
            _xmlFilePath = path;

            _valuesByAttribute = new Dictionary<string, List<string>>();

            foreach (XElement section in doc.Descendants("Section"))
            {
                XAttribute nameAttr = section.Attribute("Name");
                if (nameAttr != null)
                {
                    AddAttributeValue("Section", nameAttr.Value);
                }
            }

            foreach (XElement member in doc.Descendants("Member"))
            {
                foreach (XAttribute attr in member.Attributes())
                {
                    if (attr.Name.LocalName != "Name")
                    {
                        AddAttributeValue(attr.Name.LocalName, attr.Value);
                    }
                }
            }

            List<string> names = new List<string>();
            foreach (string xmlName in _valuesByAttribute.Keys)
            {
                _valuesByAttribute[xmlName].Sort();
                names.Add(AttributeTitles.GetTitle(xmlName));
            }
            AttributeNames = names;

            ClearData();
            OnPropertyChanged(nameof(FilesInfo));
            StatusText = "Завантажено файл " + Path.GetFileName(path);
        }

        private void AddAttributeValue(string xmlName, string value)
        {
            if (!_valuesByAttribute.ContainsKey(xmlName))
            {
                _valuesByAttribute[xmlName] = new List<string>();
            }

            if (!_valuesByAttribute[xmlName].Contains(value))
            {
                _valuesByAttribute[xmlName].Add(value);
            }
        }

        private void FillAttributeValues()
        {
            SelectedAttributeValue = null;

            List<string> values = new List<string>();

            if (!string.IsNullOrEmpty(_selectedAttribute))
            {
                string xmlName = AttributeTitles.GetXmlName(_selectedAttribute);
                if (_valuesByAttribute.ContainsKey(xmlName))
                {
                    values.AddRange(_valuesByAttribute[xmlName]);
                }
            }

            AttributeValues = values;
        }

        public void LoadXsl(string path)
        {
            XDocument.Load(path);
            _xslFilePath = path;
            OnPropertyChanged(nameof(FilesInfo));
            StatusText = "Завантажено XSL " + Path.GetFileName(path);
        }

        public void SaveXml(string path)
        {
            List<SportRecord> data = new List<SportRecord>(_sportsRecords);
            if (data.Count == 0)
            {
                throw new InvalidOperationException("Немає відфільтрованих даних для збереження. Спочатку виконайте пошук.");
            }

            XmlBuilder builder = new XmlBuilder();
            string content = builder.BuildContent(data);
            _localFileManager.SaveToFile(content, path);

            _logger.LogEvent(LogLevel.Saving, "Збережено відфільтрований фрагмент у файл " + Path.GetFileName(path));
            StatusText = "Збережено: " + path;
        }

        public string GetDefaultSavePath(string fileName, string extension)
        {
            string folder = Path.GetDirectoryName(_xmlFilePath);
            return Path.Combine(folder, Path.GetFileNameWithoutExtension(fileName) + extension);
        }

        public void ExecuteSearch()
        {
            if (_xmlFilePath == null)
            {
                throw new InvalidOperationException("Спочатку відкрийте XML-файл.");
            }

            string attributeName = "";
            if (!string.IsNullOrEmpty(SelectedAttribute))
            {
                if (string.IsNullOrEmpty(SelectedAttributeValue))
                {
                    throw new InvalidOperationException("Ви вибрали атрибут «" + SelectedAttribute + "», але не вибрали його значення. Виберіть значення у списку «Значення».");
                }
                attributeName = AttributeTitles.GetXmlName(SelectedAttribute);
            }

            _analyzerStrategy = CreateStrategy(SelectedAnalyzer);
            List<SportRecord> results = _analyzerStrategy.Analyze(_xmlFilePath, attributeName, SelectedAttributeValue, SearchKeyword);

            UpdateView(results);

            _logger.LogEvent(LogLevel.Filtering, "Параметри: " + DescribeSearch() + ". Знайдено записів: " + results.Count);
        }

        private string DescribeSearch()
        {
            string text = "«Аналізатор» – " + SelectedAnalyzer;

            if (!string.IsNullOrEmpty(SelectedAttribute) && !string.IsNullOrEmpty(SelectedAttributeValue))
            {
                text = text + ", «" + SelectedAttribute + "» – " + SelectedAttributeValue;
            }

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                text = text + ", «Ключове слово» – " + SearchKeyword;
            }

            return text;
        }

        private IXmlAnalyzerStrategy CreateStrategy(string name)
        {
            if (name == "DOM")
            {
                return new DomAnalyzer();
            }
            if (name == "LINQ to XML")
            {
                return new LinqAnalyzer();
            }
            return new SaxAnalyzer();
        }

        private void UpdateView(List<SportRecord> results)
        {
            _sportsRecords.Clear();
            foreach (SportRecord record in results)
            {
                _sportsRecords.Add(record);
            }
            StatusText = "Знайдено записів: " + results.Count + " (" + SelectedAnalyzer + ")";
        }

        public string TransformToHtml()
        {
            if (_xmlFilePath == null || _xslFilePath == null)
            {
                throw new InvalidOperationException("Спочатку відкрийте XML і XSL файли.");
            }

            string outHtmlPath = Path.ChangeExtension(_xmlFilePath, ".html");
            bool success = _transformer.Transform(_xmlFilePath, _xslFilePath, outHtmlPath);
            if (!success)
            {
                throw new Exception("Помилка трансформації: " + _transformer.LastError);
            }

            _logger.LogEvent(LogLevel.Transformation, "Збережено у файл " + Path.GetFileName(outHtmlPath));
            StatusText = "Трансформація успішна: " + outHtmlPath;
            return outHtmlPath;
        }

        public void ClearData()
        {
            _sportsRecords.Clear();
            SelectedAttribute = null;
            SearchKeyword = "";
            StatusText = "Результати і параметри пошуку очищено";
        }

        public void ExitApplication()
        {
            if (Application.Current != null)
            {
                Application.Current.Quit();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
