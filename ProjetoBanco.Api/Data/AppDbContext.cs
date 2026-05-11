using Microsoft.EntityFrameworkCore;
using ProjetoBanco.Api.Domain;

namespace ProjetoBanco.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<PessoaFisica> PessoasFisicas => Set<PessoaFisica>();
    public DbSet<PessoaJuridica> PessoasJuridicas => Set<PessoaJuridica>();
    public DbSet<Agencia> Agencias => Set<Agencia>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Emprestimo> Emprestimos => Set<Emprestimo>();
    public DbSet<Contratacao> Contratacoes => Set<Contratacao>();
    public DbSet<MaquinaDeCartao> MaquinasDeCartao => Set<MaquinaDeCartao>();
    public DbSet<ReceberSalario> ReceberSalario => Set<ReceberSalario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>()
            .HasDiscriminator<string>("Tipo")
            .HasValue<PessoaFisica>("PF")
            .HasValue<PessoaJuridica>("PJ");

        modelBuilder.Entity<Produto>()
            .HasDiscriminator<string>("Tipo")
            .HasValue<Emprestimo>("EMPRESTIMO")
            .HasValue<MaquinaDeCartao>("MAQUINA")
            .HasValue<ReceberSalario>("SALARIO");

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Agencia)
            .WithMany(a => a.Clientes)
            .HasForeignKey(c => c.AgenciaId);

        modelBuilder.Entity<Contratacao>()
            .HasOne(c => c.Cliente)
            .WithMany(cl => cl.Contratacoes)
            .HasForeignKey(c => c.ClienteId);

        modelBuilder.Entity<Contratacao>()
            .HasOne(c => c.Produto)
            .WithMany(p => p.Contratacoes)
            .HasForeignKey(c => c.ProdutoId);

        modelBuilder.Entity<Contratacao>()
            .Property(c => c.Observacao)
            .IsRequired(false)
            .HasDefaultValue("");

        modelBuilder.Entity<Contratacao>()
            .Property(c => c.DataProcessamento)
            .IsRequired(false);

        modelBuilder.Entity<Cliente>().ToTable("TB_CLIENTES");
        modelBuilder.Entity<Agencia>().ToTable("TB_AGENCIAS");
        modelBuilder.Entity<Produto>().ToTable("TB_PRODUTOS");
        modelBuilder.Entity<Contratacao>().ToTable("TB_CONTRATACOES");
    }
}