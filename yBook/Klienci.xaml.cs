using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace yBook.Views.Klienci
{
    public partial class KlienciPage : ContentPage
    {
        public KlienciPage()
        {
            InitializeComponent();
            RefreshClientList();
        }

        void OnAddClientClicked(object sender, EventArgs e)
        {
            ListaSection.IsVisible = false;
            FormSection.IsVisible = true;
        }

        void OnMoreDataChecked(object sender, CheckedChangedEventArgs e)
        {
            MoreDataSection.IsVisible = e.Value;

            MoreDataLabel.Text = e.Value
                ? "Pokaż mniej"
                : "Więcej danych klienta";
        }

        void OnMoreDataLabelTapped(object sender, EventArgs e)
        {
            CbWiecejDanych.IsChecked = !CbWiecejDanych.IsChecked;
        }

        void OnSaveClicked(object sender, EventArgs e)
        {
            var client = new Client
            {
                Name = NazwaEntry.Text,
                Email = EmailEntry.Text,
                Phone = TelefonEntry.Text,
                Nip = NipEntry.Text,
                Notes = UwagiEntry.Text,
                Discount = int.TryParse(RabatEntry.Text, out var r) ? r : 0,
                IsRegularClient = RbStalyKlient.IsChecked
            };

            ClientStore.Add(client);

            ClearForm();

            FormSection.IsVisible = false;
            ListaSection.IsVisible = true;

            RefreshClientList();
        }

        void OnCancelClicked(object sender, EventArgs e)
        {
            FormSection.IsVisible = false;
            ListaSection.IsVisible = true;
        }

        void RefreshClientList()
        {
            ClientsList.ItemsSource = null;
            ClientsList.ItemsSource = ClientStore.Clients;
        }

        void ClearForm()
        {
            NazwaEntry.Text = "";
            EmailEntry.Text = "";
            TelefonEntry.Text = "";
            RabatEntry.Text = "0";
            NipEntry.Text = "";
            UwagiEntry.Text = "";

            CbWiecejDanych.IsChecked = false;
            MoreDataSection.IsVisible = false;
            MoreDataLabel.Text = "Więcej danych klienta";
        }
    }

    // MODEL
    public class Client
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Nip { get; set; }
        public string Notes { get; set; }
        public int Discount { get; set; }

        public bool IsRegularClient { get; set; }

        public string TypeText => IsRegularClient ? "Stały" : "Niechciany";
    }

    // STORE
    public static class ClientStore
    {
        public static List<Client> Clients { get; set; } = new List<Client>();

        public static void Add(Client client)
        {
            Clients.Add(client);
        }
    }
}