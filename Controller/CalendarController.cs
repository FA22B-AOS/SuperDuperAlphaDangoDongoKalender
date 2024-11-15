using TerminKalender.Models;

namespace TerminKalender.Controllers
{
    /**
     * Controller-Klasse zur Verwaltung von Terminen.
     * Vermittelt zwischen der View (Benutzeroberfläche) und dem Model (Kalender und Termine).
     */
    public class CalendarController
    {
        private readonly Calendar calendar; // Instanz des Kalenders zur Verwaltung von Terminen

        /**
         * Konstruktor der CalendarController-Klasse.
         * Lädt bestehende Termine aus der CSV-Datei und fügt sie dem Kalender hinzu.
         */
        public CalendarController()
        {
            calendar = new Calendar();

            // Termine aus der CSV-Datei laden und in den Kalender einfügen
            var appointments = CsvHelper.LoadAppointments();
            foreach (var appointment in appointments)
            {
                calendar.AddAppointment(appointment);
            }
        }

        /**
         * Gibt alle Termine für ein bestimmtes Datum zurück.
         * 
         * @param date Das Datum, für das die Termine abgerufen werden sollen.
         * @return Eine Liste von Terminen für das angegebene Datum.
         */
        public List<Appointment> GetAppointmentsForDay(DateTime date)
        {
            return calendar.GetAppointmentsForDay(date);
        }

        /**
         * Fügt einen neuen Termin hinzu, wenn keine Überschneidungen mit bestehenden Terminen vorliegen.
         * 
         * @param title Der Titel des neuen Termins.
         * @param startDate Das Startdatum und die Startzeit des Termins.
         * @param endDate Das Enddatum und die Endzeit des Termins.
         * @param errorMessage Gibt eine Fehlermeldung zurück, falls der Termin nicht hinzugefügt werden konnte.
         * @return true, wenn der Termin erfolgreich hinzugefügt wurde; andernfalls false.
         */
        public bool AddAppointment(string title, DateTime startDate, DateTime endDate, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                errorMessage = "Der Titel darf nicht leer sein.";
                return false;
            }

            if (endDate <= startDate)
            {
                errorMessage = "Die Endzeit muss nach der Startzeit liegen.";
                return false;
            }

            var newAppointment = new Appointment(title, startDate, endDate);
            if (calendar.AddAppointment(newAppointment))
            {
                errorMessage = string.Empty;
                return true;
            }

            errorMessage = "Terminüberschneidung! Der Termin konnte nicht hinzugefügt werden.";
            return false;
        }

        /**
         * Aktualisiert einen bestehenden Termin.
         * 
         * @param oldAppointment Der bestehende Termin, der aktualisiert werden soll.
         * @param title Der neue Titel des Termins.
         * @param startDate Das neue Startdatum und die neue Startzeit des Termins.
         * @param endDate Das neue Enddatum und die neue Endzeit des Termins.
         */
        public void UpdateAppointment(Appointment oldAppointment, string title, DateTime startDate, DateTime endDate)
        {
            var newAppointment = new Appointment(title, startDate, endDate);
            calendar.UpdateAppointment(oldAppointment, newAppointment);
        }

        /**
         * Löscht einen bestehenden Termin.
         * 
         * @param appointment Der zu löschende Termin.
         */
        public void DeleteAppointment(Appointment appointment)
        {
            calendar.DeleteAppointment(appointment);
        }

        /**
         * Speichert alle Termine in eine CSV-Datei.
         */
        public void SaveAppointments()
        {
            var allAppointments = calendar.GetAllAppointments();
            CsvHelper.SaveAppointments(allAppointments);
        }
    }
}