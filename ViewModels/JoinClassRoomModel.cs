using System.ComponentModel.DataAnnotations;

namespace Classroom.ViewModels
{
    public class JoinClassRoomModel
    {
        [Required]
        public string? ClassRoomUnicCode { get; set; }
    }
}
