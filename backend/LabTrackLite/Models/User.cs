namespace LabTrackLite;

public class User
{
 public int Id { get; set; }
 public string Username { get; set; }
 public string Password { get; set; }   // (plain for hackathon)
 public string Role { get; set; }       // Admin, Engineer, Technician
}
