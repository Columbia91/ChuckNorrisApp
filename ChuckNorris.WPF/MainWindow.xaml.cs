using ChuckNorris.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChuckNorris.WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Result joke;
        private List<string> categories;

        CancellationTokenSource cancelTokenSource;
        CancellationToken cancellationToken;
        
        public MainWindow()
        {
            InitializeComponent();
            
            CompleteCategories();
            ShowRandomJokes();
        }

        #region Вывести категории шуток
        private void CompleteCategories()
        {
            Task.Factory.StartNew(() =>
            {
                string response = GetResponse("https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/categories");
                categories = JsonConvert.DeserializeObject<List<string>>(response);
                Dispatcher.Invoke((Action)delegate
                {
                    categoriesListBox.ItemsSource = categories;
                });
            });
        }
        #endregion

        #region Вывести рандомные шутки
        private void ShowRandomJokes()
        {
            cancelTokenSource = new CancellationTokenSource();
            cancellationToken = cancelTokenSource.Token;
            
            var cancellationTask = Task.Factory.StartNew(() =>
            {
                string response;
                int jokesCount = 50;

                for (int i = 0; i < jokesCount; i++)
                {
                    response = GetResponse("https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/random");
                    if (response.Length > 0)
                    {
                        joke = JsonConvert.DeserializeObject<Result>(response);
                        Dispatcher.Invoke((Action)delegate
                        {
                            richTextBox.AppendText($"{i + 1}) {joke.Value}\n");
                        });
                    }
                    else
                        break;
                }
            }, cancellationToken);
        }
        #endregion

        #region Получить ответ к запросу
        private string GetResponse(string url)
        {
            string response = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Headers["X-RapidAPI-Key"] = "a0ffdc746bmsh1d3fccdb8bdebd4p105957jsncf7f0f18ddad";
                httpWebRequest.Headers["X-RapidAPI-Host"] = "matchilling-chuck-norris-jokes-v1.p.rapidapi.com";
                httpWebRequest.Method = "GET";
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                
                using (StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                }
            }
            catch (WebException exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return response;
        }
        #endregion

        #region Изменение текста текстбокса при клике мышью
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == "Search request...")
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = string.Empty;
            }
        }
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == "")
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = "Search request...";
            }
        }
        #endregion

        #region Двойной клик или нажатие Enter по категории
        private void CategoriesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            cancelTokenSource.Cancel();

            var category = categories[categoriesListBox.SelectedIndex];
            string url = "https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/random?category=" + category;

            richTextBox.Document.Blocks.Clear();

            Task.Factory.StartNew(() =>
            {
                string response;
                int jokesCount = 50;
                
                for (int i = 0; i < jokesCount; i++)
                {
                    response = GetResponse(url);
                    if (response.Length > 0)
                    {
                        joke = JsonConvert.DeserializeObject<Result>(response);
                        Dispatcher.Invoke((Action)delegate
                        {
                            richTextBox.AppendText($"{i + 1}) {joke.Value}\n");
                        });
                    }
                }
            });
        }
        private void CategoriesListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CategoriesListBox_MouseDoubleClick(this, null);
        }
        #endregion

        #region Нажатие Enter после ввода текстового запроса
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && searchTextBox.Text.Length > 0)
            {
                string url = "https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/search?query=" + searchTextBox.Text;

                Task.Factory.StartNew(() =>
                {
                    string response = GetResponse(url);
                    var query = JsonConvert.DeserializeObject<Query>(response);

                    string jokes = "";
                    int counter = 0;
                    foreach (var joke in query.Result)
                    {
                        jokes += $"{++counter}) " + joke.Value + "\n";
                    }

                    Dispatcher.Invoke((Action)delegate
                    {
                        if (jokes.Length > 0)
                        {
                            richTextBox.Document.Blocks.Clear();
                            richTextBox.AppendText(jokes);
                        }
                        else
                        {
                            MessageBox.Show("No matches found for current search", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            searchTextBox.Text = "";
                            return;
                        }
                    });
                });
            }
        }
        #endregion
    }
}
