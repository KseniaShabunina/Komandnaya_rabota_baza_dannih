using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DeaneryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DeaneryApp
{
    public partial class MainWindow : Window
    {
        private AppDbContext _context;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                // Создаем контекст
                _context = new AppDbContext();

                // Проверяем, существует ли база данных
                if (!_context.Database.CanConnect())
                {
                    // Если базы данных нет, создаем её и заполняем тестовыми данными
                    _context.Database.Migrate(); // Применяем миграции
                    SeedTestData(); // Заполняем тестовыми данными
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SeedTestData()
        {
            // Инициализация тестовых данных
            if (!_context.Students.Any())
            {
                _context.Students.Add(new Student
                {
                    FirstName = "Иван",
                    LastName = "Иванов",
                    GroupName = "Группа 1",
                    Grades = new List<Grade>
                    {
                        new Grade { Subject = "Математика", Score = 5 },
                        new Grade { Subject = "Физика", Score = 4 }
                    }
                });

                _context.Students.Add(new Student
                {
                    FirstName = "Петр",
                    LastName = "Петров",
                    GroupName = "Группа 2",
                    Grades = new List<Grade>
                    {
                        new Grade { Subject = "Химия", Score = 3 }
                    }
                });

                _context.SaveChanges();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (username == "admin" && password == "admin")
            {
                // Успешный вход
                LoginForm.Visibility = Visibility.Collapsed;
                MainInterface.Visibility = Visibility.Visible;
                LoadStudents();
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStudents()
        {
            try
            {
                // Загружаем студентов после настройки контекста
                StudentsDataGrid.ItemsSource = _context.Students.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке студентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text;

            try
            {
                var students = _context.Students
                    .Where(s => s.LastName.Contains(searchTerm) || s.GroupName.Contains(searchTerm))
                    .ToList();

                StudentsDataGrid.ItemsSource = students;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске студентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StudentsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItem is Student selectedStudent)
            {
                try
                {
                    var grades = GetGradesForStudent(selectedStudent.ID);
                    if (grades.Any())
                    {
                        MessageBox.Show($"Оценки для {selectedStudent.FirstName} {selectedStudent.LastName}: \n{string.Join(", ", grades.Select(g => $"{g.Subject}: {g.Score}"))}", "Оценки", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"У студента {selectedStudent.FirstName} {selectedStudent.LastName} нет оценок.", "Оценки", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке оценок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private List<Grade> GetGradesForStudent(int studentId)
        {
            return _context.Grades.Where(g => g.StudentID == studentId).ToList();
        }

        // Методы для управления placeholder
        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Введите фамилию или группу")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.Black;
            }
        }

        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Введите фамилию или группу";
                SearchTextBox.Foreground = Brushes.Gray;
            }
        }

        // Обработчик изменения пароля
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length > 0)
            {
                PasswordBox.Tag = ""; // Скрываем плейсхолдер
            }
            else
            {
                PasswordBox.Tag = "Пароль"; // Показываем плейсхолдер
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose(); // Освобождение ресурсов
            base.OnClosed(e);
        }
    }
}