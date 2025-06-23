using AcervoPessoal.Services;
using AcervoPessoal.Models;

namespace AcervoPessoal
{
    public partial class MainPage : ContentPage
    {

        private DatabaseService _databaseService;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _ = TestarConexao();
        }

        private async Task TestarConexao()
        {
            var conectado = await _databaseService.TestConnection();
            StatusLabel.Text = conectado ? "✅ Conectado ao banco" : "❌ Erro na conexão";
            StatusLabel.TextColor = conectado ? Colors.Green : Colors.Red;
        }

        private async void OnCarregarClicked(object sender, EventArgs e)
        {
            CarregarButton.IsEnabled = false;
            var livros = await _databaseService.ObterLivros();
            LivrosCollectionView.ItemsSource = livros;
            CarregarButton.IsEnabled = true;
        }

        private async void OnAdicionarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AdicionarLivroPage());
        }
    }
}



namespace BibliotecaPessoal
{
    public partial class MainPage : ContentPage
    {
        
    }
}
