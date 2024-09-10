using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace NewsAPI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            //string jsonFile = System.IO.File.ReadAllText(PATH);
            //List<Article> news = JsonConvert.DeserializeObject<List<Article>>(jsonFile);
            //newsList.ItemsSource = news;
        }

        public static readonly HttpClient httpClient = new HttpClient();
        private const string DefaultUrl = "https://newsapi.org/v2/everything?apiKey=b8cc28acb3634fcc937daae3b28872a4&language=ru";
        private static string Url = DefaultUrl;
         
        private static string UrlSortedByCategory = $"https://newsapi.org/v2/everything?apiKey=b8cc28acb3634fcc937daae3b28872a4&language=ru";
        private static string UrlSortedByDate = $"https://newsapi.org/v2/top-headlines?apiKey=b8cc28acb3634fcc937daae3b28872a4&language=ru";
         
        private static string PATH = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "articles.json");
        private bool SortByDate = true;
        private string Title = "";
        private string Sort = "popularity"; // publishedAt
        private string Category;
        private string From = "";
        private DateTime DateFrom = DateTime.MinValue; //DateTime.MinValue; 

        public async void GetNewsAsync()
        {
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "NewsAPI.Android");


            if ((Url != UrlSortedByCategory) || (Url != UrlSortedByDate))
                Url = DefaultUrl;
            FormateDate();
            ChangeCategory();
            GetReadyUrl();



            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(Url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(responseBody);
                List<Article> news = new List<Article>();

                news = myDeserializedClass.articles;
                news.ForEach(art => art.data = art.publishedAt.ToString());

                newsList.ItemsSource = news;


                string jsonFile = JsonConvert.SerializeObject(news);
                System.IO.File.WriteAllText(PATH, jsonFile);
            }
            catch (HttpRequestException httpErr) { Console.WriteLine($"HTTP error occurred: {httpErr.Message}"); }
            catch (Exception err) { Console.WriteLine($"An error occurred: {err.Message}"); }
        }

        private void GetReadyUrl()
        {
            if (!SortByDate)
            {
                Url = $"https://newsapi.org/v2/top-headlines?apiKey=b8cc28acb3634fcc937daae3b28872a4&language=ru";
                if (!string.IsNullOrWhiteSpace(Category))
                    Url += $"&category={Category.Trim()}";
            }
            else
            {
                Url = $"https://newsapi.org/v2/everything?apiKey=b8cc28acb3634fcc937daae3b28872a4&language=ru";
                Sort = "publishedat";
                Url += $"&sortby={Sort}";
                if (DateFrom != DateTime.MinValue)
                    Url += From;
            }

            if (!string.IsNullOrWhiteSpace(titleLabel.Text))
                Title = titleLabel.Text;
            Url += "&q=" + Title.Trim().ToLower();
        }

        private void ChangeCategory()
        {
            switch (CategoryFilter.SelectedIndex)
            {
                case 0: Category = "business"; break;
                case 1: Category = "entertainment"; break;
                case 2: Category = "general"; break;
                case 3: Category = "health"; break;
                case 4: Category = "science"; break;
                case 5: Category = "technology"; break;
                case 6: Category = "sports"; break;
                default: Category = ""; break;
            }
        }

        private void FormateDate()
        {
            switch (DateFilter.SelectedIndex)
            {
                case 0:
                    DateFrom = DateTime.UtcNow.AddDays(-30); break;
                case 1:
                    DateFrom = DateTime.UtcNow.AddDays(-7); break;
                case 2:
                    DateFrom = DateTime.UtcNow.AddDays(-1); break;
                default:
                    DateFrom = DateTime.MinValue; break;
            }

            if (DateFrom == DateTime.MinValue)
                return;

            // форматируем дату в тип 2024-03-02 {yyyy-mm-dd}
            string[] date = DateFrom.ToShortDateString().Split('/');
            From = $"&from={date[2]}-{date[1]}-{date[0]}";
            //DateFrom = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
        }

        private void Sort_CheckedChanged(object sender, CheckedChangedEventArgs e) 
        {
            if (!DateSort.IsChecked)
            {
                CategoryList.IsVisible = true;
                DateList.IsVisible = false;
                Url = UrlSortedByDate;
                SortByDate = false;
            }
                
            else
            {
                CategoryList.IsVisible = false;
                DateList.IsVisible = true;
                Url = UrlSortedByCategory;
                SortByDate = true;
            }
                
        }

        private void Button_Clicked(object sender, EventArgs e) => GetNewsAsync();

        private async void SelectedItem(object sender, SelectedItemChangedEventArgs e)
        {
            Article selectedArtice = (Article)e.SelectedItem;
            WatchNews watchNews = new WatchNews();
            watchNews.BindingContext = selectedArtice;
            await Navigation.PushAsync(watchNews);
        }
    }

    public class Article
    {
        public Source source { get; set; }
        public string author { get; set; }
        public string data { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string urlToImage { get; set; }
        public DateTime publishedAt { get; set; }
        public string content { get; set; }
    }
    public class Root
    {
        public string status { get; set; }
        public int totalResults { get; set; }
        public List<Article> articles { get; set; }
    }
    public class Source
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}



//FormateDate();  
//ChangeCategory();
//GetReadyUrl();



//public string api_key = "&apiKey=b8cc28acb3634fcc937daae3b28872a4";
//public string Title = "Elon Musk";
//public string Sort = "popularity"; // publishedAt
//public string Category;
//public DateTime DateFrom = DateTime.Today.AddDays(-2);//DateTime.MinValue; //2024-03-02
//public string Url = $"https://newsapi.org/v2/everything?q=";


//string PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "text2.json");
//System.IO.File.WriteAllText(PATH, responseBody);
//string json = System.IO.File.ReadAllText(PATH);


//articles[0].author
// Возможность открыть новость полностью (детальный просмотр новости)
// Создать переход на нову страниу с содержимым статьи


// добавить базу данных для оффлайн просмотра?????