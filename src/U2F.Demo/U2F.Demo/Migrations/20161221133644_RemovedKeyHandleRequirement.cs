using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace U2F.Demo.Migrations
{
    public partial class RemovedKeyHandleRequirement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KeyHandle",
                table: "AuthenticationRequests",
                nullable: true,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KeyHandle",
                table: "AuthenticationRequests",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
