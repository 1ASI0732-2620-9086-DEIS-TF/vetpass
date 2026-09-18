using Microsoft.EntityFrameworkCore;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.MedicalRecords.Domain.Model.Aggregates;
using VetPass.API.MedicalRecords.Domain.Model.Entities;
using VetPass.API.MedicalRecords.Domain.Model.ValueObjects;
using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Vaccination.Domain.Model.Aggregates;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Persistence context of the platform. Every table lives in the schema
/// <c>vetpass</c>, which is not among the schemas exposed by PostgREST: the
/// only client of the database is this API, as the container diagram declares.
/// The schema <c>auth</c>, where Supabase Auth keeps the credentials, is not
/// managed here and is only referenced by a foreign key.
/// </summary>
public class VetPassDbContext(DbContextOptions<VetPassDbContext> options) : DbContext(options)
{
    public const string Schema = "vetpass";

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Vaccine> Vaccines => Set<Vaccine>();
    public DbSet<ScheduleItem> ScheduleItems => Set<ScheduleItem>();
    public DbSet<VaccinationCard> VaccinationCards => Set<VaccinationCard>();
    public DbSet<Dose> Doses => Set<Dose>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);

        ConfigureIdentityAndAccess(builder);
        ConfigurePatients(builder);
        ConfigureVaccination(builder);
        ConfigureMedicalRecords(builder);
    }

    private static void ConfigureIdentityAndAccess(ModelBuilder builder)
    {
        builder.Entity<Clinic>(clinic =>
        {
            clinic.ToTable("clinics");
            clinic.HasKey(c => c.Id);
            clinic.Property(c => c.Id).HasColumnName("id");
            clinic.Property(c => c.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            clinic.Property(c => c.Address).HasColumnName("address").HasMaxLength(240);
        });

        builder.Entity<UserProfile>(profile =>
        {
            profile.ToTable("user_profiles");
            profile.HasKey(p => p.Id);
            // The identifier is the one of the account in Supabase Auth: the
            // profile and the credentials are the same user seen from the two
            // sides of the boundary.
            profile.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
            profile.Property(p => p.Email).HasColumnName("email").HasMaxLength(160).IsRequired();
            profile.Property(p => p.FullName).HasColumnName("full_name").HasMaxLength(160).IsRequired();
            profile.Property(p => p.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(20).IsRequired();
            profile.Property(p => p.ClinicId).HasColumnName("clinic_id");
            profile.Property(p => p.ClientId).HasColumnName("client_id");
            profile.Property(p => p.CreatedAt).HasColumnName("created_at");
            profile.HasIndex(p => p.Email).IsUnique();
            profile.HasOne<Clinic>().WithMany().HasForeignKey(p => p.ClinicId).OnDelete(DeleteBehavior.Restrict);
            profile.HasOne<Client>().WithMany().HasForeignKey(p => p.ClientId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePatients(ModelBuilder builder)
    {
        builder.Entity<Client>(client =>
        {
            client.ToTable("clients");
            client.HasKey(c => c.Id);
            client.Property(c => c.Id).HasColumnName("id");
            client.Property(c => c.ClinicId).HasColumnName("clinic_id").IsRequired();
            client.Property(c => c.FullName).HasColumnName("full_name").HasMaxLength(160).IsRequired();
            client.Property(c => c.PhoneNumber).HasColumnName("phone_number").HasMaxLength(30).IsRequired();
            client.Property(c => c.Email).HasColumnName("email").HasMaxLength(160);
            client.Property(c => c.CreatedAt).HasColumnName("created_at");
            client.HasIndex(c => c.FullName);
            client.HasOne<Clinic>().WithMany().HasForeignKey(c => c.ClinicId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Pet>(pet =>
        {
            pet.ToTable("pets");
            pet.HasKey(p => p.Id);
            pet.Property(p => p.Id).HasColumnName("id");
            pet.Property(p => p.ClientId).HasColumnName("client_id").IsRequired();
            pet.Property(p => p.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
            pet.Property(p => p.Species).HasColumnName("species").HasConversion<string>().HasMaxLength(20).IsRequired();
            pet.Property(p => p.Breed).HasColumnName("breed").HasMaxLength(80);
            pet.Property(p => p.Sex).HasColumnName("sex").HasConversion<string>().HasMaxLength(10).IsRequired();
            pet.Property(p => p.BirthDate).HasColumnName("birth_date").IsRequired();
            pet.Property(p => p.CreatedAt).HasColumnName("created_at");
            pet.HasIndex(p => p.Name);
            pet.HasOne<Client>().WithMany().HasForeignKey(p => p.ClientId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureVaccination(ModelBuilder builder)
    {
        builder.Entity<Vaccine>(vaccine =>
        {
            vaccine.ToTable("vaccines");
            vaccine.HasKey(v => v.Id);
            vaccine.Property(v => v.Id).HasColumnName("id").ValueGeneratedNever();
            vaccine.Property(v => v.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            vaccine.Property(v => v.Species).HasColumnName("species").HasConversion<string>().HasMaxLength(20).IsRequired();
            vaccine.Property(v => v.IsCore).HasColumnName("is_core").IsRequired();
        });

        builder.Entity<ScheduleItem>(item =>
        {
            item.ToTable("schedule_items");
            item.HasKey(i => i.Id);
            item.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
            item.Property(i => i.VaccineId).HasColumnName("vaccine_id").IsRequired();
            item.Property(i => i.Species).HasColumnName("species").HasConversion<string>().HasMaxLength(20).IsRequired();
            item.Property(i => i.SequenceNumber).HasColumnName("sequence_number").IsRequired();
            item.Property(i => i.MinimumAgeInWeeks).HasColumnName("minimum_age_weeks").IsRequired();
            item.Property(i => i.MinimumIntervalInWeeks).HasColumnName("minimum_interval_weeks").IsRequired();
            item.HasIndex(i => new { i.VaccineId, i.SequenceNumber }).IsUnique();
            item.HasOne<Vaccine>().WithMany().HasForeignKey(i => i.VaccineId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<VaccinationCard>(card =>
        {
            card.ToTable("vaccination_cards");
            card.HasKey(c => c.Id);
            card.Property(c => c.Id).HasColumnName("id");
            card.Property(c => c.PetId).HasColumnName("pet_id").IsRequired();
            card.Property(c => c.Species).HasColumnName("species").HasConversion<string>().HasMaxLength(20).IsRequired();
            card.Property(c => c.PetBirthDate).HasColumnName("pet_birth_date").IsRequired();
            card.Property(c => c.CreatedAt).HasColumnName("created_at");
            // One pet holds exactly one card, created at the moment of its
            // registration: the uniqueness is a rule of the domain (4.10).
            card.HasIndex(c => c.PetId).IsUnique();
            card.HasOne<Pet>().WithMany().HasForeignKey(c => c.PetId).OnDelete(DeleteBehavior.Cascade);
            card.Metadata.FindNavigation(nameof(VaccinationCard.Doses))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
            card.HasMany(c => c.Doses).WithOne().HasForeignKey(d => d.CardId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Dose>(dose =>
        {
            dose.ToTable("doses");
            dose.HasKey(d => d.Id);
            dose.Property(d => d.Id).HasColumnName("id");
            dose.Property(d => d.CardId).HasColumnName("card_id").IsRequired();
            dose.Property(d => d.VaccineId).HasColumnName("vaccine_id").IsRequired();
            dose.Property(d => d.SequenceNumber).HasColumnName("sequence_number").IsRequired();
            dose.Property(d => d.ExpectedDate).HasColumnName("expected_date").IsRequired();
            dose.Property(d => d.ApplicationDate).HasColumnName("application_date");
            dose.Property(d => d.VeterinarianId).HasColumnName("veterinarian_id");
            dose.Property(d => d.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
            dose.Property(d => d.MinimumAgeInWeeks).HasColumnName("minimum_age_weeks").IsRequired();
            dose.Property(d => d.MinimumIntervalInWeeks).HasColumnName("minimum_interval_weeks").IsRequired();
            // The batch code is a value object of a single attribute: it is
            // stored in the column of the dose, not in a table of its own.
            dose.OwnsOne(d => d.BatchCode, batch =>
                batch.Property(b => b.Value).HasColumnName("batch_code").HasMaxLength(40));
            dose.HasIndex(d => d.Status);
            dose.HasOne<Vaccine>().WithMany().HasForeignKey(d => d.VaccineId).OnDelete(DeleteBehavior.Restrict);
            dose.HasOne<UserProfile>().WithMany().HasForeignKey(d => d.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMedicalRecords(ModelBuilder builder)
    {
        builder.Entity<Visit>(visit =>
        {
            visit.ToTable("visits");
            visit.HasKey(v => v.Id);
            visit.Property(v => v.Id).HasColumnName("id");
            visit.Property(v => v.PetId).HasColumnName("pet_id").IsRequired();
            visit.Property(v => v.VeterinarianId).HasColumnName("veterinarian_id").IsRequired();
            visit.Property(v => v.VisitDate).HasColumnName("visit_date").IsRequired();
            visit.Property(v => v.Reason).HasColumnName("reason").HasMaxLength(240).IsRequired();
            visit.Property(v => v.Findings).HasColumnName("findings");
            visit.Property(v => v.Diagnosis).HasColumnName("diagnosis").IsRequired();
            visit.Property(v => v.Treatment).HasColumnName("treatment");
            visit.Property(v => v.WeightKg).HasColumnName("weight_kg").HasPrecision(5, 2);
            visit.Property(v => v.CreatedAt).HasColumnName("created_at");
            visit.HasIndex(v => new { v.PetId, v.VisitDate });
            visit.HasOne<Pet>().WithMany().HasForeignKey(v => v.PetId).OnDelete(DeleteBehavior.Cascade);
            visit.HasOne<UserProfile>().WithMany().HasForeignKey(v => v.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
            visit.HasOne(v => v.Prescription).WithOne()
                .HasForeignKey<Prescription>(p => p.VisitId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Prescription>(prescription =>
        {
            prescription.ToTable("prescriptions");
            prescription.HasKey(p => p.Id);
            prescription.Property(p => p.Id).HasColumnName("id");
            prescription.Property(p => p.VisitId).HasColumnName("visit_id").IsRequired();
            prescription.Property(p => p.IssuedAt).HasColumnName("issued_at");
            prescription.HasIndex(p => p.VisitId).IsUnique();
            prescription.Metadata.FindNavigation(nameof(Prescription.Items))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
            prescription.HasMany(p => p.Items).WithOne()
                .HasForeignKey(i => i.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PrescriptionItem>(item =>
        {
            item.ToTable("prescription_items");
            item.HasKey(i => i.Id);
            item.Property(i => i.Id).HasColumnName("id");
            item.Property(i => i.PrescriptionId).HasColumnName("prescription_id").IsRequired();
            item.Property(i => i.Medication).HasColumnName("medication").HasMaxLength(160).IsRequired();
            item.Property(i => i.Dosage).HasColumnName("dosage").HasMaxLength(240).IsRequired();
            item.Property(i => i.Duration).HasColumnName("duration").HasMaxLength(120).IsRequired();
        });
    }
}
