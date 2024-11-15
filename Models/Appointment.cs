namespace TerminKalender.Models
{
    /**
     * Repräsentiert einen Termin mit einem Titel, Startzeit und Endzeit.
     */
    public class Appointment
    {
        /** 
         * Der Titel des Termins.
         */
        public string Title { get; set; }

        /**
         * Das Startdatum und die Startzeit des Termins.
         */
        public DateTime StartDate { get; set; }

        /**
         * Das Enddatum und die Endzeit des Termins.
         */
        public DateTime EndDate { get; set; }

        /**
         * Liefert den Zeitbereich des Termins im Format "HH:mm - HH:mm".
         */
        public string TimeRange => $"{StartDate:HH:mm} - {EndDate:HH:mm}";

        /**
         * Konstruktor, um einen neuen Termin zu erstellen.
         *
         * @param title Der Titel des Termins.
         * @param startDate Das Startdatum und die Startzeit des Termins.
         * @param endDate Das Enddatum und die Endzeit des Termins.
         */
        public Appointment(string title, DateTime startDate, DateTime endDate)
        {
            Title = title;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}