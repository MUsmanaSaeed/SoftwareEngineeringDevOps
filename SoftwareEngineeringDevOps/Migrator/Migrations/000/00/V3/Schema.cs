using FluentMigrator;

namespace Migrator.V3
{
    [Migration(3, "Bricks")]
    public class Schema : BaseSchema
    {
        public Schema()
            : base(
            [
                "TABLE_bricks",
                "FUNC_bricks_listall",
                "FUNC_bricks_getbyid",
                "FUNC_bricks_getbymanufacturerid",
                "FUNC_bricks_insert",
                "FUNC_bricks_update",
                "FUNC_bricks_delete",
            ])
        { }
    }
}
