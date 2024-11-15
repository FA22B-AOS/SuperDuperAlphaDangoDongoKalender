using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TerminKalender.Controllers;
using TerminKalender.Models;

namespace TerminKalender.Views
{
    /**
     * Hauptfenster der Anwendung, das die Benutzeroberfläche für die Terminverwaltung bereitstellt.
     */
    public partial class MainWindow : Window
    {
        /** 
         * Controller zur Verwaltung der Kalenderlogik und der Dateninteraktion.
         */
        private readonly CalendarController controller;

        /** 
         * Der aktuell ausgewählte Termin, der bearbeitet oder gelöscht werden kann.
         */
        private Appointment selectedAppointment;

        /** 
         * Gibt an, ob sich die Anwendung im Bearbeitungsmodus befindet.
         */
        private bool isEditing;

        /**
         * Konstruktor für das Hauptfenster. Initialisiert die Benutzeroberfläche und lädt die Daten.
         */
        public MainWindow()
        {
            InitializeComponent();
            controller = new CalendarController();
            CalendarControl.DisplayDate = DateTime.Now;
            CalendarControl.SelectedDate = DateTime.Now;
            UpdateAppointmentsList(DateTime.Now);
            InitializeTimeComboBoxes();
            UpdateHeaderAndButton(); // Initiale Überschrift und Button-Text setzen
        }

        /**
         * Initialisiert die Zeit-ComboBoxen mit Optionen im 15-Minuten-Takt.
         */
        private void InitializeTimeComboBoxes()
        {
            var timeOptions = GenerateTimeOptions();
            StartTimeComboBox.ItemsSource = timeOptions;
            EndTimeComboBox.ItemsSource = timeOptions;

            // Standardmäßig die erste Zeit auswählen
            StartTimeComboBox.SelectedIndex = 0;
            EndTimeComboBox.SelectedIndex = 1;
        }

        /**
         * Generiert eine Liste von Zeitoptionen im Format HH:mm im 15-Minuten-Takt.
         * 
         * @return Eine Liste von Zeitoptionen.
         */
        private List<string> GenerateTimeOptions()
        {
            var times = new List<string>();
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 15)
                {
                    times.Add($"{hour:D2}:{minute:D2}");
                }
            }
            return times;
        }

        /**
         * Aktualisiert die Überschrift und den Text des Buttons basierend auf dem Bearbeitungsmodus.
         */
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

        /**
         * Aktualisiert die Terminliste für ein bestimmtes Datum.
         * 
         * @param selectedDate Das Datum, für das die Termine angezeigt werden sollen.
         */
        private void UpdateAppointmentsList(DateTime selectedDate)
        {
            var appointments = controller.GetAppointmentsForDay(selectedDate);
            AppointmentsListBox.ItemsSource = appointments;
        }

        /**
         * Event-Handler für den "Speichern"-Button.
         * Fügt einen neuen Termin hinzu oder aktualisiert einen bestehenden.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void AddAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (CalendarControl.SelectedDate == null)
            {
                MessageBox.Show("Bitte wählen Sie ein Datum aus.");
                return;
            }

            string title = TitleTextBox.Text;
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Bitte geben Sie einen Titel ein.");
                return;
            }

            if (StartTimeComboBox.SelectedItem == null || EndTimeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie Start- und Endzeit aus.");
                return;
            }

            // Parse der ausgewählten Zeiten
            if (!TimeSpan.TryParse(StartTimeComboBox.SelectedItem.ToString(), out TimeSpan startTime) ||
                !TimeSpan.TryParse(EndTimeComboBox.SelectedItem.ToString(), out TimeSpan endTime))
            {
                MessageBox.Show("Ungültige Zeitangabe.");
                return;
            }

            if (endTime <= startTime)
            {
                MessageBox.Show("Die Endzeit muss nach der Startzeit liegen.");
                return;
            }

            // Datum und Uhrzeit kombinieren
            DateTime selectedDate = CalendarControl.SelectedDate.Value;
            DateTime startDateTime = selectedDate.Date.Add(startTime);
            DateTime endDateTime = selectedDate.Date.Add(endTime);

            if (isEditing && selectedAppointment != null)
            {
                controller.UpdateAppointment(selectedAppointment, title, startDateTime, endDateTime);
                MessageBox.Show("Termin erfolgreich aktualisiert.");
                isEditing = false;
            }
            else
            {
                if (!controller.AddAppointment(title, startDateTime, endDateTime, out string errorMessage))
                {
                    MessageBox.Show(errorMessage);
                    return;
                }

                MessageBox.Show("Termin erfolgreich hinzugefügt.");
            }

            ClearFields();
            UpdateAppointmentsList(selectedDate);
            UpdateHeaderAndButton();
        }

        /**
         * Event-Handler für den Kontextmenüpunkt "Termin bearbeiten".
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void EditAppointmentContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsListBox.SelectedItem is Appointment appointment)
            {
                selectedAppointment = appointment;
                TitleTextBox.Text = appointment.Title;

                // Setze Start- und Endzeit in die ComboBoxen
                StartTimeComboBox.SelectedItem = appointment.StartDate.ToString("HH:mm");
                EndTimeComboBox.SelectedItem = appointment.EndDate.ToString("HH:mm");

                isEditing = true;
                UpdateHeaderAndButton();
            }
        }

        /**
         * Event-Handler für den Kontextmenüpunkt "Termin löschen".
         * Zeigt eine Bestätigungsnachricht an, bevor der Termin gelöscht wird.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void DeleteAppointmentContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsListBox.SelectedItem is Appointment appointment)
            {
                var result = MessageBox.Show($"Möchten Sie den Termin '{appointment.Title}' wirklich löschen?",
                                             "Termin löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    controller.DeleteAppointment(appointment);
                    MessageBox.Show("Termin erfolgreich gelöscht.");
                    UpdateAppointmentsList(DateTime.Now);
                }
            }
        }

        /**
         * Event-Handler für den "Abbrechen"-Button.
         * Beendet den Bearbeitungsmodus und leert die Eingabefelder.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            // Bearbeitungsmodus beenden und Eingabefelder leeren
            isEditing = false;
            selectedAppointment = null;
            ClearFields();
            UpdateHeaderAndButton(); // Überschrift und Button-Text zurücksetzen
        }

        /**
         * Leert die Eingabefelder und setzt die ComboBoxen zurück.
         */
        private void ClearFields()
        {
            // Titel-Textbox leeren
            TitleTextBox.Clear();

            // ComboBoxen zurücksetzen
            if (StartTimeComboBox.Items.Count > 0)
            {
                StartTimeComboBox.SelectedIndex = 0; // Standardmäßig die erste Zeit auswählen
            }

            if (EndTimeComboBox.Items.Count > 0)
            {
                EndTimeComboBox.SelectedIndex = 1; // Standardmäßig die zweite Zeit auswählen
            }

            // Bearbeitungsmodus verlassen
            isEditing = false;
            selectedAppointment = null;
        }

        /**
         * Methode zum Schließen des Fensters.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /**
         * Methode zum Minimieren des Fensters.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /**
         * Methode zum Maximieren oder Wiederherstellen des Fensters.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        /**
         * Methode zum Verschieben des Fensters.
         * Wird ausgelöst, wenn die linke Maustaste gedrückt wird.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        /**
         * Event-Handler für die Auswahl eines Monats.
         * Aktualisiert den Kalender und die Terminliste für den ausgewählten Monat.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void OnMonthButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int month))
            {
                // Erstelle das Datum mit dem ausgewählten Jahr und Monat, setze den Tag auf den 1.
                DateTime selectedDate = new DateTime(DateTime.Now.Year, month, 1);

                // Aktualisiere den Kalender, um Termine für das erste Datum des ausgewählten Monats anzuzeigen
                CalendarControl.DisplayDate = selectedDate;
                UpdateAppointmentsList(selectedDate);
            }
        }

        /**
         * Event-Handler für die Auswahl eines Datums im Kalender.
         * Aktualisiert die Terminliste für das ausgewählte Datum.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void OnCalendarSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalendarControl.SelectedDate.HasValue)
            {
                // Hole das ausgewählte Datum
                DateTime selectedDate = CalendarControl.SelectedDate.Value;

                // Aktualisiere die Terminliste für das ausgewählte Datum
                UpdateAppointmentsList(selectedDate);
            }
        }

        /**
         * Event-Handler für das direkte Auswählen eines Jahres.
         * Aktualisiert den Kalender und die Terminliste für das ausgewählte Jahr.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void OnYearButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int year))
            {
                DateTime selectedDate = new DateTime(year, CalendarControl.DisplayDate.Month, 1);
                CalendarControl.DisplayDate = selectedDate;
                UpdateAppointmentsList(selectedDate);
            }
        }

        /**
         * Event-Handler für das Navigieren zum vorherigen Jahr.
         * Aktualisiert den Kalender und die Terminliste für das vorherige Jahr.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void OnPreviousYearClick(object sender, RoutedEventArgs e)
        {
            // Suche den hervorgehobenen Button; falls keiner gefunden wird, nimm ein Standardjahr
            var highlightedButton = yearPanel.Children
                .OfType<Button>()
                .FirstOrDefault(b => b.FontWeight == FontWeights.Bold);

            int currentYear;
            if (highlightedButton != null && int.TryParse(highlightedButton.Tag.ToString(), out currentYear))
            {
                int previousYear = currentYear - 1;

                // Aktualisiere das Jahr in DisplayDate
                CalendarControl.DisplayDate = new DateTime(previousYear, CalendarControl.DisplayDate.Month, 1);

                // Aktualisiere die Jahresauswahl
                UpdateYearSelection(previousYear);
            }
            else
            {
                // Fallback: Standardjahr verwenden
                currentYear = CalendarControl.DisplayDate.Year;
                UpdateYearSelection(currentYear - 1);
            }
        }

        /**
         * Event-Handler für das Navigieren zum nächsten Jahr.
         * Aktualisiert den Kalender und die Terminliste für das nächste Jahr.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void OnNextYearClick(object sender, RoutedEventArgs e)
        {
            // Suche den hervorgehobenen Button; falls keiner gefunden wird, nimm ein Standardjahr
            var highlightedButton = yearPanel.Children
                .OfType<Button>()
                .FirstOrDefault(b => b.FontWeight == FontWeights.Bold);

            int currentYear;
            if (highlightedButton != null && int.TryParse(highlightedButton.Tag.ToString(), out currentYear))
            {
                int nextYear = currentYear + 1;

                // Aktualisiere das Jahr in DisplayDate
                CalendarControl.DisplayDate = new DateTime(nextYear, CalendarControl.DisplayDate.Month, 1);

                // Aktualisiere die Jahresauswahl
                UpdateYearSelection(nextYear);
            }
            else
            {
                // Fallback: Standardjahr verwenden
                currentYear = CalendarControl.DisplayDate.Year;
                UpdateYearSelection(currentYear + 1);
            }
        }

        /**
         * Aktualisiert die Jahresauswahl im yearPanel basierend auf dem aktuellen Jahr.
         * 
         * @param baseYear Das mittlere Jahr, um das die Buttons herum aufgebaut werden.
         */
        private void UpdateYearSelection(int baseYear)
        {
            int index = 0;

            foreach (var child in yearPanel.Children)
            {
                if (child is Button button)
                {
                    // Aktualisiere nur Buttons mit einem gültigen Tag
                    if (button.Tag != null && int.TryParse(button.Tag.ToString(), out _))
                    {
                        int updatedYear = baseYear - 2 + index;
                        button.Tag = updatedYear;
                        button.Content = updatedYear;

                        // Stil zurücksetzen
                        button.FontSize = 16;
                        button.FontWeight = FontWeights.Normal;
                        button.Foreground = Brushes.Black;

                        // Hebe das aktuelle Jahr hervor
                        if (updatedYear == baseYear)
                        {
                            button.FontSize = 24;
                            button.FontWeight = FontWeights.Bold;
                            button.Foreground = new SolidColorBrush(Color.FromRgb(199, 111, 105));
                        }

                        index++;
                    }
                }
            }
        }

        /**
         * Event-Handler, wenn eine TextBox den Fokus erhält.
         * Entfernt den Platzhaltertext, falls vorhanden.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == GetPlaceholderText(textBox))
                {
                    textBox.Text = string.Empty;
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }

        /**
         * Event-Handler, wenn eine TextBox den Fokus verliert.
         * Fügt den Platzhaltertext hinzu, falls das Feld leer ist.
         * 
         * @param sender Das auslösende Objekt.
         * @param e Die Ereignisdaten.
         */
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    SetPlaceholderText(textBox);
                }
            }
        }

        /**
         * Setzt den Platzhaltertext für eine bestimmte TextBox.
         * 
         * @param textBox Die TextBox, für die der Platzhaltertext gesetzt werden soll.
         */
        private void SetPlaceholderText(TextBox textBox)
        {
            if (textBox == TitleTextBox)
            {
                textBox.Text = "Titel des Termins";
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        /**
         * Gibt den Platzhaltertext für eine bestimmte TextBox zurück.
         * 
         * @param textBox Die TextBox, für die der Platzhaltertext zurückgegeben werden soll.
         * @return Der Platzhaltertext der TextBox.
         */
        private string GetPlaceholderText(TextBox textBox)
        {
            if (textBox == TitleTextBox)
            {
                return "Titel des Termins";
            }
            return string.Empty;
        }

        /**
         * Überschreibt die OnClosing-Methode.
         * Speichert die Termine, bevor die Anwendung geschlossen wird.
         * 
         * @param e Die Ereignisdaten zum Schließen des Fensters.
         */
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            controller.SaveAppointments();
        }
    }
}