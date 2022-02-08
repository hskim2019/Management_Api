using ManagementApplication.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management_Api.Models
{
    public class Note
    {
        [Key]
        public int NoteNo { get; set; }
        //[Required]
        public string NoteTitle { get; set; }
        //[Required]
        public string NoteContents { get; set; }
        //[Required]
        public string UserID { get; set; }
        //[ForeignKey("UserNo")]
        //public virtual User User { get; set; }

    }
}
