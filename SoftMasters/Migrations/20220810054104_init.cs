using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftMasters.test.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Freights",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Freights", x => x.Name);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceNumber = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceNumber);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OperationNames",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(90)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationNames", x => x.Name);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(90)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Name);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Trains",
                columns: table => new
                {
                    TrainId = table.Column<int>(type: "int", nullable: false),
                    FromStationName = table.Column<string>(type: "varchar(90)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToStationName = table.Column<string>(type: "varchar(90)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trains", x => new { x.TrainId, x.FromStationName, x.ToStationName });
                    table.UniqueConstraint("AK_Trains_TrainId", x => x.TrainId);
                    table.ForeignKey(
                        name: "FK_Trains_Stations_FromStationName",
                        column: x => x.FromStationName,
                        principalTable: "Stations",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trains_Stations_ToStationName",
                        column: x => x.ToStationName,
                        principalTable: "Stations",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Compositions",
                columns: table => new
                {
                    CombinedTrainIndex = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrainId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compositions", x => x.CombinedTrainIndex);
                    table.ForeignKey(
                        name: "FK_Compositions_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "TrainId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    CarId = table.Column<int>(type: "int", nullable: false),
                    PositionInTarin = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FreightName = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    CompositionNumber = table.Column<string>(type: "varchar(95)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.CarId);
                    table.ForeignKey(
                        name: "FK_Cars_Compositions_CompositionNumber",
                        column: x => x.CompositionNumber,
                        principalTable: "Compositions",
                        principalColumn: "CombinedTrainIndex",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cars_Freights_FreightName",
                        column: x => x.FreightName,
                        principalTable: "Freights",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cars_Invoices_InvoiceNumber",
                        column: x => x.InvoiceNumber,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceNumber",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    CarNumber = table.Column<int>(type: "int", nullable: false),
                    WhenLastOperation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastOperationName = table.Column<string>(type: "varchar(90)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StationName = table.Column<string>(type: "varchar(90)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => new { x.WhenLastOperation, x.CarNumber });
                    table.ForeignKey(
                        name: "FK_Operations_Cars_CarNumber",
                        column: x => x.CarNumber,
                        principalTable: "Cars",
                        principalColumn: "CarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Operations_OperationNames_LastOperationName",
                        column: x => x.LastOperationName,
                        principalTable: "OperationNames",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Operations_Stations_StationName",
                        column: x => x.StationName,
                        principalTable: "Stations",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_CompositionNumber",
                table: "Cars",
                column: "CompositionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_FreightName",
                table: "Cars",
                column: "FreightName");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_InvoiceNumber",
                table: "Cars",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Compositions_TrainId",
                table: "Compositions",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_CarNumber",
                table: "Operations",
                column: "CarNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_LastOperationName",
                table: "Operations",
                column: "LastOperationName");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_StationName",
                table: "Operations",
                column: "StationName");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_FromStationName",
                table: "Trains",
                column: "FromStationName");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_ToStationName_FromStationName",
                table: "Trains",
                columns: new[] { "ToStationName", "FromStationName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "OperationNames");

            migrationBuilder.DropTable(
                name: "Compositions");

            migrationBuilder.DropTable(
                name: "Freights");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Trains");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}
