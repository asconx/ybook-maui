using System;
using System.Collections;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using yBook.Services;
using yBook.Models;

namespace yBook.Views.Ustawienia;

public partial class PokojePage
{
    // Handler for the "dodaj kwaterę" button
    private async void OnAddRoomClicked(object sender, EventArgs e)
    {
        var edycjaPage = new PokojEdycjaPage();
        await Navigation.PushAsync(edycjaPage);
    }

    // Handler for the edit (✎) tap inside the CollectionView item template
    private async void OnZmienSzczegolyTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement el)
        {
            var item = el.BindingContext as Pokoj;
            if (item == null) return;

            var edycjaPage = new PokojEdycjaPage(item);
            await Navigation.PushAsync(edycjaPage);
        }
    }

    // Handler for the delete (🗑) tap inside the CollectionView item template
    private async void OnDeleteTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement el)
        {
            var item = el.BindingContext;
            if (item == null)
            {
                await this.DisplayAlert("Błąd", "Nie można znaleźć elementu.", "OK");
                return;
            }

            bool confirm = await this.DisplayAlert("Usuń", "Czy na pewno usunąć ten element?", "Tak", "Nie");
            if (!confirm)
                return;

            if (PokojList?.ItemsSource is IList list)
            {
                list.Remove(item);
            }
            else
            {
                // If ItemsSource is not an IList, just notify for now
                await this.DisplayAlert("Usunięto", "Element został usunięty (jeśli to możliwe).", "OK");
            }
        }
    }
}
