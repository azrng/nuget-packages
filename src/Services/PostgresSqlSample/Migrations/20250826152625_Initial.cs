using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PostgresSqlSample.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "azrng");

            migrationBuilder.CreateTable(
                name: "test1",
                schema: "azrng",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "主键"),
                    long_test = table.Column<long>(type: "bigint", nullable: false),
                    double_test = table.Column<double>(type: "double precision", nullable: false),
                    decimal_test = table.Column<decimal>(type: "numeric", nullable: false),
                    float_test = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test1", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test2",
                schema: "azrng",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "主键"),
                    creater = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "", comment: "创建人"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "创建时间"),
                    modifyer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "", comment: "最后修改人"),
                    modify_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "最后修改时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test2", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test3",
                schema: "azrng",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "主键"),
                    creater = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "", comment: "创建人"),
                    create_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "创建时间"),
                    modifyer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "", comment: "最后修改人"),
                    modify_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "最后修改时间"),
                    deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否删除"),
                    disabled = table.Column<bool>(type: "boolean", nullable: false, comment: "是否禁用")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test3", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "azrng",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", maxLength: 36, nullable: false, comment: "主键")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "账号"),
                    password = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "密码"),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 50, nullable: false, comment: "创建时间"),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "用户名"),
                    is_valid = table.Column<bool>(type: "boolean", maxLength: 50, nullable: false, comment: "是否有效")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "useraddress",
                schema: "azrng",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", maxLength: 36, nullable: false, comment: "主键")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false, comment: "名称"),
                    address = table.Column<string>(type: "text", nullable: false, comment: "地址"),
                    user_id = table.Column<long>(type: "bigint", nullable: false, comment: "用户Id")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_useraddress", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test1",
                schema: "azrng");

            migrationBuilder.DropTable(
                name: "test2",
                schema: "azrng");

            migrationBuilder.DropTable(
                name: "test3",
                schema: "azrng");

            migrationBuilder.DropTable(
                name: "user",
                schema: "azrng");

            migrationBuilder.DropTable(
                name: "useraddress",
                schema: "azrng");
        }
    }
}
