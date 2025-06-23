using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AcervoPessoal.Configuration
{
    internal class EnvLoader
    {
        public static async Task LoadAsync()
        {
            try
            {
                // Tentar múltiplas abordagens para encontrar o .env

                // Método 1: Arquivo local (funciona em desenvolvimento)
                await LoadFromLocalFile();

                // Método 2: Embedded Resource (funciona em produção)
                await LoadFromEmbeddedResource();

                // Método 3: Platform-specific paths
                await LoadFromPlatformSpecificPath();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar .env: {ex.Message}");
            }
        }

        private static async Task LoadFromLocalFile()
        {
            try
            {
                // Tentar diferentes caminhos
                string[] possiblePaths = {
                    Path.Combine(Environment.CurrentDirectory, ".env"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
                    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
                    ".env"
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        DotNetEnv.Env.Load(path);
                        System.Diagnostics.Debug.WriteLine($"✅ .env carregado de: {path}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadFromLocalFile erro: {ex.Message}");
            }
        }

        private static async Task LoadFromEmbeddedResource()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "AcervoPessoal..env";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string content = await reader.ReadToEndAsync();

                            // Salvar temporariamente e carregar
                            var tempPath = Path.Combine(FileSystem.CacheDirectory, ".env");
                            await File.WriteAllTextAsync(tempPath, content);
                            DotNetEnv.Env.Load(tempPath);

                            System.Diagnostics.Debug.WriteLine("✅ .env carregado de embedded resource");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadFromEmbeddedResource erro: {ex.Message}");
            }
        }

        private static async Task LoadFromPlatformSpecificPath()
        {
            try
            {
                string envPath = "";

                #if ANDROID
                envPath = Path.Combine(FileSystem.Current.AppDataDirectory, ".env");
                #elif IOS
                envPath = Path.Combine(FileSystem.Current.AppDataDirectory, ".env");
                #elif WINDOWS
                envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
                #endif

                if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
                {
                    DotNetEnv.Env.Load(envPath);
                    System.Diagnostics.Debug.WriteLine($"✅ .env carregado de: {envPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadFromPlatformSpecificPath erro: {ex.Message}");
            }
        }

        // Método alternativo: Carregar de string hardcoded (para testes)
        public static void LoadFromString(string envContent)
        {
            var lines = envContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (!line.StartsWith("#") && line.Contains("="))
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                    }
                }
            }
        }
    }
}
