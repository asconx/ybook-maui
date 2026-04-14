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
        if (sender is VisualElement el && el.BindingContext is Pokoj item)
        {
            await Shell.Current.GoToAsync(nameof(PokojEdycjaPage), new Dictionary<string, object>
            {
                { "Pokoj", item }
            });
        }
    }

    // Handler for the delete (🗑) tap inside the CollectionView item template
    private async void OnDeleteTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement el && el.BindingContext is Pokoj item)
        {
            // 1. Pytamy o potwierdzenie
            bool confirm = await DisplayAlert("Usuń", $"Czy na pewno usunąć {item.Nazwa}?", "Tak", "Nie");
            if (!confirm) return;

            try
            {
                // 2. Wywołujemy API do usunięcia pokoju
                bool success = await _panelService.DeletePokoj(item.Id);

                if (success)
                {
                    // 3. Usuwamy pokój z listy widocznej na ekranie
                    pokoje.Remove(item);
                }
                else
                {
                    await this.DisplayAlert("Błąd", "Serwer odrzucił żądanie usunięcia.", "OK");
                }
            }
            catch (Exception ex)
            {
                await this.DisplayAlert("Błąd połączenia", ex.Message, "OK");
            }
        }
    }
}
