// Dans CarRental.Desktop2.BackOffice/MainForm.cs (Fichier de Logique)

using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Desktop2.BackOffice
{
    // C'est ici que vous ajoutez ou modifiez le constructeur
    public partial class MainForm : Form
    {
        // 1. Déclarez le champ pour stocker le Service Provider
        private readonly IServiceProvider _serviceProvider;

        // 2. MODIFIEZ le constructeur par défaut pour accepter l'injection
        // (Si vous aviez un constructeur sans paramètre, remplacez-le par celui-ci)
        public MainForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            this.Text = "Back Office - Menu Principal";

            // Attachez l'événement au bouton créé dans le designer
            // Assurez-vous que le nom du bouton est 'btnManageTariffs'
            this.btnManageTariffs.Click += BtnManageTariffs_Click;
        }

        // 3. Ajoutez la méthode pour gérer le clic sur le bouton
        private void BtnManageTariffs_Click(object sender, EventArgs e)
        {
            // Ouvre la Forme de gestion des Tarifs en utilisant l'injection de dépendances
            using (var form = _serviceProvider.GetRequiredService<TariffManagementForm>())
            {
                form.ShowDialog();
            }
        }

        // Le reste de votre code (si vous en avez) suit ici.
    }
}