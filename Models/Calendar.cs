namespace TerminKalender.Models
{
    /**
     * Klasse zur Verwaltung von Terminen.
     * Bietet Funktionen zum Hinzufügen, Löschen, Bearbeiten und Abrufen von Terminen.
     */
    public class Calendar
    {
        // Liste, die alle Termine enthält
        private readonly List<Appointment> appointments = new();

        /**
         * Fügt einen neuen Termin hinzu, wenn keine Überschneidungen mit bestehenden Terminen vorliegen.
         *
         * @param newAppointment Der hinzuzufügende Termin.
         * @return true, wenn der Termin erfolgreich hinzugefügt wurde; andernfalls false.
         */
        public bool AddAppointment(Appointment newAppointment)
        {
            // Prüfen, ob der neue Termin sich mit bestehenden Terminen überschneidet
            if (!appointments.Any(a => (newAppointment.StartDate < a.EndDate) && (newAppointment.EndDate > a.StartDate)))
            {
                appointments.Add(newAppointment);
                return true;
            }
            return false;
        }

        /**
         * Gibt alle Termine für einen bestimmten Tag zurück.
         *
         * @param date Das Datum, für das die Termine abgerufen werden sollen.
         * @return Eine Liste der Termine für das angegebene Datum, sortiert nach Startzeit.
         */
        public List<Appointment> GetAppointmentsForDay(DateTime date)
        {
            return appointments
                .Where(a => a.StartDate.Date == date.Date) // Filtere Termine nach Datum
                .OrderBy(a => a.StartDate) // Sortiere nach Startzeit
                .ToList();
        }

        /**
         * Löscht einen bestimmten Termin aus der Liste.
         *
         * @param appointment Der zu löschende Termin.
         */
        public void DeleteAppointment(Appointment appointment)
        {
            appointments.Remove(appointment);
        }

        /**
         * Aktualisiert einen bestehenden Termin.
         * Der alte Termin wird entfernt und der neue hinzugefügt.
         *
         * @param oldAppointment Der zu aktualisierende Termin.
         * @param newAppointment Der neue Termin, der den alten ersetzt.
         */
        public void UpdateAppointment(Appointment oldAppointment, Appointment newAppointment)
        {
            DeleteAppointment(oldAppointment); // Alten Termin entfernen
            appointments.Add(newAppointment);  // Neuen Termin hinzufügen
        }

        /**
         * Gibt alle gespeicherten Termine zurück.
         *
         * @return Eine neue Liste mit allen Terminen.
         */
        public List<Appointment> GetAllAppointments()
        {
            // Rückgabe einer Kopie der Liste, um die interne Liste zu schützen
            return new List<Appointment>(appointments);
        }
    }
}