using LabTrackLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


namespace LabTrackLite
{
  record ChatQuery(string Query);

  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      var defaultConn = builder.Configuration.GetConnectionString("Default");
      if (!string.IsNullOrEmpty(defaultConn))
      {
        if (defaultConn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
          // Convert URI-style PostgreSQL URL to Npgsql connection string
          var uri = new Uri(defaultConn);
          var userInfo = uri.UserInfo.Split(':');
          var username = userInfo.Length > 0 ? userInfo[0] : string.Empty;
          var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
          var host = uri.Host;
          var portPart = uri.Port > 0 ? $";Port={uri.Port}" : string.Empty;
          var database = uri.AbsolutePath.TrimStart('/');
          var connBuilder = $"Host={host};Username={username};Password={password};Database={database}{portPart};Ssl Mode=Require;Trust Server Certificate=true";
          builder.Services.AddDbContext<LabDbContext>(o => o.UseNpgsql(connBuilder));
        }
        else
        {
          builder.Services.AddDbContext<LabDbContext>(o => o.UseNpgsql(defaultConn));
        }
      }
      else
      {
        // No connection string provided; use an in-memory database only if the provider is available.
        // Keep as Npgsql by default; developer can add an alternative DB provider for local runs.
        builder.Services.AddDbContext<LabDbContext>(o => o.UseNpgsql(""));
      }

      builder.Services.AddCors(options =>
{
 options.AddPolicy("frontend", p =>
  p.WithOrigins("http://localhost:5173")
   .AllowAnyHeader()
   .AllowAnyMethod()
   .AllowCredentials());
});

      builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(o =>
{
 o.TokenValidationParameters = new()
 {
   ValidateIssuer = false,
   ValidateAudience = false,
   ValidateLifetime = true,
   IssuerSigningKey = new SymmetricSecurityKey(
     Encoding.UTF8.GetBytes("SUPER_SECRET_KEY"))
 };
});


      builder.Services.AddAuthorization();

      var app = builder.Build();
      app.UseCors("frontend");
      app.Use(async (ctx, next) =>
{
 ctx.Response.Headers["Access-Control-Allow-Origin"] = "http://localhost:5173";
 ctx.Response.Headers["Access-Control-Allow-Headers"] = "*";
 ctx.Response.Headers["Access-Control-Allow-Methods"] = "*";

 await next();
});
app.Use(async (ctx, next) =>
{
 ctx.Response.StatusCode = 200;
 await next();
});


      




      // ---------------- SEED DATA ----------------
      using (var scope = app.Services.CreateScope())
      {
        var db = scope.ServiceProvider.GetRequiredService<LabDbContext>();
        // Ensure database schema exists (for simple local runs)
        db.Database.EnsureCreated();

        if (!db.Assets.Any())
        {
          db.Assets.AddRange(
            new Asset { Name = "Microscope", Category = "Optical", QrCode = "QR-101", Status = "Available" },
            new Asset { Name = "Centrifuge", Category = "Mechanical", QrCode = "QR-102", Status = "Repair" },
            new Asset { Name = "Spectrometer", Category = "Chemical", QrCode = "QR-103", Status = "Available" }
          );
          db.SaveChanges();
        }

        if (!db.Tickets.Any())
        {
          db.Tickets.Add(new Ticket { Title = "Lens not working", Status = "Open", AssetId = 1 });
          db.SaveChanges();
        }
        if(!db.Users.Any())
{
 db.Users.AddRange(
   new User{Username="admin",Password="admin",Role="Admin"},
   new User{Username="eng",Password="eng",Role="Engineer"},
   new User{Username="tech",Password="tech",Role="Technician"}
 );
 db.SaveChanges();
}
      }

            // Rate limiting disabled: remove or configure when required

      app.UseAuthorization();
      //login api
      app.MapPost("/login", (LoginDto dto, LabDbContext db) =>
{
 var user = db.Users.FirstOrDefault(x =>
     x.Username == dto.Username && x.Password == dto.Password);

 if (user == null) return Results.Ok(new { role = "Guest", token = "LOGGED_IN" });

 return Results.Ok(new { role = user.Role, token = "LOGGED_IN" });
});





      // ---------------- ASSETS ----------------
      app.MapGet("/assets", (LabDbContext db, int page = 1) =>
        db.Assets.AsNoTracking().Skip((page - 1) * 5).Take(5).ToList());

      app.MapPost("/assets",(Asset a, LabDbContext db) =>
      {
        db.Assets.Add(a);
        db.SaveChanges();

        db.AuditLogs.Add(new AuditLog
        {
          UserId = 1,
          Action = "CREATE",
          Entity = "Asset",
          EntityId = a.Id
        });
        db.SaveChanges();
        return Results.Ok(a);
      });


      // ---------------- TICKETS ----------------
      app.MapGet("/tickets", (LabDbContext db) => db.Tickets.AsNoTracking().ToList());

      app.MapPost("/tickets", (Ticket t, LabDbContext db) =>
      {
        db.Tickets.Add(t);
        db.SaveChanges();

        db.AuditLogs.Add(new AuditLog
        {
          UserId = 1,
          Action = "CREATE",
          Entity = "Ticket",
          EntityId = t.Id
        });
        db.SaveChanges();
        return Results.Ok(t);
      });

      app.MapPatch("/tickets/{id}",  (int id, string newStatus, ClaimsPrincipal user, LabDbContext db) =>
      {
        var ticket = db.Tickets.Find(id);
        if (ticket == null)
          return Results.NotFound();

        var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "Technician";

        if (!IsValidTransition(ticket.Status, newStatus, role))
          return Results.BadRequest("Invalid status transition");

        ticket.Status = newStatus;
        db.SaveChanges();
        return Results.Ok(ticket);
      });


      // ---------------- USERS ----------------
      app.MapGet("/users", [Authorize(Roles = "Admin")] (LabDbContext db) => db.Users);

      app.MapPost("/users", [Authorize(Roles = "Admin")] (User u, LabDbContext db) =>
      {
        db.Users.Add(u);
        db.SaveChanges();

        db.AuditLogs.Add(new AuditLog
        {
          UserId = 1,
          Action = "CREATE",
          Entity = "User",
          EntityId = u.Id
        });
        db.SaveChanges();
        return Results.Ok(u);
      });


      // ---------------- COMMENTS ----------------
      app.MapGet("/tickets/{id}/comments", (int id, LabDbContext db) =>
        db.Comments.Where(c => c.TicketId == id).AsNoTracking());

      app.MapPost("/tickets/{id}/comments", [Authorize] (int id, Comment c, LabDbContext db) =>
      {
        c.TicketId = id;
        db.Comments.Add(c);
        db.SaveChanges();

        db.AuditLogs.Add(new AuditLog
        {
          UserId = 1,
          Action = "CREATE",
          Entity = "Comment",
          EntityId = c.Id
        });
        db.SaveChanges();
        return Results.Ok(c);
      });


      // ---------------- CHATBOT ----------------
      app.MapPost("/chat/query", (ChatQuery q, LabDbContext db) =>
      {
        var m = q.Query?.ToLower() ?? string.Empty;

        if (m.Contains("open tickets"))
          return Results.Ok(db.Tickets.Where(t => t.Status == "Open"));

        if (m.Contains("count assets"))
          return Results.Ok(db.Assets.Count());

        if (m.Contains("repair assets"))
          return Results.Ok(db.Assets.Where(a => a.Status == "Repair"));

        return Results.Ok("Try: open tickets, count assets, repair assets");
      });

      app.Run();
    }

    static bool IsValidTransition(string from, string to, string role)
    {
      if (from == "Open" && to == "InProgress") return true;
      if (from == "InProgress" && to == "Resolved") return true;
      if (from == "Resolved" && to == "Closed") return true;
      if (from == "Closed" && to == "Open" && role != "Technician") return true;
      return false;
    }
  }
}


