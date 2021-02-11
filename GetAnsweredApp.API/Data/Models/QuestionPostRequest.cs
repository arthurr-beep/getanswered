using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GetAnsweredApp.API.Data.Models
{
    public class QuestionPostFullRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Please include some content for the question")]
        public string Content { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }
    }

    public class QuestionPostRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please include some content for the question")]
        public string Content { get; set; }
        
    }
}
