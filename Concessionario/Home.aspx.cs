using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Concessionario
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CaricaAuto();
                CaricaOptional();
                //CaricaGaranzia();
            }

        }

        private void CaricaAuto()
        {
            string Optional = ConfigurationManager.ConnectionStrings["Concessionario"].ConnectionString.ToString();
            SqlConnection conn = new SqlConnection(Optional);
            try
            {
                conn.Open();
                string query = "SELECT ID, NomeModello, PrezzoBase, PercorsoImmagine FROM Auto";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                ddlAuto.Items.Clear();
                ddlAuto.Items.Add(new ListItem("Seleziona un'auto", ""));

                while (reader.Read())
                {
                    string nomeModello = reader["NomeModello"].ToString();
                    string id = reader["ID"].ToString();
                    ListItem li = new ListItem(nomeModello, id);
                    ddlAuto.Items.Add(li);
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void CaricaOptional()
        {
            string Optional = ConfigurationManager.ConnectionStrings["Concessionario"].ConnectionString.ToString();
            SqlConnection conn = new SqlConnection(Optional);

            try
            {
                conn.Open();
                string query = "SELECT ID, Descrizione, Prezzo FROM Optional";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                //
                cblOptional.Items.Clear();
                while (reader.Read())
                {
                    string id = reader["ID"].ToString();
                    string descrizione = reader["Descrizione"].ToString();
                    ListItem item = new ListItem(descrizione, id);

                    cblOptional.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }

        protected void ddlAuto_SelectedIndexChanged(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Concessionario"].ConnectionString;

            // utilizzo using per aprire e chiudere la connessione automaticamente
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT PercorsoImmagine FROM Auto WHERE ID = @AutoId";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@AutoId", ddlAuto.SelectedValue);

                    var percorsoImmagine = cmd.ExecuteScalar() as string;
                    imgAuto.ImageUrl = percorsoImmagine;
                }
                catch (Exception ex)
                {
                    Response.Write(ex.Message);
                }

            }
        }


        protected void btnCalcola_Click(object sender, EventArgs e)
        {
            decimal prezzoBase = 0, totaleOptional = 0;
            string descOptional = "Optional scelti: <br />";
            bool optionalSelezionato = false;

            // recupero prezzo base auto 
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Concessionario"].ConnectionString))
            {
                conn.Open();

                // prezzo base auto
                using (SqlCommand cmdPrezzoBase = new SqlCommand("SELECT PrezzoBase FROM Auto WHERE ID = @AutoId", conn))
                {
                    cmdPrezzoBase.Parameters.AddWithValue("@AutoId", ddlAuto.SelectedValue);
                    prezzoBase = (decimal)cmdPrezzoBase.ExecuteScalar();
                }

                // prezzo  optional selezionati
                foreach (ListItem item in cblOptional.Items)
                {
                    if (item.Selected)
                    {
                        optionalSelezionato = true;
                        using (SqlCommand cmdPrezzoOptional = new SqlCommand("SELECT Prezzo FROM Optional WHERE ID = @OptionalId", conn))
                        {
                            cmdPrezzoOptional.Parameters.AddWithValue("@OptionalId", item.Value);
                            decimal prezzoOptional = (decimal)cmdPrezzoOptional.ExecuteScalar();
                            totaleOptional += prezzoOptional;
                            descOptional += $"{item.Text} - €{prezzoOptional}<br />";
                        }
                    }
                }
            }

            // Assemblaggio e visualizzazione del risultato
            if (!optionalSelezionato) descOptional = "Nessun optional selezionato.<br />";
            decimal prezzoTotale = prezzoBase + totaleOptional;
            ltlRisultato.Text = $"<div class='text-center my-4'><h4 class='fw-bolder mb-3'>Preventivo Auto</h4>" +
                                $"<p class='fs-4 fw-bold'>Prezzo base dell'auto selezionata: €{prezzoBase}</p>{descOptional}" +
                                $"<p class='fs-4 fw-bold'>Prezzo totale degli optional: €{totaleOptional}</p>" +
                                $"<p class='fs-4 fw-bold'>Prezzo totale: €{prezzoTotale}</p></div>";
        }
    }
}