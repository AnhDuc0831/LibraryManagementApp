using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class BookImportViewModel
    {
        // Thuộc tính để Binding vào Checkbox
        public bool IsSelected { get; set; } = true; // Mặc định chọn hết

        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Category { get; set; }
        public int PublishYear { get; set; }
        public int Price { get; set; }
    }
}
