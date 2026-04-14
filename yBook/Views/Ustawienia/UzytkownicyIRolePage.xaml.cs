using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using yBook.Models;

namespace yBook.Views.Ustawienia
{
    public partial class UzytkownicyIRolePage : ContentPage
    {
        public UzytkownicyIRolePage()
        {
            InitializeComponent();

            // Bind user

            // Simple static roles list for now
            RolesCollection.ItemsSource = new List<string> { "Administrator", "Manager", "Pracownik" };
        }

        async void OnAddRoleClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Dodaj rolę", "Nazwa roli:");
            if (string.IsNullOrWhiteSpace(result)) return;

            var list = RolesCollection.ItemsSource as List<string>;
            if (list == null) list = new List<string>();
            list.Add(result.Trim());
            RolesCollection.ItemsSource = null;
            RolesCollection.ItemsSource = list;
        }
    }
}