using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Entities;
using companyOSINT.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Website> Websites => Set<Website>();
    public DbSet<Software> Software => Set<Software>();
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<DomainToSkip> DomainsToSkip => Set<DomainToSkip>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<PostalCode> PostalCodes => Set<PostalCode>();
    public DbSet<UserProject> UserProjects => Set<UserProject>();
    public DbSet<UserProjectCompany> UserProjectCompanies => Set<UserProjectCompany>();
    public DbSet<ApiToken> ApiTokens => Set<ApiToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>()
            .HasIndex(c => c.DateLastChecked)
            .HasDatabaseName("IX_Companies_DateLastChecked");

        modelBuilder.Entity<Website>()
            .HasIndex(w => w.DateLastChecked)
            .HasDatabaseName("IX_Websites_DateLastChecked");

        modelBuilder.Entity<DomainToSkip>()
            .HasIndex(d => d.Domain)
            .IsUnique()
            .HasDatabaseName("IX_DomainsToSkip_Domain");

        modelBuilder.Entity<Software>()
            .HasIndex(s => s.WebsiteId)
            .HasDatabaseName("IX_Software_WebsiteId");

        modelBuilder.Entity<Tool>()
            .HasIndex(t => t.WebsiteId)
            .HasDatabaseName("IX_Tools_WebsiteId");

        modelBuilder.Entity<Sector>()
            .HasIndex(s => s.Name)
            .IsUnique()
            .HasDatabaseName("IX_Sectors_Name");

        modelBuilder.Entity<Company>()
            .HasIndex(c => c.PostalCode)
            .HasDatabaseName("IX_Companies_PostalCode");

        modelBuilder.Entity<Company>()
            .HasOne(c => c.Sector)
            .WithMany()
            .HasForeignKey(c => c.SectorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PostalCode>(entity =>
        {
            entity.HasKey(p => p.Code);
            entity.Property(p => p.Code).HasMaxLength(5).IsFixedLength();
            entity.Property(p => p.Place).HasMaxLength(100);
        });

        modelBuilder.Entity<UserProject>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProject>()
            .HasIndex(p => p.UserId)
            .HasDatabaseName("IX_UserProjects_UserId");

        modelBuilder.Entity<UserProject>()
            .HasMany(p => p.Companies)
            .WithOne(pc => pc.Project)
            .HasForeignKey(pc => pc.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProjectCompany>()
            .HasOne(pc => pc.Company)
            .WithMany()
            .HasForeignKey(pc => pc.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProjectCompany>()
            .HasIndex(pc => new { pc.ProjectId, pc.CompanyId })
            .IsUnique()
            .HasDatabaseName("IX_UserProjectCompanies_ProjectId_CompanyId");

        modelBuilder.Entity<ApiToken>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApiToken>()
            .HasIndex(t => t.UserId)
            .HasDatabaseName("IX_ApiTokens_UserId");

        modelBuilder.Entity<ApiToken>()
            .HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_ApiTokens_TokenHash");

        modelBuilder.Entity<Sector>().HasData(
            // Produktion & Industrie
            SeedSector("Maschinenbau"),
            SeedSector("Automobilindustrie & Fahrzeugbau"),
            SeedSector("Elektrotechnik & Elektronik"),
            SeedSector("Chemie & Pharma"),
            SeedSector("Kunststoff & Gummi"),
            SeedSector("Metallerzeugung & -verarbeitung"),
            SeedSector("Lebensmittelherstellung"),
            SeedSector("Holz- & Möbelindustrie"),
            SeedSector("Textil & Bekleidung"),
            SeedSector("Papier & Druck"),
            SeedSector("Glas, Keramik & Baustoffe"),
            SeedSector("Medizintechnik"),
            SeedSector("Luft- & Raumfahrttechnik"),
            SeedSector("Optik & Feinmechanik"),
            // Bau & Immobilien
            SeedSector("Baugewerbe & Hochbau"),
            SeedSector("Tiefbau & Infrastruktur"),
            SeedSector("Gebäudetechnik & Haustechnik"),
            SeedSector("Immobilienwirtschaft"),
            SeedSector("Architektur & Planung"),
            // Handel
            SeedSector("Großhandel"),
            SeedSector("Einzelhandel"),
            SeedSector("Online-Handel & E-Commerce"),
            SeedSector("Kraftfahrzeughandel"),
            // IT & Telekommunikation
            SeedSector("Softwareentwicklung"),
            SeedSector("IT-Dienstleistungen & Beratung"),
            SeedSector("Telekommunikation"),
            SeedSector("Rechenzentren & Cloud-Dienste"),
            SeedSector("Cybersecurity"),
            // Dienstleistungen
            SeedSector("Unternehmensberatung"),
            SeedSector("Rechtsberatung"),
            SeedSector("Steuerberatung & Wirtschaftsprüfung"),
            SeedSector("Marketing & Werbung"),
            SeedSector("Personaldienstleistung"),
            SeedSector("Facility Management"),
            SeedSector("Sicherheitsdienstleistungen"),
            SeedSector("Reinigung & Gebäudedienste"),
            // Finanzwesen
            SeedSector("Bankwesen"),
            SeedSector("Versicherungen"),
            SeedSector("Finanzdienstleistungen & Vermögensverwaltung"),
            // Gesundheit & Soziales
            SeedSector("Gesundheitswesen & Kliniken"),
            SeedSector("Pflege & Betreuung"),
            SeedSector("Sozialwesen"),
            // Transport & Logistik
            SeedSector("Spedition & Logistik"),
            SeedSector("Personenbeförderung"),
            SeedSector("Schifffahrt & Hafenwirtschaft"),
            SeedSector("Luftfahrt"),
            // Energie & Umwelt
            SeedSector("Energieversorgung"),
            SeedSector("Erneuerbare Energien"),
            SeedSector("Entsorgung & Recycling"),
            SeedSector("Wasser- & Abwasserwirtschaft"),
            // Medien & Kommunikation
            SeedSector("Verlagswesen"),
            SeedSector("Film, TV & Rundfunk"),
            SeedSector("Öffentlichkeitsarbeit & PR"),
            // Bildung & Forschung
            SeedSector("Bildung & Weiterbildung"),
            SeedSector("Forschung & Entwicklung"),
            // Gastronomie & Tourismus
            SeedSector("Gastronomie"),
            SeedSector("Hotellerie & Beherbergung"),
            SeedSector("Tourismus & Reiseveranstaltung"),
            SeedSector("Event & Veranstaltung"),
            // Weitere
            SeedSector("Landwirtschaft & Gartenbau"),
            SeedSector("Forstwirtschaft & Holzwirtschaft"),
            SeedSector("Handwerk"),
            SeedSector("Design & Kreativwirtschaft"),
            SeedSector("Sport & Freizeit"),
            SeedSector("Kultur & Unterhaltung"),
            SeedSector("Bergbau & Rohstoffe")
        );

        modelBuilder.Entity<DomainToSkip>().HasData(
            SeedDomain("northdata.de"), SeedDomain("northdata.com"),
            SeedDomain("firmenwissen.de"),
            SeedDomain("companyhouse.de"), SeedDomain("companyhouse.com"),
            SeedDomain("unternehmensregister.de"),
            SeedDomain("handelsregister.de"),
            SeedDomain("bundesanzeiger.de"),
            SeedDomain("dnb.com"),
            SeedDomain("bisnode.de"),
            SeedDomain("creditreform.de"),
            SeedDomain("hoppenstedt.de"),
            SeedDomain("gelbeseiten.de"),
            SeedDomain("dasoertliche.de"),
            SeedDomain("11880.com"),
            SeedDomain("golocal.de"),
            SeedDomain("yelp.de"), SeedDomain("yelp.com"),
            SeedDomain("tripadvisor.de"), SeedDomain("tripadvisor.com"),
            SeedDomain("kununu.com"),
            SeedDomain("wlw.de"),
            SeedDomain("europages.de"), SeedDomain("europages.com"),
            SeedDomain("facebook.com"), SeedDomain("facebook.de"),
            SeedDomain("linkedin.com"),
            SeedDomain("xing.com"),
            SeedDomain("instagram.com"),
            SeedDomain("twitter.com"), SeedDomain("x.com"),
            SeedDomain("youtube.com"),
            SeedDomain("tiktok.com"),
            SeedDomain("wikipedia.org"),
            SeedDomain("wikidata.org"),
            SeedDomain("google.com"), SeedDomain("google.de"),
            SeedDomain("apple.com"),
            SeedDomain("amazon.de"), SeedDomain("amazon.com"),
            SeedDomain("ebay.de"), SeedDomain("ebay.com")
        );
    }

    private static Sector SeedSector(string name)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes($"sector:{name}"));
        return new Sector
        {
            Id = new Guid(bytes),
            Name = name,
            DateCreated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DateModified = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private static DomainToSkip SeedDomain(string domain)
    {
        // Deterministic GUID from domain name for stable seed data
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(domain));
        return new DomainToSkip
        {
            Id = new Guid(bytes),
            Domain = domain,
            DateCreated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DateModified = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.DateCreated = now;
                entry.Entity.DateModified = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.DateModified = now;
            }
        }
    }
}
