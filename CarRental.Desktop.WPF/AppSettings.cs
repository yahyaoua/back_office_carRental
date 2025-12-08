using System;
using System.IO; // <-- NOUVEAU : Corrige l'erreur CS0103 sur 'Path'

namespace CarRental.Desktop.WPF
{
    // C'est la seule définition de cette classe dans tout le projet WPF
    public static class AppSettings
    {
        public static string BaseImagePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CarRental2_Images"
            );
    }
}