namespace RfidPhoto.Models
{
    public class Imballo
    {
        public string IDIMBALLO { get; set; }
        public DateTime DATA_ORA_REGISTRAZIONE { get; set; }
        public string CODICE_CLIENTE { get; set; }
        public string RAGIONE_SOCIALE { get; set; }
        public string NUMERO_FORMULARIO { get; set; }
        public DateTime DATA_FORMULARIO { get; set; }
        public double QUANTITA_FORMULARIO { get; set; }
        public string LAVORAZIONE { get; set; }
        public string PROTOCOLLO { get; set; }
        public byte[]? IMMAGINE1 { get; set; }
        public byte[]? IMMAGINE2 { get; set; }

    }
}