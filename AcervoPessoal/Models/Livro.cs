using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcervoPessoal.Models
{
    internal class Livro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string ISBN { get; set; }
        public string Editora { get; set; }
        public int? AnoPulicacao { get; set; }
        public int? Paginas { get; set; }
        public int? CategoriaId { get; set; }
        public string Sinopse { get; set; }
        public string StatusLeitura { get; set; }
        public int? Avaliacao { get; set; }
        public DateTime? DataAquisicao { get; set; }
        public decimal? Preco { get; set; }
        public string CategoriaNome { get; set; }
    }
}
