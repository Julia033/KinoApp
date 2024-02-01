using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;
using KinoApp.Model;
using OpenQA.Selenium;

namespace KinoApp.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Parser parser;

        private int _yearFrom;
        public int YearFrom
        {
            get { return _yearFrom; }
            set
            {
                if (_yearFrom != value)
                {
                    _yearFrom = value;
                    OnPropertyChanged(nameof(YearFrom));
                }
            }
        }

        private int _yearTo;
        public int YearTo
        {
            get { return _yearTo; }
            set
            {
                if (_yearTo != value)
                {
                    _yearTo = value;
                    OnPropertyChanged(nameof(YearTo));
                }
            }
        }

        private List<Film> filmList;
        public List<Film> FilmList
        {
            get => filmList;
            set
            {
                filmList = value;
                OnPropertyChanged("FilmList");
            }
        }

        #region Парсинг
        private ICommand _startParsingCommand;
        public ICommand StartParsingCommand
        {
            get
            {
                if (_startParsingCommand == null)
                {
                    _startParsingCommand = new Command(async param => await StartParsing()); //, param => CanStartParsing());
                }
                return _startParsingCommand;
            }
        }

        private Command getFilms;
        public Command GetFilms
        {
            get
            {
                getFilms = new Command(obj =>
                {
                    using (var db = new dbContext())
                    {
                        var FilmsInDB = db.Films.OrderBy(x => x.ID_Film).ToList(); // получаем данные из бд

                        FilmList = FilmsInDB.Select(x => new Film // создаем новый список
                        {
                            ID_Film = x.ID_Film,
                            Name = x.Name,
                            Year = x.Year,
                            Rank = x.Rank,
                            Country = x.Country,
                            Genres = x.Genres
                        }).ToList();
                    }
                });
                return getFilms;
            }
        }

        private async Task StartParsing()
        {
            parser = new Parser();
            await parser.ParseData(YearFrom, YearTo);
            GetFilms.Execute(null);
            IsDataLoaded = true;
        }

        //private bool CanStartParsing()
        //{
        //    return // YearFrom != null && YearTo != null &&
        //           YearFrom >= 2000 && YearFrom <= 2024 &&
        //           YearTo >= 2000 && YearTo <= 2024 &&
        //           YearFrom <= YearTo;
        //}
        #endregion

        #region Работа с данными

        private Film _selectedFilm;
        public Film SelectedFilm
        {
            get { return _selectedFilm; }
            set
            {
                _selectedFilm = value;
                OnPropertyChanged(nameof(SelectedFilm));
            }
        }

        private ICommand _deleteSelectedCommand;
        public ICommand DeleteSelectedCommand
        {
            get
            {
                if (_deleteSelectedCommand == null)
                    _deleteSelectedCommand = new Command(param => DeleteSelected(), param => CanDeleteSelected());

                return _deleteSelectedCommand;
            }
        }

        private void DeleteSelected()
        {
            using (var db = new dbContext())
            {
                var filmToDelete = db.Films.FirstOrDefault(c => c.ID_Film == SelectedFilm.ID_Film);
                if (filmToDelete != null)
                {
                    db.Films.Remove(filmToDelete);
                    db.SaveChanges();
                }
            }
            GetFilms.Execute(null);
        }
        private bool CanDeleteSelected()
        {
            return SelectedFilm != null;
        }

        private ICommand _deleteAllCommand;
        public ICommand DeleteAllCommand
        {
            get
            {
                if (_deleteAllCommand == null)
                    _deleteAllCommand = new Command(param => DeleteAll(), param => CanDeleteAll());
                return _deleteAllCommand;
            }
        }

        private void DeleteAll()
        {
            using (var db = new dbContext())
            {
                db.Films.RemoveRange(db.Films);
                db.Countries.RemoveRange(db.Countries);
                db.Genres.RemoveRange(db.Genres);
                db.SaveChanges();
            }
            GetFilms.Execute(null);
            MessageBox.Show("Все данные успешно удалены.");
        }

        private bool CanDeleteAll()
        {
            return FilmList != null && FilmList.Count > 0;
        }

        private ICommand _editFilmCommand;
        public ICommand EditFilmCommand
        {
            get
            {
                if (_editFilmCommand == null)
                {
                    _editFilmCommand = new Command(obj =>
                    {
                        try
                        {
                            using (var db = new dbContext())
                            {
                                var filmToEdit = db.Films // получаем существующий объект из контекста
                                    .Include(c => c.Country)
                                    .Include(c => c.Genres)
                                    .FirstOrDefault(c => c.ID_Film == SelectedFilm.ID_Film);

                                if (filmToEdit != null)
                                {
                                    db.Entry(filmToEdit).CurrentValues.SetValues(SelectedFilm); // обновляем свойства filmToEdit на основе SelectedFilm...
                                    db.Entry(filmToEdit.Country).CurrentValues.SetValues(SelectedFilm.Country); // обновляем данные в таблице Courts
                                    filmToEdit.Genres.Clear(); // обновляем данные в таблице Genres
                                    foreach (var selectedGenre in SelectedFilm.Genres)
                                    {
                                        var existingGenre = db.Genres.Find(selectedGenre.ID_Genre);
                                        if (existingGenre != null)
                                            filmToEdit.Genres.Add(existingGenre);
                                    }

                                    db.SaveChanges();
                                }
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Некорректный ввод! Повторите попытку.");
                        }
                    });
                }
                return _editFilmCommand;
            }
        }

        private bool _isDataLoaded = false;
        public bool IsDataLoaded
        {
            get { return _isDataLoaded; }
            set
            {
                _isDataLoaded = value;
                OnPropertyChanged(nameof(IsDataLoaded));
            }
        }

        private ICommand _updateCommand;
        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new Command(async param =>
                    {
                        DeleteAll();
                        try
                        {
                            await StartParsing();
                        }
                        catch
                        {
                            await parser.ParseData(YearFrom, YearTo);
                        }
                        GetFilms.Execute(null);
                    });
                }
                return _updateCommand;
            }
        }
        #endregion



    }
}
