using Codesanook.BasicUserProfile.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Codesanook.BasicUserProfile {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            // Naming convention for Lambda parameter
            // prefer abbreviation when it is clear about the context 
            // prefer long name when it is hard to reference context

            // For Orchard migration, use:
            // table parameter for CreateTableCommand
            // column parameter for CreateColumnCommand
            // type parameter for ContentTypeDefinitionBuilder
            // part for ContentPartDefinitionBuilder

            // Create UserProfilePartRecord table
            SchemaBuilder.CreateTable(
                nameof(UserProfilePartRecord),
                table => table
                    .ContentPartRecord()
                    .Column<string>(
                        nameof(UserProfilePartRecord.FirstName),
                        column => column.WithLength(64)
                    )
                    .Column<string>(
                        nameof(UserProfilePartRecord.LastName),
                        column => column.WithLength(64)
                    )
                    .Column<string>(
                        nameof(UserProfilePartRecord.MobilePhoneNumber),
                        column => column.WithLength(10)
                    )
                    .Column<string>(
                        nameof(UserProfilePartRecord.OrganizationName),
                        column => column.WithLength(64)
                    )
            );

            // Prepare content part
            ContentDefinitionManager.AlterPartDefinition(
                nameof(UserProfilePart),
                part => part
                    .Attachable()
                    .WithDescription("Provide user profile part")
            );

            // Add UserProfilePart to existing User type
            ContentDefinitionManager.AlterTypeDefinition(
                "User",
                type => type.WithPart(nameof(UserProfilePart))
            );

            return 1;
        }
    }
}