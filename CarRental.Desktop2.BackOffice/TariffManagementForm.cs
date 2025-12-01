// Dans CarRental.Desktop2.BackOffice/TariffManagementForm.cs

using CarRental2.Core.Interfaces;
using System;
using System.Linq;
using System.Windows.Forms;

namespace CarRental.Desktop2.BackOffice // Utilisez le namespace de votre projet (Desktop2.BackOffice)
{
    public partial class TariffManagementForm : Form
    {
        private readonly IUnitOfWork _unitOfWork;

        // Le IUnitOfWork est injecté ici, prouvant que la DI fonctionne !
        public TariffManagementForm(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            this.Text = "Gestion des Tarifs (Test DI)";

            // Attachez la méthode de test au bouton créé
            this.btnLoadTest.Click += TestButton_Click;
        }

        private async void TestButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Tente de récupérer les données via IUnitOfWork.Tariffs
                var tariffs = await _unitOfWork.Tariffs.GetAllAsync();

                MessageBox.Show($"Connexion réussie ! Nombre de tarifs trouvés : {tariffs.Count()}", "Test de Connexion DB réussi");

                // Affiche les résultats dans la DataGridView (même si la table est vide, la connexion sera validée)
                tariffDataGridView.DataSource = tariffs.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de connexion/chargement : {ex.Message}\nVérifiez la chaîne de connexion dans Program.cs et que la DB est en ligne.", "Erreur Critique");
            }
        }

        private void tariffDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void lblTariffName_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void TariffManagementForm_Load(object sender, EventArgs e)
        {

        }
    }
}