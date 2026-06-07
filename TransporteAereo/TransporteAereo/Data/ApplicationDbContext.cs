using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransporteAereo.Models;

namespace TransporteAereo.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Aeronave> Aeronave { get; set; } = default!;
        public DbSet<Aeroporto> Aeroporto { get; set; } = default!;
        public DbSet<Assento> Assento { get; set; } = default!;
        public DbSet<Escala> Escala { get; set; } = default!;
        public DbSet<Reserva> Reserva { get; set; } = default!;
        public DbSet<Voo> Voo { get; set; } = default!;
        public DbSet<VooPoltrona> VooPoltrona { get; set; } = default!;
        public DbSet<Viagem> Viagem { get; set; } = default!;

        public DbSet<Compra> Compra { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Cliente)
                .WithMany()
                .HasForeignKey(c => c.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Compra)
                .WithMany(c => c.Reservas)
                .HasForeignKey(r => r.IdCompra)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Viagem)
                .WithMany() 
                .HasForeignKey(c => c.IdViagem)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Cliente)
                .WithMany(u => u.Reservas) 
                .HasForeignKey(r => r.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voo>()
                .HasOne(v => v.Aeronave)
                .WithMany(a => a.Voos)
                .HasForeignKey(v => v.IdAeronave)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.CPF)
                .IsUnique();

            modelBuilder.Entity<Voo>()
                .HasOne(v => v.AeroportoOrigem)
                .WithMany(a => a.VoosDeOrigem)
                .HasForeignKey(v => v.IdOrigem)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voo>()
                .HasOne(v => v.AeroportoDestino)
                .WithMany(a => a.VoosDeDestino)
                .HasForeignKey(v => v.IdDestino)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VooPoltrona>()
                .HasIndex(vp => new { vp.IdVoo, vp.IdPoltrona })
                .IsUnique();

            modelBuilder.Entity<VooPoltrona>()
                .HasOne(vp => vp.Voo)
                .WithMany(v => v.VooPoltronas)
                .HasForeignKey(vp => vp.IdVoo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VooPoltrona>()
                .HasOne(vp => vp.Assento)
                .WithMany(a => a.VooPoltronas)
                .HasForeignKey(vp => vp.IdPoltrona)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .Property(r => r.DataReserva)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.VooPoltrona)
                .WithMany(vp => vp.Reservas)
                .HasForeignKey(r => r.IdVooPoltrona)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voo>()
                .HasOne(v => v.Viagem)
                .WithMany(vi => vi.Voos)
                .HasForeignKey(v => v.IdViagem)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Viagem>()
                .HasOne(v => v.AeroportoOrigem)
                .WithMany(a => a.ViagensDeOrigem)
                .HasForeignKey(v => v.IdOrigem)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Viagem>()
                .HasOne(v => v.AeroportoDestino)
                .WithMany(a => a.ViagensDeDestino)
                .HasForeignKey(v => v.IdDestino)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}