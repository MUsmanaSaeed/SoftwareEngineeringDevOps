using FluentMigrator;

namespace Migrator.V2
{
    [Migration(2, "Manufacturers")]
    public class Schema : BaseSchema
    {
        public Schema()
            : base(
            [
                "TABLE_manufacturers",
                "FUNC_manufacturers_listall",
                "FUNC_manufacturers_getbyid",
                "FUNC_manufacturers_insert",
                "FUNC_manufacturers_update",
                "FUNC_manufacturers_delete",
            ])
        { }
    }
}
