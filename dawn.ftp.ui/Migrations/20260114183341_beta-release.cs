using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dawn.ftp.ui.ui.Migrations
{
    /// <inheritdoc />
    public partial class betarelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileTransferModels",
                columns: table => new
                {
                    FileTransferId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FailureReason = table.Column<string>(type: "TEXT", nullable: true),
                    FileSize = table.Column<ulong>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    SourceLocation = table.Column<string>(type: "TEXT", nullable: true),
                    RemoteIP = table.Column<string>(type: "TEXT", nullable: true),
                    DestinationLocation = table.Column<string>(type: "TEXT", nullable: true),
                    Progress = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransferStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TransferEnd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTransferModels", x => x.FileTransferId);
                });

            migrationBuilder.CreateTable(
                name: "SftpConnectionProperties",
                columns: table => new
                {
                    ConnectionId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    HostOsIcon = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    KeyHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UseKeyAuth = table.Column<bool>(type: "INTEGER", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SftpConnectionProperties", x => x.ConnectionId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileTransferModels");

            migrationBuilder.DropTable(
                name: "SftpConnectionProperties");
        }
    }
}
