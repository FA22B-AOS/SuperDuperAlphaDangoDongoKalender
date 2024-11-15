using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TerminKalender
{
    public partial class MainWindow : Window
    {
        private Calendar calendar = new Calendar();
        private string csvFilePath = "appointments.csv";
        private Appointment selectedAppointment = null;
        private bool isEditing = false;
        private DateTime shownDate;

        public MainWindow()
        {
            InitializeComponent();
            UpdateHeaderAndButton();
            shownDate = DateTime.Now;
            UpdateCalendarDisplay();

            // Platzhalter initialisieren
            SetPlaceholderText(TitleTextBox, "Titel des Termins");
            SetPlaceholderText(StartTimeTextBox, "HH:MM");
            SetPlaceholderText(EndTimeTextBox, "HH:MM");

            // CSV-Datei beim Start laden
            calendar.LoadFromCsv(csvFilePath);
            UpdateAppointmentsList();

            // Event-Handler für das Beenden der Anwendung
            this.Closed += MainWindow_Closed;
        }

        // Methode zum Schließen des Fensters
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Methode zum Minimieren des Fensters
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Methode zum Verschieben des Fensters
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Methode zum Aktualisieren der Überschrift und des Button-Textes
        private void UpdateHeaderAndButton()
        {
            if (isEditing)
            {
                HeaderTextBlock.Text = "Termin bearbeiten";
                SaveButton.Content = "Speichern";
                CancelButton.Visibility = Visibility.Visible; // "Abbrechen"-Button anzeigen
            }
            else
            {
                HeaderTextBlock.Text = "Neuen Termin hinzufügen";
                SaveButton.Content = "Speichern";
                CancelButton.Visibility = Visibility.Collapsed; // "Abbrechen"-Button verbergen
            }
        }

        // Event-Handler für das Kontextmenü "Termin bearbeiten"
        private void EditAppointmentContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsListBox.SelectedItem is Appointment appointmentToEdit)
            {
                selectedAppointment = appointmentToEdit;
                TitleTextBox.Text = appointmentToEdit.Title;
                StartTimeTextBox.Text = appointmentToEdit.StartDate.ToString("HH:mm");
                EndTimeTextBox.Text = appointmentToEdit.EndDate.ToString("HH:mm");
                CalendarControl.SelectedDate = appointmentToEdit.StartDate.Date;

                isEditing = true; // Bearbeitungsmodus aktivieren
                UpdateHeaderAndButton(); // Überschrift und Buttons aktualisieren
            }
        }

        // Event-Handler für den "Abbrechen"-Button
        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Bearbeitungsmodus beenden und Eingabefelder leeren
            isEditing = false;
            selectedAppointment = null;
            ClearInputFields();
            UpdateHeaderAndButton(); // Überschrift und Button-Text zurücksetzen
        }


        private void DeleteAppointmentContextMenu_Click(object sender, RoutedEventArgs e)
        {
            // Prüfen, ob ein Termin ausgewählt ist
            if (AppointmentsListBox.SelectedItem is Appointment appointmentToDelete)
            {
                // Bestätigungsdialog anzeigen
                MessageBoxResult result = MessageBox.Show(
                    $"Möchten Sie den Termin '{appointmentToDelete.Title}' wirklich löschen?",
                    "Termin löschen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                // Wenn der Benutzer "Ja" wählt, den Termin löschen
                if (result == MessageBoxResult.Yes)
                {
                    calendar.DeleteAppointment(appointmentToDelete);
                    UpdateAppointmentsList(); // Liste aktualisieren
                    MessageBox.Show("Termin erfolgreich gelöscht.");
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie einen Termin aus, den Sie löschen möchten.");
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // CSV-Datei beim Beenden speichern
            calendar.SaveToCsv(csvFilePath);
        }

        private void OnYearButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int year))
            {
                shownDate = new DateTime(year, shownDate.Month, 1);
                UpdateCalendarDisplay();
            }
        }

        private void OnMonthButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int month))
            {
                shownDate = new DateTime(shownDate.Year, month, 1);
                UpdateCalendarDisplay();
            }
        }

        private void OnPreviousYearClick(object sender, RoutedEventArgs e)
        {
            shownDate = shownDate.AddYears(-1);
            UpdateCalendarDisplay();
        }

        private void OnNextYearClick(object sender, RoutedEventArgs e)
        {
            shownDate = shownDate.AddYears(1);
            UpdateCalendarDisplay();
        }

        private void OnCalendarSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalendarControl.SelectedDate.HasValue)
            {
                shownDate = CalendarControl.SelectedDate.Value;
                UpdateAppointmentsList();
            }
        }

        private void UpdateCalendarDisplay()
        {
            CalendarControl.DisplayDate = shownDate;
            CalendarControl.SelectedDate = shownDate;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAppointmentsList();
        }

        // Event-Handler für den "Speichern"-Button
        private void AddAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (CalendarControl.SelectedDate == null)
            {
                MessageBox.Show("Bitte wählen Sie ein Datum aus.");
                return;
            }

            string title = TitleTextBox.Text;
            if (string.IsNullOrWhiteSpace(title) || title == "Titel des Termins")
            {
                MessageBox.Show("Bitte geben Sie einen Titel ein.");
                return;
            }

            if (!DateTime.TryParse(StartTimeTextBox.Text, out DateTime startTime))
            {
                MessageBox.Show("Bitte geben Sie eine gültige Startzeit im Format HH:MM ein.");
                return;
            }

            if (!DateTime.TryParse(EndTimeTextBox.Text, out DateTime endTime))
            {
                MessageBox.Show("Bitte geben Sie eine gültige Endzeit im Format HH:MM ein.");
                return;
            }

            if (endTime <= startTime)
            {
                MessageBox.Show("Die Endzeit muss nach der Startzeit liegen.");
                return;
            }

            DateTime selectedDate = CalendarControl.SelectedDate.Value;
            DateTime startDateTime = selectedDate.Date.Add(startTime.TimeOfDay);
            DateTime endDateTime = selectedDate.Date.Add(endTime.TimeOfDay);

            var newAppointment = new Appointment(title, startDateTime, endDateTime);

            if (isEditing && selectedAppointment != null)
            {
                calendar.UpdateAppointment(selectedAppointment, newAppointment);
                MessageBox.Show("Termin erfolgreich bearbeitet.");
                isEditing = false;
                selectedAppointment = null;
            }
            else
            {
                if (calendar.AddAppointment(newAppointment))
                {
                    MessageBox.Show("Termin erfolgreich hinzugefügt.");
                }
                else
                {
                    MessageBox.Show("Terminüberschneidung! Der Termin konnte nicht hinzugefügt werden.");
                }
            }

            UpdateAppointmentsList();
            ClearInputFields();
            UpdateHeaderAndButton(); // Überschrift und Button-Text zurücksetzen
        }


        private void UpdateAppointmentsList()
        {
            if (CalendarControl.SelectedDate.HasValue)
            {
                DateTime selectedDate = CalendarControl.SelectedDate.Value;
                var appointmentsForDay = calendar.GetAppointmentsForDay(selectedDate);
                AppointmentsListBox.ItemsSource = appointmentsForDay;
            }
            else
            {
                AppointmentsListBox.ItemsSource = null;
            }
        }

        private void DeleteAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsListBox.SelectedItem is Appointment appointmentToDelete)
            {
                calendar.DeleteAppointment(appointmentToDelete);
                UpdateAppointmentsList();
                MessageBox.Show("Termin erfolgreich gelöscht.");
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie einen Termin aus, den Sie löschen möchten.");
            }
        }

        private void EditAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsListBox.SelectedItem is Appointment appointmentToEdit)
            {
                selectedAppointment = appointmentToEdit;
                TitleTextBox.Text = appointmentToEdit.Title;
                StartTimeTextBox.Text = appointmentToEdit.StartDate.ToString("HH:mm");
                EndTimeTextBox.Text = appointmentToEdit.EndDate.ToString("HH:mm");
                CalendarControl.SelectedDate = appointmentToEdit.StartDate.Date;
                isEditing = true;
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie einen Termin aus, den Sie bearbeiten möchten.");
            }
        }

        private void ClearInputFields()
        {
            TitleTextBox.Text = string.Empty;
            StartTimeTextBox.Text = string.Empty;
            EndTimeTextBox.Text = string.Empty;
            SetPlaceholderText(TitleTextBox, "Titel des Termins");
            SetPlaceholderText(StartTimeTextBox, "HH:MM");
            SetPlaceholderText(EndTimeTextBox, "HH:MM");
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                RemovePlaceholderText(textBox);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    SetPlaceholderText(textBox, GetPlaceholderText(textBox));
                }
            }
        }

        private void SetPlaceholderText(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.Foreground = Brushes.Gray;
            textBox.Tag = placeholder;
        }

        private void RemovePlaceholderText(TextBox textBox)
        {
            if (textBox.Text == GetPlaceholderText(textBox))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black;
            }
        }

        private string GetPlaceholderText(TextBox textBox)
        {
            return textBox.Tag as string ?? string.Empty;
        }
    }

    public record Appointment(string Title, DateTime StartDate, DateTime EndDate)
    {
        public string TimeRange => $"{StartDate:HH:mm} - {EndDate:HH:mm}";
    }

    public class Calendar
    {
        private List<Appointment> appointments = new List<Appointment>();

        public bool AddAppointment(Appointment newAppointment)
        {
            if (!appointments.Any(a => (newAppointment.StartDate < a.EndDate) && (newAppointment.EndDate > a.StartDate)))
            {
                appointments.Add(newAppointment);
                return true;
            }
            return false;
        }

        public List<Appointment> GetAppointmentsForDay(DateTime date)
        {
            return appointments.Where(a => a.StartDate.Date == date.Date)
                               .OrderBy(a => a.StartDate)
                               .ToList();
        }


        public void DeleteAppointment(Appointment appointment)
        {
            appointments.Remove(appointment);
        }

        public void UpdateAppointment(Appointment oldAppointment, Appointment newAppointment)
        {
            DeleteAppointment(oldAppointment);
            appointments.Add(newAppointment);
        }

        public void LoadFromCsv(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                var lines = System.IO.File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3 &&
                        DateTime.TryParse(parts[1], out DateTime startDate) &&
                        DateTime.TryParse(parts[2], out DateTime endDate))
                    {
                        var title = parts[0];
                        var newAppointment = new Appointment(title, startDate, endDate);
                        appointments.Add(newAppointment);
                    }
                }
            }
        }

        public void SaveToCsv(string filePath)
        {
            var lines = appointments.Select(a => $"{a.Title},{a.StartDate},{a.EndDate}");
            System.IO.File.WriteAllLines(filePath, lines);
        }
    }
}
