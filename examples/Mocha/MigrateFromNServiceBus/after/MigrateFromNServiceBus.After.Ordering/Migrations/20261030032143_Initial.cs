using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Ordering.Migrations;

/// <inheritdoc />
public partial class _20261030032143_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inbox_messages",
            columns: table => new
            {
                message_id = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                consumer_type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                message_type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("ix_inbox_messages_primary_key", x => new { x.message_id, x.consumer_type });
            });

        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_orders", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                envelope = table.Column<JsonDocument>(type: "json", nullable: false),
                times_sent = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("ix_outbox_messages_primary_key", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "saga_states",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                saga_name = table.Column<string>(type: "text", nullable: false),
                state = table.Column<JsonDocument>(type: "json", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("ix_saga_states_primary_key", x => new { x.id, x.saga_name });
            });

        migrationBuilder.CreateTable(
            name: "scheduled_messages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                envelope = table.Column<JsonDocument>(type: "json", nullable: false),
                scheduled_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                times_sent = table.Column<int>(type: "integer", nullable: false),
                max_attempts = table.Column<int>(type: "integer", nullable: false),
                last_error = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("ix_scheduled_messages_primary_key", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_processed_at",
            table: "inbox_messages",
            column: "processed_at");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_created_at",
            table: "outbox_messages",
            column: "created_at",
            descending: new bool[0]);

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_times_sent",
            table: "outbox_messages",
            column: "times_sent");

        migrationBuilder.CreateIndex(
            name: "ix_saga_states_created_at",
            table: "saga_states",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_scheduled_messages_scheduled_time",
            table: "scheduled_messages",
            column: "scheduled_time",
            filter: "\"times_sent\" < \"max_attempts\"");

        migrationBuilder.CreateIndex(
            name: "ix_scheduled_messages_times_sent",
            table: "scheduled_messages",
            column: "times_sent");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "inbox_messages");

        migrationBuilder.DropTable(
            name: "orders");

        migrationBuilder.DropTable(
            name: "outbox_messages");

        migrationBuilder.DropTable(
            name: "saga_states");

        migrationBuilder.DropTable(
            name: "scheduled_messages");
    }
}
