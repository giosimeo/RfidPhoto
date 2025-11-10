using Microsoft.Data.SqlClient;
using RfidPhoto.Interface;
using RfidPhoto.Models;
using System.Data;

namespace RfidPhoto.Service
{
    public class GestDBService : IGestDBInterface
    {
        private readonly string connString;
        public GestDBService(IConfiguration configuration)
        {
            connString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection string not found.");
        }


        public async Task<Imballo> GetImballoAttivo(string readerId)
        {
            var conn = new SqlConnection(connString);
            string imballo = "";
            Imballo imbObj = new Imballo(); 
            int conta = 1;  
            await conn.OpenAsync();
            try
            {
                while (imballo == "" || conta < 10)
                {
                    string q = @"
                    SELECT MIN(RFID) AS RFID
                    FROM dbo.V_RILEVAMENTI_RFID_VALIDI
                    WHERE RILEVATORE_RFID = @reader;";
                    SqlCommand cmd = new SqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@reader", readerId);
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        string rfid = reader["RFID"].ToString();
                        reader.Close();
                        if (rfid != "")
                        {
                            string q1 = "SELECT I.ID_IMBALLO FROM dbo.V_IMBALLI_ATTIVI I WHERE I.RFID = @rfid;";
                            SqlCommand c1 = new SqlCommand(q1, conn);
                            c1.Parameters.AddWithValue("@rfid", rfid);
                            SqlDataReader rdImb = await c1.ExecuteReaderAsync();
                            if (rdImb.HasRows)
                            {
                                rdImb.Read();
                                string idImb = rdImb["ID_IMBALLO"].ToString();
                                rdImb.Close();

                                string q2 = @"
                                    SELECT
                                        p.DATA_ORA_REGISTRAZIONE AS DATA_ORA_REGISTRAZIONE,
                                        p.CODICE_CLIENTE AS CODICE_CLIENTE,
                                        p.RAGIONE_SOCIALE AS RAGIONE_SOCIALE,
                                        p.NUMERO_FORMULARIO AS NUMERO_FORMULARIO,
                                        p.DATA_FORMULARIO AS DATA_FORMULARIO,
                                        p.QUANTITA_FORMULARIO AS QUANTITA_FORMULARIO,
                                        i.MATERIALE AS LAVORAZIONE,
                                        CONCAT(l.SCHEDA_AFFINAZIONE,'.',l.NUMERO_PROTOCOLLO) AS PROTOCOLLO,
                                        I.IMMAGINE1, I.IMMAGINE2
                                    FROM dbo.PROTOCOLLI p
                                    JOIN dbo.LOTTI l ON p.NUMERO_PROTOCOLLO = l.NUMERO_PROTOCOLLO AND p.SCHEDA_AFFINAZIONE = l.SCHEDA_AFFINAZIONE
                                    JOIN dbo.IMBALLI i ON l.ID_LOTTO = i.ID_LOTTO
                                    WHERE i.ID_IMBALLO = @id;";
                                SqlCommand c2 = new SqlCommand(q2, conn);
                                c2.Parameters.AddWithValue("@id", idImb);
                                SqlDataReader rdImballo = await c2.ExecuteReaderAsync();
                                if (rdImballo.HasRows)
                                {
                                    rdImballo.Read();
                                    imballo = idImb;
                                    imbObj = new Imballo
                                    {
                                        IDIMBALLO = idImb,
                                        DATA_ORA_REGISTRAZIONE = (DateTime)rdImballo["DATA_ORA_REGISTRAZIONE"],
                                        CODICE_CLIENTE = rdImballo["CODICE_CLIENTE"].ToString(),
                                        RAGIONE_SOCIALE = rdImballo["RAGIONE_SOCIALE"].ToString(),
                                        NUMERO_FORMULARIO = rdImballo["NUMERO_FORMULARIO"].ToString(),
                                        DATA_FORMULARIO = (DateTime)rdImballo["DATA_FORMULARIO"],
                                        QUANTITA_FORMULARIO = (double)rdImballo["QUANTITA_FORMULARIO"],
                                        LAVORAZIONE = rdImballo["LAVORAZIONE"].ToString(),
                                        PROTOCOLLO = rdImballo["PROTOCOLLO"].ToString(),
                                        IMMAGINE1 = rdImballo["IMMAGINE1"] == DBNull.Value ? null : (byte[])rdImballo["IMMAGINE1"],
                                        IMMAGINE2 = rdImballo["IMMAGINE2"] == DBNull.Value ? null : (byte[])rdImballo["IMMAGINE2"],
                                    };
                                    rdImballo.Close();
                                }
                                break;
                            }
                        }
                    } 
                    conta ++;
                }
                return imbObj;
                
            }
            catch (Exception)
            {

                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }

        }

        public async Task<List<Readers>> ListReaders()
        {
            var conn = new SqlConnection(connString);
            await conn.OpenAsync();
            try
            {
                string q = @"
                    SELECT L.CODICE_RISORSA, R.DESCRIZIONE, L.CODICE_LOCAZIONE, L.STATO, L.RILEVATORE_RFID
                    FROM dbo.LOCAZIONI L
                    JOIN dbo.RISORSE R ON R.CODICE_RISORSA = L.CODICE_RISORSA
                    WHERE L.RILEVATORE_RFID IS NOT NULL order by R.DESCRIZIONE;";
                SqlCommand cmd = new SqlCommand(q, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                List<Readers> readers = new List<Readers>();
                while (reader.Read())
                {
                    Readers read = new Readers
                    {
                        CODICE_RISORSA = reader["CODICE_RISORSA"].ToString(),
                        DESCRIZIONE = reader["DESCRIZIONE"].ToString(),
                        CODICE_LOCAZIONE = reader["CODICE_LOCAZIONE"].ToString(),
                        STATO = reader["STATO"].ToString(),
                        RILEVATORE_RFID = reader["RILEVATORE_RFID"].ToString()
                    };
                    readers.Add(read);
                }
                return readers;
            }
            catch (Exception e)
            {
                return null;

            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<int> SaveImage(string id, byte[] imageData, int nImage)
        {
            var conn = new SqlConnection(connString);
            await conn.OpenAsync();
            try
            {
                string col = nImage == 1 ? "IMMAGINE1" : "IMMAGINE2";
                SqlCommand cmd = new SqlCommand($"UPDATE dbo.IMBALLI SET {col}=@img WHERE ID_IMBALLO=@id;", conn);
                cmd.Parameters.Add("@img", SqlDbType.VarBinary, -1).Value = imageData;
                cmd.Parameters.AddWithValue("@id", id);
                int n = await cmd.ExecuteNonQueryAsync();
                return n;
            }
            catch (Exception e)
            {
                string txt;
                txt = DateTime.Now + $" ERRORE: {e.Message}";
                //WriteLog(txt);
                return -1;

            }
            finally
            {
                await conn.CloseAsync();
            }

        }

        public async Task<byte[]> GetImageImballo(string id, int nImage)
        {
            var conn = new SqlConnection(connString);
            await conn.OpenAsync();
            try
            {
                string col = nImage == 1 ? "IMMAGINE1" : "IMMAGINE2";

                string q = $"SELECT {col} FROM dbo.IMBALLI WHERE ID_IMBALLO=@id;";
                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    return (byte[])reader[col];
                }
                else
                {
                    return null;
                }
                    
            }
            catch (Exception e)
            {
                return null;

            }
            finally
            {
                await conn.CloseAsync();
            }

        }
    }
}
