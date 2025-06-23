using AcervoPessoal.Models;
using AcervoPessoal.Services;

namespace AcervoPessoal;

public partial class AdicionarLivroPage : ContentPage
{
    private DatabaseService _databaseService;
    private List<Categoria> _categorias;

    public AdicionarLivroPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
        _ = CarregarCategorias();
    }

    private async Task CarregarCategorias()
    {
        var categorias = await _databaseService.ObterCategorias();
        _categorias = categorias.ToList();
        CategoriaPicker.ItemsSource = _categorias.Select(c => c.Nome).ToList();
    }

    private void OnAvaliacaoChanged(object sender, ValueChangedEventArgs e)
    {
        int avaliacao = (int)Math.Round(e.NewValue);
        AvaliacaoSlider.Value = avaliacao;
        AvaliacaoLabel.Text = $"{avaliacao} estrela{(avaliacao != 1 ? "s" : "")}";
    }

    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TituloEntry.Text) || string.IsNullOrWhiteSpace(AutorEntry.Text))
        {
            await DisplayAlert("Erro", "Título e Autor são obrigatórios!", "OK");
            return;
        }

        SalvarButton.IsEnabled = false;

        var livro = new Livro
        {
            Titulo = TituloEntry.Text,
            Autor = AutorEntry.Text,
            ISBN = string.IsNullOrWhiteSpace(ISBNEntry.Text) ? null : ISBNEntry.Text,
            Editora = string.IsNullOrWhiteSpace(EditoraEntry.Text) ? null : EditoraEntry.Text,
            AnoPulicacao = int.TryParse(AnoEntry.Text, out int ano) ? ano : null,
            Paginas = int.TryParse(PaginasEntry.Text, out int paginas) ? paginas : null,
            CategoriaId = CategoriaPicker.SelectedIndex >= 0 ? _categorias[CategoriaPicker.SelectedIndex].Id : null,
            StatusLeitura = StatusPicker.SelectedItem?.ToString() ?? "Não lido",
            Avaliacao = (int)AvaliacaoSlider.Value
        };

        var sucesso = await _databaseService.InserirLivro(livro);

        if (sucesso)
        {
            await DisplayAlert("Sucesso", "Livro adicionado com sucesso!", "OK");
            await Navigation.PopAsync();
        }

        SalvarButton.IsEnabled = true;
    }
}