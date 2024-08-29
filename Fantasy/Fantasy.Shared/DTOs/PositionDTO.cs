using Fantasy.Shared.Entities;

namespace Fantasy.Shared.DTOs
{
    public class PositionDTO
    {
        public User User { get; set; } = null!;

        public int Points { get; set; }
    }
}