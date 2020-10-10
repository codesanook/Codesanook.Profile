using Codesanook.Users.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Codesanook.Users {
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
                nameof(BasicUserProfilePartRecord),
                table => table
                    .ContentPartRecord()
                    .Column<string>(
                        nameof(BasicUserProfilePartRecord.FirstName),
                        column => column.WithLength(64)
                    )
                    .Column<string>(
                        nameof(BasicUserProfilePartRecord.LastName),
                        column => column.WithLength(64)
                    )
                    .Column<string>(
                        nameof(BasicUserProfilePartRecord.MobilePhoneNumber),
                        column => column.WithLength(10)
                    )
                    .Column<string>(
                        nameof(BasicUserProfilePartRecord.OrganizationName),
                        column => column.WithLength(64)
                    )
            );

            // Create a content part
            ContentDefinitionManager.AlterPartDefinition(
                nameof(BasicUserProfilePart),
                part => part
                    .Attachable()
                    .WithDescription("Provide a basic user profile part")
            );

            // Let's Add UserProfilePart to existing User type manually from the admin dashboard
            return 1;
        }
    }
}