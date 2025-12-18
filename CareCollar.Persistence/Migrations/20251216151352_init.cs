using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareCollar.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    species = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    weight_kg = table.Column<double>(type: "double precision", nullable: false),
                    breed = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pets", x => x.id);
                    table.ForeignKey(
                        name: "fk_pets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "collar_devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_connection = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    battery_level = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collar_devices", x => x.id);
                    table.ForeignKey(
                        name: "fk_collar_devices_pets_pet_id",
                        column: x => x.pet_id,
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "health_thresholds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_type = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    min_value = table.Column<double>(type: "double precision", nullable: true),
                    max_value = table.Column<double>(type: "double precision", nullable: true),
                    threshold_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_health_thresholds", x => x.id);
                    table.ForeignKey(
                        name: "fk_health_thresholds_pets_pet_id",
                        column: x => x.pet_id,
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_data",
                columns: table => new
                {
                    time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    collar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    heart_rate_bpm = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: false),
                    temperature_celsius = table.Column<double>(type: "double precision", nullable: false),
                    gps_latitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: false),
                    gps_longitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: false),
                    activity_index = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_health_data", x => new { x.time, x.collar_id });
                    table.ForeignKey(
                        name: "fk_health_data_collar_devices_collar_id",
                        column: x => x.collar_id,
                        principalTable: "collar_devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.Sql(
                "SELECT create_hypertable('health_data'::regclass, 'time'::text, chunk_time_interval => INTERVAL '7 days');",
                suppressTransaction: true
            );
            
            migrationBuilder.CreateIndex(
                name: "ix_collar_devices_pet_id",
                table: "collar_devices",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "ix_collar_devices_serial_number",
                table: "collar_devices",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_health_data_collar_id",
                table: "health_data",
                column: "collar_id");

            migrationBuilder.CreateIndex(
                name: "ix_health_data_time",
                table: "health_data",
                column: "time");

            migrationBuilder.CreateIndex(
                name: "ix_health_thresholds_pet_id",
                table: "health_thresholds",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "ix_pets_user_id",
                table: "pets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "health_data");

            migrationBuilder.DropTable(
                name: "health_thresholds");

            migrationBuilder.DropTable(
                name: "collar_devices");

            migrationBuilder.DropTable(
                name: "pets");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
