using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoBanco.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_AGENCIAS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Numero = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Endereco = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_AGENCIAS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_PRODUTOS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Descricao = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Tipo = table.Column<string>(type: "NVARCHAR2(13)", maxLength: 13, nullable: false),
                    ValorSolicitado = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    TaxaJuros = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    PrazoDias = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TaxaMDR = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    EmpresaConvenio = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PRODUTOS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_CLIENTES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Telefone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    AgenciaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Tipo = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    CPF = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CNPJ = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RazaoSocial = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CLIENTES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TB_CLIENTES_TB_AGENCIAS_AgenciaId",
                        column: x => x.AgenciaId,
                        principalTable: "TB_AGENCIAS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_CONTRATACOES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ClienteId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ProdutoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Observacao = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CONTRATACOES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TB_CONTRATACOES_TB_CLIENTES_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "TB_CLIENTES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB_CONTRATACOES_TB_PRODUTOS_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "TB_PRODUTOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CLIENTES_AgenciaId",
                table: "TB_CLIENTES",
                column: "AgenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_TB_CONTRATACOES_ClienteId",
                table: "TB_CONTRATACOES",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_TB_CONTRATACOES_ProdutoId",
                table: "TB_CONTRATACOES",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_CONTRATACOES");

            migrationBuilder.DropTable(
                name: "TB_CLIENTES");

            migrationBuilder.DropTable(
                name: "TB_PRODUTOS");

            migrationBuilder.DropTable(
                name: "TB_AGENCIAS");
        }
    }
}
