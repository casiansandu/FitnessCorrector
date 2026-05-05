using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassLibrary1.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutSessionMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkoutSessionMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseSlug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    AverageDepth = table.Column<double>(type: "double precision", nullable: false),
                    AverageTempoSeconds = table.Column<double>(type: "double precision", nullable: false),
                    AverageSymmetry = table.Column<double>(type: "double precision", nullable: false),
                    RepCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessionMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionMetrics_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessionRepMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepIndex = table.Column<int>(type: "integer", nullable: false),
                    Depth = table.Column<double>(type: "double precision", nullable: false),
                    TempoTotalSeconds = table.Column<double>(type: "double precision", nullable: false),
                    TempoEccentricSeconds = table.Column<double>(type: "double precision", nullable: false),
                    TempoConcentricSeconds = table.Column<double>(type: "double precision", nullable: false),
                    Symmetry = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessionRepMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionRepMetrics_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionMetrics_WorkoutSessionId",
                table: "WorkoutSessionMetrics",
                column: "WorkoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionRepMetrics_WorkoutSessionId",
                table: "WorkoutSessionRepMetrics",
                column: "WorkoutSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkoutSessionMetrics");

            migrationBuilder.DropTable(
                name: "WorkoutSessionRepMetrics");
        }
    }
}
