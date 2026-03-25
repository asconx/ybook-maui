using System;
using Microsoft.Maui.Controls;
using yBook.Services;
using yBook.Models;
using yBook.Views.Uzytkownicy;

namespace yBook.Views.Uzytkownicy
{
    public partial class Uzytkownicy1Page : ContentPage
    {
        public Uzytkownicy1Page()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UsersList.ItemsSource = UserStore.Users;
            UpdateEmpty();
            UserStore.Users.CollectionChanged += Users_CollectionChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            UserStore.Users.CollectionChanged -= Users_CollectionChanged;
        }

        void Users_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateEmpty();
        }

        void UpdateEmpty()
        {
            EmptyLabel.IsVisible = UserStore.Users.Count == 0;
        }

        // Bezpoœrednie otwarcie formularza jako modal — niezawodne
        async void OnInviteClicked(object sender, EventArgs e)
        {
            var page = new UzytkownicyPage();
            await Navigation.PushModalAsync(page);
        }
    }
}