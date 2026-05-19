using FluentMigrator;

namespace Migrator.V1
{
    [Migration(1, "Users")]
    public class Schema : BaseSchema
    {
        public Schema()
            : base(
            [
                "TABLE_users",
                "FUNC_users_listall",
                "FUNC_users_getbyusername",
                "FUNC_users_insert",
                "FUNC_users_update",
                "FUNC_users_delete",
                "QUERY_users_insertadmin",
            ])
        { }
    }
}
