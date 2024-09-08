using System.ComponentModel.DataAnnotations;
using Fantasy.Shared.Resources;

namespace Fantasy.Shared.Entities;

public class Team
{
    public int Id { get; set; }

    [Display(Name = "Team", ResourceType = typeof(Literals))]
    [MaxLength(100, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Literals))]
    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Literals))]
    public string Name { get; set; } = null!;

    public string? Image { get; set; }

    public Country Country { get; set; } = null!;

    [Display(Name = "Country", ResourceType = typeof(Literals))]
    [Range(1, int.MaxValue, ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Literals))]
    public int CountryId { get; set; }

    [Display(Name = "IsImageSquare", ResourceType = typeof(Literals))]
    public bool IsImageSquare { get; set; }

    public string ImageFull => string.IsNullOrEmpty(Image) ? "/images/NoImage.png" : Image;

    public ICollection<TournamentTeam>? TournamentTeams { get; set; }

    public int TournamentsCount => TournamentTeams == null ? 0 : TournamentTeams.Count;
}