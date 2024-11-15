using System.IO;

namespace TerminKalender.Models
{
    /**
     * Utility-Klasse zur Verwaltung von CSV-Daten.
     * Ermöglicht das Laden und Speichern von Terminen in einer CSV-Datei.
     */
    public static class CsvHelper
    {
        private static readonly string FilePath = "appointments.csv"; // Standardpfad zur CSV-Datei

        /**
         * Lädt die Termine aus einer CSV-Datei.
         * 
         * @return Eine Liste der geladenen Termine. Gibt eine leere Liste zurück, wenn die Datei nicht existiert.
         */
        public static List<Appointment> LoadAppointments()
        {
            var appointments = new List<Appointment>();

            // Prüfen, ob die Datei existiert
            if (!File.Exists(FilePath))
                return appointments; // Leere Liste zurückgeben, falls die Datei nicht vorhanden ist

            // Jede Zeile der Datei einlesen
            foreach (var line in File.ReadLines(FilePath))
            {
                var values = line.Split(','); // Spalten durch Kommas trennen

                // Prüfen, ob die Zeile gültig ist und die Daten in ein Appointment-Objekt umwandeln
                if (values.Length == 3 && DateTime.TryParse(values[1], out var startDate) && DateTime.TryParse(values[2], out var endDate))
                {
                    appointments.Add(new Appointment(values[0], startDate, endDate));
                }
            }

            return appointments;
        }

        /**
         * Speichert eine Liste von Terminen in einer CSV-Datei.
         * 
         * @param appointments Die Liste der zu speichernden Termine.
         */
        public static void SaveAppointments(List<Appointment> appointments)
        {
            using (var writer = new StreamWriter(FilePath))
            {
                // Jede Zeile der Datei schreiben
                foreach (var appointment in appointments)
                {
                    // Termin in CSV-Format konvertieren
                    var line = $"{appointment.Title},{appointment.StartDate},{appointment.EndDate}";
                    writer.WriteLine(line); // Zeile in die Datei schreiben
                }
            }
        }
    }
}