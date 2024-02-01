using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using KinoApp.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Markup;
using System.Runtime.InteropServices;

namespace KinoApp.ViewModel
{
    public class Parser
    {
        private IWebDriver driver;

        public Parser()
        {
            InitializeWebDriver();
        }
        private void InitializeWebDriver()
        {
            driver = new ChromeDriver();
            driver.Url = @"https://www.kinopoisk.ru/s/portal.html";
        }

        public async Task ParseData(int yearFrom, int yearTo)
        {
            await Task.Delay(3000);
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    driver.FindElement(By.CssSelector(".yearSB1"));
                    await Task.Delay(2000);
                    break;
                }
                catch (Exception)
                {
                    await Task.Delay(4000);
                }
            }
            await Task.Delay(2000);
            driver.FindElement(By.CssSelector(".yearSB1")).SendKeys(yearFrom.ToString());
            await Task.Delay(500);
            driver.FindElement(By.CssSelector(".yearSB2")).SendKeys(yearTo.ToString());
            await Task.Delay(500);
            driver.FindElement(By.CssSelector("text el_17")).SendKeys("фильм");
            await Task.Delay(500);
            driver.FindElement(By.XPath("//input[@class='el_18 submit nice_button']")).Click();
            await Task.Delay(5000);
            driver.FindElement(By.XPath("//a[contains(.,'показать все')]")).Click();
            await Task.Delay(5000);
            driver.FindElement(By.XPath("//div[@class='element']//p[@class='name']/a")).Click();
            await Task.Delay(1000);


            var oldNumber = string.Empty;
            var counter = 0;
            while (true)
            {
                await Task.Delay(3000);
                counter++;
                if (counter == 8) break;
                await Task.Delay(500);
                var _name = driver.FindElement(By.XPath("(//span[@data-tid='75209b22'])")).GetAttribute("textContent");
                var _year = driver.FindElement(By.XPath("//div[contains(@class, 'styles_row') and contains(text(), 'Год производства')]/div[@class='styles_valueDark__BCk93 styles_value__g6yP4']/a")).GetAttribute("textContent");
                var _genre = driver.FindElement(By.XPath("(//span[@data-tid='d5ff4cc'])")).GetAttribute("textContent");
                var _rank = "-";
                try
                {
                    var _rankElement = driver.FindElement(By.XPath("(//span[@data-tid='939058a8'])"));
                    _rank = _rankElement.GetAttribute("textContent");
                }
                catch (NoSuchElementException)
                {}
                var _country = driver.FindElement(By.XPath("(//span[@data-tid=603f73a4'])")).GetAttribute("textContent");
                using (var context = new dbContext())
                {
                    Country Country = context.Countries.FirstOrDefault(c => c.Name == _country);
                    if (Country == null)
                    {
                        Country = new Country()
                        {
                            Name = _country,
                        };
                        context.Countries.Add(Country);
                    }
                    else
                    {
                        Country.Name = _country;
                    }

                    var existingFilm = context.Films.FirstOrDefault(f => f.Name == _name);

                    if (existingFilm == null)
                    {
                        existingFilm = new Film()
                        {
                            Name = _name,
                            Year = _year,
                            Rank = _rank,
                            Country = Country
                        };
                        context.Films.Add(existingFilm);
                    }
                    else
                    {
                        existingFilm.Name = _name;
                        existingFilm.Year = _year;
                        existingFilm.Rank = _rank;
                        existingFilm.Country = Country;
                    }

                    var genreNames = _genre.Split(','); //разбивка жанров
                    foreach (var genreName in genreNames)
                    {
                        var Genre = context.Genres.FirstOrDefault(g => g.Name == genreName.Trim());
                        if (Genre == null)
                        {
                            Genre = new Genre()
                            {
                                Name = genreName.Trim()
                            };
                            context.Genres.Add(Genre);
                        }
                        existingFilm.Genres.Add(Genre);
                    }
                    context.SaveChanges();
                }

            }
            driver.Quit();
        }
    }
}
