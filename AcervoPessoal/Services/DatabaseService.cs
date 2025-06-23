using AcervoPessoal.Configuration;
using AcervoPessoal.Models;
using DotNetEnv;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcervoPessoal.Services
{
    internal class DatabaseService
    {

        private string connectionString;
        //"server=10.0.2.2;" +
        //"uid=root;" +
        //"pwd=root;" +
        //"database=acervo_pessoal";

        private bool isInitialized = false;

        public string ConnectionString => connectionString;

        public async Task InitializeAsync()
        {
            if (!isInitialized)
            {
                await EnvLoader.LoadAsync();
                connectionString = BuildConnectionString();
                isInitialized = true;

                // Debug: Mostrar valores carregados
                System.Diagnostics.Debug.WriteLine($"🔌 Connection String: {GetSafeConnectionString()}");
            }
        }

        public DatabaseService()
        {
            // Carregar variáveis do arquivo .env
            Env.Load();

            // Construir connection string a partir das variáveis de ambiente
            connectionString = BuildConnectionString();
        }

        private string BuildConnectionString()
        {
            var server = Environment.GetEnvironmentVariable("DB_SERVER");
            var database = Environment.GetEnvironmentVariable("DB_DATABASE");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var port = Environment.GetEnvironmentVariable("DB_PORT");

            // Se as variáveis não foram carregadas, usar valores padrão
            if (string.IsNullOrEmpty(server))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Variáveis de ambiente não encontradas. Usando valores padrão.");

                // Valores padrão para desenvolvimento
                server = "10.0.2.2";
                database = "acervo_pessoal";
                user = "root";
                password = "root";
                port = "3306";
            }

            return $"server={server};" +
                   $"port={port};" +
                   $"database={database};" +
                   $"uid={user};" +
                   $"pwd={password};";
        }

        // Método para debug (não mostra a senha)
        private string GetSafeConnectionString()
        {
            var cs = connectionString;
            if (cs.Contains("pwd="))
            {
                var start = cs.IndexOf("pwd=") + 9;
                var end = cs.IndexOf(";", start);
                if (end > start)
                {
                    var password = cs.Substring(start, end - start);
                    cs = cs.Replace(password, "****");
                }
            }
            return cs;
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                if (!isInitialized)
                    await InitializeAsync();

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                System.Diagnostics.Debug.WriteLine("✅ Conexão com banco estabelecida!");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro de conexão: {ex.Message}");
                return false;
            }
        }


        public async Task<ObservableCollection<Livro>> ObterLivros()
        {
            if (!isInitialized)
                await InitializeAsync();
            // Inicializa a coleção de livros
            var livros = new ObservableCollection<Livro>();

            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT l.*, c.nome as categoria_nome 
                    FROM livros l 
                    LEFT JOIN categorias c ON l.categoria_id = c.id 
                    ORDER BY l.titulo";

                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    livros.Add(new Livro
                    {
                        Id = reader.GetInt32("id"),
                        Titulo = reader.GetString("titulo"),
                        Autor = reader.GetString("autor"),
                        ISBN = reader.IsDBNull(reader.GetOrdinal("isbn")) ? null : reader.GetString("isbn"),
                        Editora = reader.IsDBNull(reader.GetOrdinal("editora")) ? null : reader.GetString("editora"),
                        AnoPulicacao = reader.IsDBNull(reader.GetOrdinal("ano_publicacao")) ? null : reader.GetInt32("ano_publicacao"),
                        Paginas = reader.IsDBNull(reader.GetOrdinal("paginas")) ? null : reader.GetInt32("paginas"),
                        StatusLeitura = reader.GetString("status_leitura"),
                        Avaliacao = reader.IsDBNull(reader.GetOrdinal("avaliacao")) ? null : reader.GetInt32("avaliacao"),
                        CategoriaNome = reader.IsDBNull(reader.GetOrdinal("categoria_nome")) ? "Sem categoria" : reader.GetString("categoria_nome")
                    });
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao buscar livros: {ex.Message}", "OK");
            }

            return livros;
        }

        public async Task<bool> InserirLivro(Livro livro)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO livros (titulo, autor, isbn, editora, ano_publicacao, paginas, categoria_id, sinopse, status_leitura, avaliacao, data_aquisicao, preco)
                    VALUES (@titulo, @autor, @isbn, @editora, @ano, @paginas, @categoria, @sinopse, @status, @avaliacao, @data, @preco)";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@titulo", livro.Titulo);
                command.Parameters.AddWithValue("@autor", livro.Autor);
                command.Parameters.AddWithValue("@isbn", livro.ISBN ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@editora", livro.Editora ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ano", livro.AnoPulicacao ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@paginas", livro.Paginas ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@categoria", livro.CategoriaId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@sinopse", livro.Sinopse ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@status", livro.StatusLeitura ?? "Não lido");
                command.Parameters.AddWithValue("@avaliacao", livro.Avaliacao ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@data", livro.DataAquisicao ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@preco", livro.Preco ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao inserir livro: {ex.Message}", "OK");
                return false;
            }
        }

        public async Task<ObservableCollection<Categoria>> ObterCategorias()
        {
            var categorias = new ObservableCollection<Categoria>();

            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM categorias ORDER BY nome";
                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    categorias.Add(new Categoria
                    {
                        Id = reader.GetInt32("id"),
                        Nome = reader.GetString("nome"),
                        Descricao = reader.GetString("descricao")
                    });
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao buscar categorias: {ex.Message}", "OK");
            }

            return categorias;
        }
    }
}
