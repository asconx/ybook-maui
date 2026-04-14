namespace yBook.Views.Ceny;

public partial class UslugiOplatyDodawanie : ContentPage
{
	private List<string> _ceny = new();
	private Usluga _usluga;
	private Action _refresh;

	public UslugiOplatyDodawanie(Usluga usluga, bool modif, Action refreshAction)
	{
		InitializeComponent();


		_usluga = usluga;
		_refresh = refreshAction;

		if (modif)
			SetFieldsFromUsluga(usluga);
	}

	void SetFieldsFromUsluga(Usluga usluga)
	{
		NameEntry.Text = usluga.Name;

		RodzajPicker.SelectedItem = usluga.Rodzaj;
		TypPicker.SelectedItem = usluga.Typ;

		_ceny = usluga.Ceny?.ToList() ?? new List<string>();
		RefreshCenyUI();

		if (DateTime.TryParse(usluga.DataOd, out var od))
			DataOdPicker.Date = od;

		if (DateTime.TryParse(usluga.DataDo, out var doo))
			DataDoPicker.Date = doo;

		OpisEditor.Text = usluga.Opis;
	}

	async void OnSaveClicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(NameEntry.Text))
		{
			await DisplayAlert("B³¹d", "Nazwa jest wymagana", "OK");
			return;
		}

		// ---------------------------
		// MAPOWANIE UI -> MODEL
		// ---------------------------
		_usluga.Name = NameEntry.Text;

		_usluga.Rodzaj = RodzajPicker.SelectedItem?.ToString() ?? _usluga.Rodzaj;
		_usluga.Typ = TypPicker.SelectedItem?.ToString() ?? _usluga.Typ;

		_usluga.Ceny = _ceny.ToList();

		_usluga.DataOd = NoDataOdSwitch.IsToggled ? null : DataOdPicker?.Date?.ToString("yyyy-MM-dd") ?? "";
		_usluga.DataDo = NoDataDoSwitch.IsToggled ? null : DataDoPicker?.Date?.ToString("yyyy-MM-dd") ?? "";

		_usluga.Opis = OpisEditor.Text;

		// ---------------------------
		// REFRESH LISTY
		// ---------------------------
		_refresh?.Invoke();

		await Shell.Current.GoToAsync("..");
	}

	async void OnCancelClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}

	void RefreshCenyUI()
	{
		CenyContainer.Children.Clear();

		foreach (var cena in _ceny)
		{
			var row = new HorizontalStackLayout
			{
				Spacing = 10
			};

			var label = new Label
			{
				Text = cena,
				VerticalOptions = LayoutOptions.Center
			};

			var deleteBtn = new Button
			{
				Text = "Usuñ",
				BackgroundColor = Colors.Red,
				TextColor = Colors.White
			};

			deleteBtn.Clicked += (s, e) =>
			{
				_ceny.Remove(cena);
				RefreshCenyUI();
			};

			row.Children.Add(label);
			row.Children.Add(deleteBtn);

			CenyContainer.Children.Add(row);
		}
	}

	private void OnNoDataOdToggled(object sender, ToggledEventArgs e)
	{
		DataOdPicker.IsEnabled = !e.Value;
	}

	private void OnNoDataDoToggled(object sender, ToggledEventArgs e)
	{
		DataDoPicker.IsEnabled = !e.Value;
	}

	async void OnAddCenaClicked(object sender, EventArgs e)
	{
		string cena = await DisplayPromptAsync("Cena", "Podaj wartoœæ ceny:");

		if (!string.IsNullOrWhiteSpace(cena))
		{
			_ceny.Add(cena);
			RefreshCenyUI();
		}
	}
}