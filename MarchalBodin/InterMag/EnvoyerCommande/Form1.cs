using System;
using System.Diagnostics;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Objets100cLib;
using System.Runtime.InteropServices;
using MBCore.Model;

namespace EnvoyerCommande
{
    public partial class Form1 : Form
    {
        private int typeCde;

        private string doPiece;
        private DataSet dsCde = new DataSet();
        private InterMagRepository InterMagRepos;

        // Pour mettre au premier plan en continue
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
#if !DEBUG
            // TopMost = true; fonctionne pas :(
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
#endif
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length < 3)
                {
                    throw new Exception("Nombre d'argument insuffisant");
                }

                doPiece = args[3];

                BaseCialAbstract.setDefaultParams(args[1], args[2]);
                InterMagRepos = new InterMagRepository();
                InterMagRepos.Log += new InterMagRepository.LogEventHandler(Log);

                Text = label1.Text = "Envoi de la commande " + doPiece;

                DbSelector.DataSource = InterMagRepos.getDbListFromContext().Where(d=>d.name != InterMagRepos.dbName && d.isMag == true).ToList();
                DbSelector.DisplayMember = "name";
                DbSelector.ValueMember = "name";

                // Récupère la commande
                Log("Vérification de la commande...");
                dsCde = InterMagRepos.getCommande(doPiece);
                string message = InterMagRepos.valideCommande(dsCde.Tables[0]);

                if (message != String.Empty)
                {
                    Log($"{InterMagRepos.dbName}::Erreur Doc {doPiece} : {message}");
                }
                else
                {
                    Log("Commande OK.");
                    EnvoyerBtn.Enabled = true;
                    DbSelector.Enabled = true;

                    typeCde = InterMagRepos.getTypeCde(dsCde);
                    if (typeCde == InterMagRepository.TYPE_DEPOT)
                    {
                        // Auto selectionne le magasin cible sur une commande dépot
                        DbSelector.SelectedIndex = DbSelector.FindString((string) dsCde.Tables[0].Rows[0]["CT_Classement"]);
                        DbSelector.Enabled = false;
                    }
                    typeCdelabel.Text = InterMagRepos.typeCdeLabels[typeCde];

                    string MagRef = (string)dsCde.Tables[0].Rows[0]["magasin_referent"];
                    MagReflabel.Text = "";
                    if (typeCde == InterMagRepository.TYPE_RETRO && MagRef != "")
                    {
                        // Auto selectionne le magasin cible sur une commande rétro
                        MagReflabel.Text = (string)dsCde.Tables[0].Rows[0]["magasin_referent"];
#if DEBUG
                        MagRef += "DEV";
#endif
                        DbSelector.SelectedIndex = DbSelector.FindString(MagRef);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckDocumentIsClosed()
        {
            try
            {
                InterMagRepos.DocumentIsClosed(doPiece);
            }
            catch (Exception)
            {
                if (MessageBox.Show(
                "Veuillez fermer le document puis cliquer sur OK",
                "Fermer le doc",
                MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    CheckDocumentIsClosed();
                }
                else
                {
                    throw new Exception("Action annulée par l'utilisateur.");
                }
            }
        }

        private void Log(string message)
        {
            Debug.Print(message);
            textBox1.Text += message + Environment.NewLine;
            Application.DoEvents();
        }
        
        private void EnvoyerBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Log($"Envoi de la commande {doPiece} ...");
                CheckDocumentIsClosed();
                Debug.Print(DbSelector.SelectedIndex.ToString());

                string targetDb = DbSelector.SelectedValue.ToString();

                if (typeCde == InterMagRepository.TYPE_DEPOT)
                {
                    targetDb = dsCde.Tables[0].Rows[0]["CT_Classement"].ToString();
                }

                InterMagRepos.sendSqlCde(dsCde, targetDb);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", ex.ToString(), EventLogEntryType.Error);
                Log(ex.Message);
            }
            finally
            {
                DbSelector.Enabled = false;
                EnvoyerBtn.Enabled = false;
                dsCde.Dispose();
                InterMagRepos.closeBaseCial();
                Log("Vous pouvez fermer cette fenêtre.");
            }
        }
    }
}
